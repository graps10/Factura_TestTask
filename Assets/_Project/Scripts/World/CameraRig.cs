using TurretRush.Config;
using TurretRush.Player;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.World
{
    /// <summary>
    /// Chase camera. Follows the car's position with damping but keeps a fixed world
    /// rotation - inheriting the car's yaw would swing the entire screen every time
    /// the car drifts, which reads as motion sickness rather than speed.
    ///
    /// Runs as ILateTickable so it moves after the car has already been placed this
    /// frame; doing it in Tick would always render one frame behind.
    /// </summary>
    public sealed class CameraRig : IStartable, ILateTickable
    {
        private readonly Transform _camera;
        private readonly CarView _car;
        private readonly CameraConfig _config;

        private Vector3 _velocity;
        private float _shakeStrength;
        private float _shakeRemaining;
        private float _shakeDuration;

        public CameraRig(Camera camera, CarView car, CameraConfig config)
        {
            _camera = camera.transform;
            _car = car;
            _config = config;
        }

        public void Start() => SnapToTarget();

        public void LateTick()
        {
            var deltaTime = Time.deltaTime;

            var followed = Vector3.SmoothDamp(
                _camera.position,
                TargetPosition(),
                ref _velocity,
                _config.SmoothTime);

            // The shake is added on top of the damped position instead of being
            // tweened onto the transform. A DOTween shake would be writing to the
            // same transform the follow writes to, and whichever ran last would win.
            _camera.position = followed + EvaluateShake(deltaTime);
        }

        public void Shake(float strength, float duration)
        {
            if (strength <= 0f || duration <= 0f)
                return;

            // A second hit part way through the first restarts rather than stacks,
            // so a crowd arriving at once cannot throw the camera across the screen.
            _shakeStrength = Mathf.Max(_shakeStrength * (_shakeRemaining > 0f ? 0.5f : 0f), strength);
            _shakeDuration = duration;
            _shakeRemaining = duration;
        }

        /// <summary>Jumps straight to the target with no easing. Used on level restart
        /// so the camera does not sweep across the whole track.</summary>
        public void SnapToTarget()
        {
            _velocity = Vector3.zero;
            _shakeRemaining = 0f;
            _shakeStrength = 0f;

            _camera.SetPositionAndRotation(TargetPosition(), Quaternion.Euler(_config.EulerAngles));
        }

        private Vector3 TargetPosition()
        {
            var car = _car.Root.position;

            return new Vector3(
                car.x * _config.LateralFollow + _config.Offset.x,
                _config.Offset.y,
                car.z + _config.Offset.z);
        }

        private Vector3 EvaluateShake(float deltaTime)
        {
            if (_shakeRemaining <= 0f)
                return Vector3.zero;

            _shakeRemaining -= deltaTime;
            var falloff = Mathf.Clamp01(_shakeRemaining / _shakeDuration);

            // Perlin rather than Random: consecutive frames stay related, so this
            // reads as the camera being knocked about. Per-frame random values look
            // like a rendering fault instead.
            var t = Time.time * _config.ShakeFrequency;
            var amplitude = _shakeStrength * falloff;

            return new Vector3(
                (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * amplitude,
                (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * amplitude,
                0f);
        }
    }
}
