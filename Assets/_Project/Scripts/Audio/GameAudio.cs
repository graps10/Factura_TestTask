using System;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Enemies;
using TurretRush.Level;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TurretRush.Audio
{
    public sealed class GameAudio : IStartable, IDisposable
    {
        private readonly AudioConfig _config;
        private readonly Weapon _weapon;
        private readonly ProjectileSystem _projectiles;
        private readonly EnemySystem _enemies;
        private readonly Health _carHealth;

        private AudioSource[] _voices;
        private GameObject _root;
        private int _next;

        public GameAudio(
            AudioConfig config,
            Weapon weapon,
            ProjectileSystem projectiles,
            EnemySystem enemies,
            Health carHealth)
        {
            _config = config;
            _weapon = weapon;
            _projectiles = projectiles;
            _enemies = enemies;
            _carHealth = carHealth;
        }

        public void Start()
        {
            _root = new GameObject("Audio");
            _voices = new AudioSource[_config.VoiceCount];

            for (var i = 0; i < _voices.Length; i++)
            {
                var voice = _root.AddComponent<AudioSource>();
                voice.playOnAwake = false;

                // Flat 2D. Everything happens in a narrow cone in front of a camera
                // that never turns, so panning would buy little while rolloff would
                // make the shot wander in volume - and it fires four times a second.
                voice.spatialBlend = 0f;

                _voices[i] = voice;
            }

            _weapon.Fired += OnShotFired;
            _projectiles.Hit += OnEnemyHit;
            _enemies.Killed += OnEnemyKilled;
            _enemies.CarHit += OnCarHit;
            _carHealth.Died += OnCarDestroyed;
        }

        public void PlayResult(LevelResult result)
            => Play(result == LevelResult.Win ? _config.Win : null);

        public void Dispose()
        {
            _weapon.Fired -= OnShotFired;
            _projectiles.Hit -= OnEnemyHit;
            _enemies.Killed -= OnEnemyKilled;
            _enemies.CarHit -= OnCarHit;
            _carHealth.Died -= OnCarDestroyed;

            if (_root != null)
                Object.Destroy(_root);
        }

        private void OnShotFired() => Play(_config.Shot);

        // The projectile only reports non-lethal hits, so a kill is never two sounds.
        private void OnEnemyHit(Vector3 point, int damage) => Play(_config.EnemyHit);

        private void OnEnemyKilled(Vector3 position) => Play(_config.EnemyKilled);

        private void OnCarHit(Vector3 point, int damage) => Play(_config.CarHit);

        private void OnCarDestroyed() => Play(_config.CarDestroyed);

        private void Play(SfxCue cue)
        {
            if (cue?.Clip == null || _voices == null)
                return;

            var voice = _voices[_next];
            _next = (_next + 1) % _voices.Length;

            voice.clip = cue.Clip;
            voice.volume = cue.Volume;
            voice.pitch = 1f + Random.Range(-cue.PitchVariance, cue.PitchVariance);
            voice.Play();
        }
    }
}
