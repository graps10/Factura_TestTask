using System;
using System.Collections.Generic;
using TurretRush.Config;
using TurretRush.Level;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace TurretRush.Combat
{
    /// <summary>
    /// Owns every bullet in flight. One system over a flat list rather than a
    /// MonoBehaviour per bullet, so a hundred rounds in the air cost one Update.
    /// </summary>
    public sealed class ProjectileSystem : IStartable, ITickable, IResettable, IDisposable
    {
        private readonly WeaponConfig _config;
        private readonly List<Shot> _live = new();

        private ObjectPool<ProjectileView> _pool;
        private Transform _root;

        public ProjectileSystem(WeaponConfig config) => _config = config;

        /// <summary>Where a shot landed and for how much. Raised here because this is
        /// the only place that knows the point of impact.</summary>
        public event Action<Vector3, int> Hit;

        public int LiveCount => _live.Count;

        public void Start()
        {
            _root = new GameObject("Projectiles").transform;
            _pool = new ObjectPool<ProjectileView>(
                createFunc: () => Object.Instantiate(_config.ProjectilePrefab, _root),
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
        }

        public void Spawn(Vector3 origin, Vector3 direction)
        {
            var view = _pool.Get();
            view.PrepareForLaunch(origin, direction);

            _live.Add(new Shot
            {
                View = view,
                Position = origin,
                Direction = direction.normalized,
                Travelled = 0f
            });
        }

        public void Tick()
        {
            var step = _config.Speed * Time.deltaTime;
            if (step <= 0f)
                return;

            // Backwards so removing the current entry cannot skip the next one.
            for (var i = _live.Count - 1; i >= 0; i--)
            {
                var shot = _live[i];

                // Swept from this frame's position to the next. At 90 m/s a bullet
                // covers 1.5 m per frame against a body half a metre thick, so testing
                // the point it lands on would pass straight through.
                if (Physics.SphereCast(
                        shot.Position,
                        _config.Radius,
                        shot.Direction,
                        out var hit,
                        step,
                        _config.HitMask,
                        QueryTriggerInteraction.Collide))
                {
                    var damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(_config.Damage);

                        // Only while the target still stands. A kill already shows the
                        // body drop, the debris and the payout; a number on top of that
                        // lands on the coins.
                        if (damageable.IsAlive)
                            Hit?.Invoke(hit.point, _config.Damage);
                    }

                    Despawn(i);
                    continue;
                }

                shot.Position += shot.Direction * step;
                shot.Travelled += step;

                if (shot.Travelled >= _config.MaxRange)
                {
                    Despawn(i);
                    continue;
                }

                shot.View.transform.position = shot.Position;
                _live[i] = shot;
            }
        }

        public void ResetToStart()
        {
            for (var i = _live.Count - 1; i >= 0; i--)
                Despawn(i);
        }

        public void Dispose()
        {
            _pool?.Dispose();

            if (_root != null)
                Object.Destroy(_root.gameObject);
        }

        private void Despawn(int index)
        {
            _pool.Release(_live[index].View);
            _live.RemoveAt(index);
        }

        private struct Shot
        {
            public ProjectileView View;
            public Vector3 Position;
            public Vector3 Direction;
            public float Travelled;
        }
    }
}
