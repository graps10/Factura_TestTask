using System;
using System.Collections.Generic;
using TurretRush.Config;
using TurretRush.Enemies;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace TurretRush.Vfx
{
    /// <summary>
    /// Plays and recycles one-shot particle bursts.
    /// </summary>
    public sealed class VfxSystem : IStartable, ITickable, IDisposable
    {
        private readonly VfxConfig _config;
        private readonly EnemySystem _enemies;
        private readonly List<Playing> _playing = new();

        private ObjectPool<ParticleSystem> _deathPool;
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

                _deathPool.Release(effect.System);
                _playing.RemoveAt(i);
            }
        }

        public void PlayEnemyDeath(Vector3 position)
        {
            if (_config.EnemyDeathPrefab == null)
                return;

            var system = _deathPool.Get();
            system.transform.position = position;

            // Clearing first drops any particles still alive from the previous use,
            // which would otherwise appear at the new position for one frame.
            system.Clear(true);
            system.Play(true);

            _playing.Add(new Playing
            {
                System = system,
                Remaining = LifetimeOf(system)
            });
        }

        public void StopAll()
        {
            for (var i = _playing.Count - 1; i >= 0; i--)
                _deathPool.Release(_playing[i].System);

            _playing.Clear();
        }

        public void Dispose()
        {
            _enemies.Killed -= PlayEnemyDeath;
            _deathPool?.Dispose();

            if (_root != null)
                Object.Destroy(_root.gameObject);
        }

        /// <summary>
        /// Taken from the particle system itself rather than from a field in the
        /// config, so retiming the effect in the editor cannot leave the pool
        /// recycling it while it is still visible.
        /// </summary>
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
            public float Remaining;
        }
    }
}
