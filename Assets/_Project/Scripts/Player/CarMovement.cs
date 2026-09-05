using TurretRush.Config;
using TurretRush.Level;
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
    public sealed class CarMovement : IStartable, ITickable, IResettable
    {
        private readonly CarView _view;
        private readonly CarConfig _config;
        private readonly LateralDrift _drift;

        private float _bank;
        private float _bankVelocity;

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
            _bank = 0f;
            _bankVelocity = 0f;

            ApplyTransform();
            _view.SetBank(0f);
            _view.SetExhaust(0f);
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

            if (Speed > Mathf.Epsilon)
            {
                var delta = Speed * deltaTime;
                DistanceTravelled += delta;

                ApplyTransform();
                _view.SpinWheels(delta);
            }

            // Outside the speed check on purpose: the body has to settle back level
            // and the exhaust has to die down while the car is braking to a stop.
            UpdateBodywork(deltaTime);
        }

        private void UpdateBodywork(float deltaTime)
        {
            var speedFactor = _config.Speed <= 0f ? 0f : Speed / _config.Speed;
            var heading = _drift.HeadingDegrees(DistanceTravelled, _config.HeadingLookAhead);

            // Scaled by speed, because roll comes from cornering and a parked car does
            // not corner. Without it the start line is already six degrees into the
            // first bend, and the car sits leaning while it waits for the player.
            //
            // Damped rather than driven straight from the heading, so the body lags the
            // steering slightly - weight taking a moment to transfer, rather than the
            // whole car snapping to a new angle.
            _bank = Mathf.SmoothDamp(
                _bank,
                heading * _config.BankPerHeadingDegree * speedFactor,
                ref _bankVelocity,
                _config.BankSmoothTime,
                Mathf.Infinity,
                deltaTime);

            _view.SetBank(_bank);
            _view.SetExhaust(speedFactor);
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
