using TurretRush.Config;
using TurretRush.Level;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Player
{
    /// <summary>
    /// Drives the car forward along the drift curve, and is the single source of
    /// truth for how far into the level it is: the streamer, the camera, the progress
    /// bar and the spawner all read <see cref="DistanceTravelled"/>.
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

        // Not in the constructor: placing the car is a scene side effect. Registered
        // first, so everything downstream starts with a positioned car.
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

            // Outside the speed check: the body still has to settle level and the
            // exhaust to die down while the car brakes.
            UpdateBodywork(deltaTime);
        }

        private void UpdateBodywork(float deltaTime)
        {
            var speedFactor = _config.Speed <= 0f ? 0f : Speed / _config.Speed;
            var heading = _drift.HeadingDegrees(DistanceTravelled, _config.HeadingLookAhead);

            // Scaled by speed, because roll comes from cornering: the start line sits
            // six degrees into the first bend, so without this a parked car leans while
            // it waits. Damped as well, so weight takes a moment to transfer.
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
