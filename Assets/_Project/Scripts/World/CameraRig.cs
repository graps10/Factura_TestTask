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

        public CameraRig(Camera camera, CarView car, CameraConfig config)
        {
            _camera = camera.transform;
            _car = car;
            _config = config;
        }

        public void Start() => SnapToTarget();

        public void LateTick()
        {
            _camera.position = Vector3.SmoothDamp(
                _camera.position,
                TargetPosition(),
                ref _velocity,
                _config.SmoothTime);
        }

        /// <summary>Jumps straight to the target with no easing. Used on level restart
        /// so the camera does not sweep across the whole track.</summary>
        public void SnapToTarget()
        {
            _velocity = Vector3.zero;
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
    }
}
