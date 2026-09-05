using System;
using System.Collections.Generic;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Level;
using TurretRush.Player;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace TurretRush.Enemies
{
    /// <summary>
    /// Owns the whole enemy population: the plan, the pool, and the behaviour of
    /// every enemy currently on screen. One system stepping a list rather than a
    /// MonoBehaviour per enemy, for the same reasons as the projectiles - and here
    /// it also keeps the aggro rule in one readable place instead of scattered
    /// across forty instances.
    /// </summary>
    public sealed class EnemySystem : IStartable, ITickable, IResettable, IDisposable
    {
        private readonly EnemyConfig _config;
        private readonly LevelConfig _level;
        private readonly CarView _car;
        private readonly Health _carHealth;

        private readonly List<LiveEnemy> _live = new();

        private IReadOnlyList<SpawnPoint> _plan;
        private int _cursor;
        private ObjectPool<EnemyView> _pool;
        private Transform _root;

        public EnemySystem(EnemyConfig config, LevelConfig level, CarView car, Health carHealth)
        {
            _config = config;
            _level = level;
            _car = car;
            _carHealth = carHealth;
        }

        /// <summary>Raised once per enemy shot dead, with where it fell. The death
        /// particles, the coin counter and the kill tally all hang off this.</summary>
        public event Action<Vector3> Killed;

        /// <summary>Where the car was struck and for how much, for the damage number.
        /// Raised here because this is the only place that knows where the enemy was
        /// standing when it connected.</summary>
        public event Action<Vector3, int> CarHit;

        public int AliveCount => _live.Count;

        /// <summary>
        /// What an overlay needs to draw one enemy, without handing out the mutable
        /// entry behind it. Index based rather than an enumerable so a per-frame walk
        /// over every live enemy allocates nothing.
        /// </summary>
        public EnemySnapshot GetLive(int index)
        {
            var enemy = _live[index];
            return new EnemySnapshot(enemy.Position, enemy.Health.Normalized);
        }

        public int KillCount { get; private set; }

        /// <summary>
        /// Enemies are placed on the road as soon as the level is reset, so the
        /// player sees them waiting during the intro, but they do not think until the
        /// level actually starts. Without this they would keep charging through the
        /// result screen and land hits on a car whose fate is already decided.
        /// </summary>
        public bool IsRunning { get; private set; }

        public void Begin() => IsRunning = true;

        public void Halt()
        {
            IsRunning = false;

            // Speed is pushed to the animator from Tick, so stopping Tick leaves the
            // blend tree on whatever it last heard - a charging enemy would run on the
            // spot for as long as the result screen is up. Nothing will speak to the
            // animator again, so the last word has to be the right one.
            for (var i = 0; i < _live.Count; i++)
                _live[i].View.SetSpeed(0f);
        }

        public void Start()
        {
            _root = new GameObject("Enemies").transform;
            _pool = new ObjectPool<EnemyView>(
                createFunc: () => Object.Instantiate(_config.Prefab, _root),
                actionOnGet: view => view.gameObject.SetActive(true),
                actionOnRelease: view =>
                {
                    if (view != null)
                        view.gameObject.SetActive(false);
                },
                actionOnDestroy: view =>
                {
                    if (view != null)
                        Object.Destroy(view.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: _config.PoolCapacity,
                maxSize: _config.PoolMaxSize);

            BuildPlan();
        }

        public void Tick()
        {
            if (!IsRunning)
                return;

            var deltaTime = Time.deltaTime;
            var carPosition = _car.Root.position;

            ActivateAhead(carPosition.z);

            for (var i = _live.Count - 1; i >= 0; i--)
                Step(_live[i], i, deltaTime, carPosition);
        }

        public void ResetToStart()
        {
            IsRunning = false;

            for (var i = _live.Count - 1; i >= 0; i--)
                Release(i);

            KillCount = 0;
            BuildPlan();

            // One activation pass while stopped, so the opening shot of the level
            // already has enemies standing on the road ahead instead of them popping
            // in the moment the car starts moving.
            ActivateAhead(_car.Root.position.z);
        }

        public void Dispose()
        {
            _pool?.Dispose();

            if (_root != null)
                Object.Destroy(_root.gameObject);
        }

        private void BuildPlan()
        {
            var planner = new EnemySpawnPlanner(
                _config.SafeStartDistance,
                Mathf.Max(_config.SafeStartDistance + 1f, _level.Length - _config.FinishMargin),
                _level.DrivableHalfWidth,
                _config.Count,
                _config.SpacingMargin);

            _plan = planner.Plan(_config.Seed);
            _cursor = 0;
        }

        private void ActivateAhead(float carZ)
        {
            // The plan is sorted by Z, so this walks forward once over the level
            // instead of scanning every remaining spawn point every frame.
            var horizon = carZ + _config.ActivationLead;

            while (_cursor < _plan.Count && _plan[_cursor].Z <= horizon)
            {
                Spawn(_plan[_cursor], _cursor);
                _cursor++;
            }
        }

        private void Spawn(SpawnPoint point, int index)
        {
            var view = _pool.Get();
            var position = point.ToPosition();

            view.PrepareForSpawn(position);

            var health = new Health(_config.MaxHealth);
            view.Bind(health);

            _live.Add(new LiveEnemy
            {
                View = view,
                Health = health,
                Anchor = position,
                Position = position,
                // Derived from the index so a given enemy wanders the same way every
                // attempt, keeping the whole level reproducible.
                Phase = index * 1.37f
            });
        }

        private void Step(LiveEnemy enemy, int index, float deltaTime, Vector3 carPosition)
        {
            if (!enemy.Health.IsAlive)
            {
                Kill(index, enemy.Position);
                return;
            }

            if (carPosition.z - enemy.Position.z > _config.DespawnBehind)
            {
                Release(index);
                return;
            }

            enemy.Age += deltaTime;

            if (!enemy.IsCharging && IsCarWithinReach(enemy, carPosition))
                enemy.IsCharging = true;

            var previous = enemy.Position;
            var next = enemy.IsCharging
                ? Charge(enemy, deltaTime, carPosition)
                : Wander(enemy);

            enemy.Position = next;
            enemy.View.MoveTo(next);
            enemy.View.FaceDirection(next - previous, _config.TurnSpeed, deltaTime);

            // Speed is measured rather than assumed, so the blend tree reacts to what
            // the body is actually doing - the idle shuffle drives the walk, the
            // charge drives the run, and neither needs its own animator flag.
            enemy.View.SetSpeed(deltaTime > 0f ? Vector3.Distance(previous, next) / deltaTime : 0f);

            if (enemy.IsCharging && _car.Overlaps(next, _config.ImpactMargin))
            {
                // The enemy spends itself on the hit. That is the whole melee rule -
                // no attack cooldown, no re-engage, no enemy stuck grinding against
                // the bumper for the rest of the level.
                _carHealth.TakeDamage(_config.DamageToCar);
                CarHit?.Invoke(next, _config.DamageToCar);
                Release(index);
            }
        }

        private bool IsCarWithinReach(LiveEnemy enemy, Vector3 carPosition)
        {
            var dx = carPosition.x - enemy.Position.x;
            var dz = carPosition.z - enemy.Position.z;

            return dx * dx + dz * dz <= _config.DetectRadius * _config.DetectRadius;
        }

        private Vector3 Wander(LiveEnemy enemy)
        {
            // Two sines at unrelated rates: a small, smooth, endlessly varied shuffle
            // around the spawn point that never needs a destination or a timer, and
            // that replays identically because it is a function of age and phase.
            var t = enemy.Age * _config.WanderFrequency;

            return enemy.Anchor + new Vector3(
                Mathf.Sin(t + enemy.Phase),
                0f,
                Mathf.Sin(t * 0.73f + enemy.Phase * 1.9f)) * _config.WanderRadius;
        }

        private Vector3 Charge(LiveEnemy enemy, float deltaTime, Vector3 carPosition)
        {
            var toCar = carPosition - enemy.Position;
            toCar.y = 0f;

            if (toCar.sqrMagnitude < 0.0001f)
                return enemy.Position;

            return enemy.Position + toCar.normalized * (_config.RunSpeed * deltaTime);
        }

        private void Kill(int index, Vector3 position)
        {
            KillCount++;
            Release(index);
            Killed?.Invoke(position);
        }

        private void Release(int index)
        {
            var enemy = _live[index];

            enemy.View.Unbind();
            _pool.Release(enemy.View);
            _live.RemoveAt(index);
        }

        public readonly struct EnemySnapshot
        {
            public readonly Vector3 Position;
            public readonly float NormalizedHealth;

            public EnemySnapshot(Vector3 position, float normalizedHealth)
            {
                Position = position;
                NormalizedHealth = normalizedHealth;
            }
        }

        private sealed class LiveEnemy
        {
            public EnemyView View;
            public Health Health;
            public Vector3 Anchor;
            public Vector3 Position;
            public float Phase;
            public float Age;
            public bool IsCharging;
        }
    }
}
