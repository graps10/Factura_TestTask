using System;

namespace TurretRush.Combat
{
    /// <summary>
    /// Decides when the turret is due to fire. Kept apart from the weapon so the
    /// rate of fire can be asserted directly instead of being eyeballed in play
    /// mode, and so a frame hitch cannot quietly change the gun's damage output.
    /// </summary>
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
        /// Advances time and reports how many shots came due. The schedule is kept
        /// as absolute times rather than a countdown that gets reset, so a long frame
        /// pays back the shots it swallowed instead of silently dropping them, and
        /// rounding cannot accumulate into a drifting rate of fire.
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
