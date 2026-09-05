using System;
using System.Collections.Generic;
using TurretRush.Config;
using TurretRush.Enemies;
using TurretRush.Level;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace TurretRush.Vfx
{
    /// <summary>
    /// Plays and recycles one-shot particle bursts.
    ///
    /// Listens to EnemySystem.Killed rather than to each enemy's own death: one
    /// subscription for the life of the game cannot leak, forty cycling through a pool
    /// very much can.
    /// </summary>
    public sealed class VfxSystem : IStartable, ITickable, IResettable, IDisposable
    {
        private readonly VfxConfig _config;
        private readonly EnemySystem _enemies;
        private readonly List<Playing> _playing = new();

        private ObjectPool<ParticleSystem> _deathPool;
        private ObjectPool<ParticleSystem> _explosionPool;
        private Transform _root;

        public VfxSystem(VfxConfig config, EnemySystem enemies)
        {
            _config = config;
            _enemies = enemies;
        }

        public void Start()
        {
            _root = new GameObject("Vfx").transform;

            _deathPool = CreatePool(_config.EnemyDeathPrefab);
            _explosionPool = CreatePool(_config.CarExplosionPrefab);

            _enemies.Killed += PlayEnemyDeath;
        }

        public void Tick()
        {
            var deltaTime = Time.deltaTime;

            for (var i = _playing.Count - 1; i >= 0; i--)
            {
                var effect = _playing[i];
                effect.Remaining -= deltaTime;

                if (effect.Remaining > 0f)
                {
                    _playing[i] = effect;
                    continue;
                }

                effect.Pool.Release(effect.System);
                _playing.RemoveAt(i);
            }
        }

        public void PlayEnemyDeath(Vector3 position)
            => Play(_deathPool, _config.EnemyDeathPrefab, position);

        public void PlayCarExplosion(Vector3 position)
            => Play(_explosionPool, _config.CarExplosionPrefab, position);

        public void ResetToStart()
        {
            for (var i = _playing.Count - 1; i >= 0; i--)
                _playing[i].Pool.Release(_playing[i].System);

            _playing.Clear();
        }

        public void Dispose()
        {
            _enemies.Killed -= PlayEnemyDeath;

            _deathPool?.Dispose();
            _explosionPool?.Dispose();

            if (_root != null)
                Object.Destroy(_root.gameObject);
        }

        /// <summary>Each effect remembers its own pool, so one list and one timer
        /// serve every kind of burst.</summary>
        private void Play(ObjectPool<ParticleSystem> pool, ParticleSystem prefab, Vector3 position)
        {
            if (prefab == null)
                return;

            var system = pool.Get();
            system.transform.position = position;

            // Drops particles left from the previous use, which would otherwise show
            // at the new position for a frame.
            system.Clear(true);
            system.Play(true);

            _playing.Add(new Playing
            {
                System = system,
                Pool = pool,
                Remaining = LifetimeOf(system)
            });
        }

        /// <summary>From the effect itself, not a config field, so retiming it in the
        /// editor cannot leave the pool reclaiming it while it is still visible.</summary>
        private static float LifetimeOf(ParticleSystem system)
        {
            var main = system.main;
            return main.duration + main.startLifetime.constantMax;
        }

        private ObjectPool<ParticleSystem> CreatePool(ParticleSystem prefab)
            => new(
                createFunc: () => Object.Instantiate(prefab, _root),
                actionOnGet: system => system.gameObject.SetActive(true),
                actionOnRelease: system =>
                {
                    if (system != null)
                        system.gameObject.SetActive(false);
                },
                actionOnDestroy: system =>
                {
                    if (system != null)
                        Object.Destroy(system.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: _config.PoolCapacity,
                maxSize: _config.PoolMaxSize);

        private struct Playing
        {
            public ParticleSystem System;
            public ObjectPool<ParticleSystem> Pool;
            public float Remaining;
        }
    }
}
