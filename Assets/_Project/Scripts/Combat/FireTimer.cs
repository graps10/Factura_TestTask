using System;

namespace TurretRush.Combat
{
    /// <summary>Decides when the turret is due to fire.</summary>
    public sealed class FireTimer
    {
        private readonly float _interval;
        private readonly float _initialDelay;

        private float _elapsed;
        private float _nextShotAt;

        public FireTimer(float interval, float initialDelay)
        {
            if (interval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(interval), "Fire interval must be positive.");
            if (initialDelay < 0f)
                throw new ArgumentOutOfRangeException(nameof(initialDelay), "Initial delay cannot be negative.");

            _interval = interval;
            _initialDelay = initialDelay;

            Reset();
        }

        public void Reset()
        {
            _elapsed = 0f;
            _nextShotAt = _initialDelay;
        }

        /// <summary>
        /// Advances time and reports how many shots came due. Absolute times rather
        /// than a countdown reset after each shot: a long frame pays back the shots it
        /// swallowed, and rounding cannot accumulate into a rate of fire that drifts
        /// with the frame rate.
        /// </summary>
        public int Step(float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Time cannot run backwards.");

            _elapsed += deltaTime;

            var shots = 0;
            while (_elapsed >= _nextShotAt)
            {
                _nextShotAt += _interval;
                shots++;
            }

            return shots;
        }
    }
}
