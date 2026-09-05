using System;
using UnityEngine;

namespace TurretRush.Level
{
    /// <summary>
    /// How far through the level the car is. One place owns the division, so the win
    /// condition and the progress bar cannot disagree about what finished means.
    /// </summary>
    public sealed class LevelProgress
    {
        private readonly float _length;

        public LevelProgress(float length)
        {
            if (length <= 0f)
                throw new ArgumentOutOfRangeException(nameof(length), "A level needs a positive length.");

            _length = length;
        }

        public float Length => _length;

        /// <summary>0 at the start line, 1 at the finish. Clamped, because the car
        /// rolls on for a moment after the level is already won.</summary>
        public float Evaluate(float distance) => Mathf.Clamp01(distance / _length);

        public bool IsComplete(float distance) => distance >= _length;
    }
}
