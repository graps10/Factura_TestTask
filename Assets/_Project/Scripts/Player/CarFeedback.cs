using System;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.World;
using VContainer.Unity;

namespace TurretRush.Player
{
    /// <summary>
    /// Turns damage to the car into something the player can feel: the body flashes
    /// and the camera takes a knock.
    /// </summary>
    public sealed class CarFeedback : IStartable, IDisposable
    {
        private readonly Health _carHealth;
        private readonly CarView _car;
        private readonly CameraRig _camera;
        private readonly VfxConfig _config;

        private int _lastSeen;

        public CarFeedback(Health carHealth, CarView car, CameraRig camera, VfxConfig config)
        {
            _carHealth = carHealth;
            _car = car;
            _camera = camera;
            _config = config;
        }

        public void Start()
        {
            _lastSeen = _carHealth.Current;
            _carHealth.Changed += OnHealthChanged;
        }

        public void Dispose() => _carHealth.Changed -= OnHealthChanged;

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
    }
}
