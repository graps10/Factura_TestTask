using UnityEngine;

namespace TurretRush.Player
{
    /// <summary>
    /// Where the barrel points, in world yaw degrees, zero being straight down the
    /// road. World rather than car-local: the car weaves as it drives, and a barrel
    /// inheriting that rotation would wander away from wherever the player left it.
    /// </summary>
    public sealed class TurretAimState
    {
        private readonly float _maxYaw;
        private readonly float _degreesPerScreenWidth;
        private readonly float _smoothTime;

        private float _velocity;

        public TurretAimState(float maxYaw, float degreesPerScreenWidth, float smoothTime)
        {
            _maxYaw = Mathf.Abs(maxYaw);
            _degreesPerScreenWidth = degreesPerScreenWidth;
            _smoothTime = Mathf.Max(0f, smoothTime);
        }

        /// <summary>Where the player has asked the barrel to go.</summary>
        public float TargetYaw { get; private set; }

        /// <summary>Where the barrel actually is, trailing the target by the smoothing.</summary>
        public float CurrentYaw { get; private set; }

        /// <param name="normalizedDragX">Horizontal drag as a fraction of screen width.</param>
        public void AddInput(float normalizedDragX)
        {
            TargetYaw = Mathf.Clamp(
                TargetYaw + normalizedDragX * _degreesPerScreenWidth,
                -_maxYaw,
                _maxYaw);
        }

        public void Step(float deltaTime)
        {
            if (_smoothTime <= 0f)
            {
                CurrentYaw = TargetYaw;
                _velocity = 0f;
                return;
            }

            CurrentYaw = Mathf.SmoothDamp(
                CurrentYaw,
                TargetYaw,
                ref _velocity,
                _smoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        public void Reset()
        {
            TargetYaw = 0f;
            CurrentYaw = 0f;
            _velocity = 0f;
        }
    }
}
