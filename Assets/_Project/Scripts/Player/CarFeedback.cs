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
    /// Turns damage to the car into something the player can feel: the body flashes
    /// on a hit, and on the last one it disappears in an explosion.
    ///
    /// Kept apart from the car and the camera because it belongs to neither. The car
    /// does not need to know a camera exists, and the camera does not need to know
    /// what hit points are; this is the one place that joins them, and deleting it
    /// would leave the game working and silent.
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
            // Health also reports going up, which is what a level restart looks like.
            // Only a drop is an impact.
            var damaged = current < _lastSeen;
            _lastSeen = current;

            if (!damaged)
                return;

            _car.Flash();
            _camera.Shake(_config.HitShakeStrength, _config.HitShakeDuration);
        }

        private void OnDied()
        {
            // Hidden rather than destroyed: the same car drives the next attempt, and
            // Health guarantees this fires exactly once per life, so there is no
            // second explosion to guard against.
            _car.SetVisible(false);
            _vfx.PlayCarExplosion(_car.Root.position);
            _camera.Shake(_config.ExplosionShakeStrength, _config.ExplosionShakeDuration);
        }
    }
}
