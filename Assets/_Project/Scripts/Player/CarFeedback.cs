using System;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Level;
using TurretRush.Vfx;
using TurretRush.World;
using VContainer.Unity;

namespace TurretRush.Player
{
    /// <summary>
    /// Turns damage into something the player can feel: the body flashes, and on the
    /// last hit it goes up in an explosion.
    ///
    /// Separate from both the car and the camera because it belongs to neither -
    /// deleting it would leave the game working and silent.
    /// </summary>
    public sealed class CarFeedback : IStartable, IResettable, IDisposable
    {
        private readonly Health _carHealth;
        private readonly CarView _car;
        private readonly CameraRig _camera;
        private readonly VfxSystem _vfx;
        private readonly VfxConfig _config;

        private int _lastSeen;

        public CarFeedback(
            Health carHealth,
            CarView car,
            CameraRig camera,
            VfxSystem vfx,
            VfxConfig config)
        {
            _carHealth = carHealth;
            _car = car;
            _camera = camera;
            _vfx = vfx;
            _config = config;
        }

        public void Start()
        {
            _lastSeen = _carHealth.Current;

            _carHealth.Changed += OnHealthChanged;
            _carHealth.Died += OnDied;
        }

        public void ResetToStart()
        {
            _lastSeen = _carHealth.Current;
            _car.SetVisible(true);
        }

        public void Dispose()
        {
            _carHealth.Changed -= OnHealthChanged;
            _carHealth.Died -= OnDied;
        }

        private void OnHealthChanged(int current, int max)
        {
            // Health also reports going up - that is a restart, not an impact.
            var damaged = current < _lastSeen;
            _lastSeen = current;

            if (!damaged)
                return;

            _car.Flash();
            _camera.Shake(_config.HitShakeStrength, _config.HitShakeDuration);
        }

        private void OnDied()
        {
            // Hidden, not destroyed: the same car drives the next attempt. Died fires
            // exactly once per life, so no already-exploded flag is needed.
            _car.SetVisible(false);
            _vfx.PlayCarExplosion(_car.Root.position);
            _camera.Shake(_config.ExplosionShakeStrength, _config.ExplosionShakeDuration);
        }
    }
}
