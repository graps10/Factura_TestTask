using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TurretRush.Config;
using TurretRush.Player;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.World
{
    /// <summary>
    /// Chase camera. Damped follow with a fixed world rotation - inheriting the car's
    /// yaw would swing the whole screen on every drift.
    ///
    /// ILateTickable, so it moves after the car has been placed this frame rather than
    /// rendering one frame behind it.
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
        private bool _following;

        public CameraRig(Camera camera, CarView car, CameraConfig config)
        {
            _camera = camera.transform;
            _car = car;
            _config = config;
        }

        public void Start() => SnapToIntro();

        public void LateTick()
        {
            // The intro tween owns the transform; two writers and the last one wins.
            if (!_following)
                return;

            var deltaTime = Time.deltaTime;

            var followed = Vector3.SmoothDamp(
                _camera.position,
                TargetPosition(),
                ref _velocity,
                _config.SmoothTime);

            // Added on top of the damped position rather than tweened onto the
            // transform, which the follow is already writing to.
            _camera.position = followed + EvaluateShake(deltaTime);
        }

        public void Shake(float strength, float duration)
        {
            if (strength <= 0f || duration <= 0f)
                return;

            // Restarts rather than stacks, so a crowd cannot throw the camera off.
            _shakeStrength = Mathf.Max(_shakeStrength * (_shakeRemaining > 0f ? 0.5f : 0f), strength);
            _shakeDuration = duration;
            _shakeRemaining = duration;
        }

        /// <summary>Jumps straight to the showcase pose behind the parked car.</summary>
        public void SnapToIntro()
        {
            _camera.DOKill();
            _following = false;
            _velocity = Vector3.zero;
            _shakeRemaining = 0f;
            _shakeStrength = 0f;

            _camera.SetPositionAndRotation(IntroPosition(), Quaternion.Euler(_config.IntroEulerAngles));
        }

        /// <summary>Pulls back into the gameplay pose and hands the transform back to
        /// the follow. Awaited, so the hint cannot appear over a moving camera.</summary>
        public async UniTask PlayIntroAsync(CancellationToken cancellationToken)
        {
            _camera.DOKill();

            // Two tweens on the same transform rather than a sequence, so DOKill on
            // that transform reaches both - a DOTween.Sequence has no target.
            _camera.DOMove(TargetPosition(), _config.IntroDuration).SetEase(_config.IntroEase);

            await _camera.DORotate(_config.EulerAngles, _config.IntroDuration)
                .SetEase(_config.IntroEase)
                .ToUniTask(cancellationToken: cancellationToken);

            // Only reached on completion; a cancelled tween throws instead.
            _velocity = Vector3.zero;
            _following = true;
        }

        private Vector3 TargetPosition() => PoseAround(_config.Offset);

        private Vector3 IntroPosition() => PoseAround(_config.IntroOffset);

        private Vector3 PoseAround(Vector3 offset)
        {
            var car = _car.Root.position;

            return new Vector3(
                car.x * _config.LateralFollow + offset.x,
                offset.y,
                car.z + offset.z);
        }

        private Vector3 EvaluateShake(float deltaTime)
        {
            if (_shakeRemaining <= 0f)
                return Vector3.zero;

            _shakeRemaining -= deltaTime;
            var falloff = Mathf.Clamp01(_shakeRemaining / _shakeDuration);

            // Perlin, not Random: consecutive frames stay related, so it reads as a
            // knock rather than as a rendering fault.
            var t = Time.time * _config.ShakeFrequency;
            var amplitude = _shakeStrength * falloff;

            return new Vector3(
                (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * amplitude,
                (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * amplitude,
                0f);
        }
    }
}
