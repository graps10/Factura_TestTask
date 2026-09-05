using UnityEngine;

namespace TurretRush.Player
{
    /// <summary>
    /// The S-curve the car follows instead of a dead straight line. A pure function
    /// of distance, so nothing is stored between frames and a restart reproduces the
    /// same road.
    ///
    /// Two sines rather than Perlin noise: the weights sum to one, so the offset is
    /// provably within +/- amplitude and cannot put the car on the sand. PerlinNoise
    /// promises no such range.
    /// </summary>
    public sealed class LateralDrift
    {
        private const float PrimaryWeight = 0.6f;
        private const float SecondaryWeight = 0.4f;

        /// <summary>Not a round fraction, so the two waves take a long time to line
        /// up and the road never looks like it repeats.</summary>
        private const float SecondaryRatio = 0.37f;

        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly float _phase;

        public LateralDrift(float amplitude, float frequency, float phase = 0f)
        {
            _amplitude = Mathf.Max(0f, amplitude);
            _frequency = frequency;
            _phase = phase;
        }

        public float Amplitude => _amplitude;

        /// <summary>Sideways offset from the centre line, in metres.</summary>
        public float Evaluate(float distance)
        {
            var primary = Mathf.Sin(distance * _frequency + _phase);
            var secondary = Mathf.Sin(distance * _frequency * SecondaryRatio + _phase * 1.7f);
            return (primary * PrimaryWeight + secondary * SecondaryWeight) * _amplitude;
        }

        /// <summary>Which way the car points, sampled from the curve itself - so
        /// position and heading cannot drift apart.</summary>
        public float HeadingDegrees(float distance, float lookAhead)
        {
            var here = Evaluate(distance);
            var ahead = Evaluate(distance + lookAhead);
            return Mathf.Atan2(ahead - here, lookAhead) * Mathf.Rad2Deg;
        }
    }
}
