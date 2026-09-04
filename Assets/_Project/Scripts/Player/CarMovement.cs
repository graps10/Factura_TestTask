using TurretRush.Config;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Player
{
    /// <summary>
    /// Drives the car forward and places it on the drift curve. This is the single
    /// source of truth for how far into the level we are - the ground streamer, the
    /// camera, the progress bar and the enemy spawner all read
    /// <see cref="DistanceTravelled"/> rather than measuring the transform again.
    /// </summary>
    public sealed class CarMovement : IStartable, ITickable
    {
        private readonly CarView _view;
        private readonly CarConfig _config;
        private readonly LateralDrift _drift;

        public CarMovement(CarView view, CarConfig config, LevelConfig level)
        {
            _view = view;
            _config = config;

            // The configured amplitude is a wish; the road width is the law.
            _drift = new LateralDrift(
                Mathf.Min(config.DriftAmplitude, level.DrivableHalfWidth),
                config.DriftFrequency,
                config.DriftPhase);
        }

        public bool IsRunning { get; private set; }

        public float DistanceTravelled { get; private set; }

        public float Speed { get; private set; }

        public void Begin() => IsRunning = true;

        public void Halt() => IsRunning = false;

        public void ResetToStart()
        {
            IsRunning = false;
            Speed = 0f;
            DistanceTravelled = 0f;
            ApplyTransform();
        }

        // Placing the car is a scene side effect, so it happens here and not in the
        // constructor. Registered first in GameLifetimeScope, which makes this the
        // first entry point to run - everything downstream sees a positioned car.
        public void Start() => ResetToStart();

        public void Tick() => Step(Time.deltaTime);

        private void Step(float deltaTime)
        {
            var targetSpeed = IsRunning ? _config.Speed : 0f;
            var rate = targetSpeed > Speed ? _config.Acceleration : _config.Braking;
            Speed = Mathf.MoveTowards(Speed, targetSpeed, rate * deltaTime);

            if (Speed <= Mathf.Epsilon)
                return;

            var delta = Speed * deltaTime;
            DistanceTravelled += delta;

            ApplyTransform();
            _view.SpinWheels(delta);
        }

        private void ApplyTransform()
        {
            var lateral = _drift.Evaluate(DistanceTravelled);
            var heading = _drift.HeadingDegrees(DistanceTravelled, _config.HeadingLookAhead);

            _view.Root.SetPositionAndRotation(
                new Vector3(lateral, 0f, DistanceTravelled),
                Quaternion.Euler(0f, heading, 0f));
        }
    }
}
