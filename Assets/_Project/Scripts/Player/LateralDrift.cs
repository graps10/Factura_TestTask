using UnityEngine;

namespace TurretRush.Player
{
    /// <summary>
    /// The gentle S-curve the car follows instead of driving in a dead straight
    /// line. Pure function of distance travelled: given the same distance it always
    /// returns the same offset, so nothing has to be stored between frames and a
    /// restart reproduces the exact same road.
    ///
    /// Two sines rather than Perlin noise: the weights add up to one, so the result
    /// is provably inside +/- amplitude and can never push the car off the asphalt.
    /// Unity's PerlinNoise makes no such guarantee about its output range.
    /// </summary>
    public sealed class LateralDrift
    {
        private const float PrimaryWeight = 0.6f;
        private const float SecondaryWeight = 0.4f;

        /// <summary>Deliberately not a round fraction, so the two waves take a long
        /// time to line up and the road never looks like it repeats.</summary>
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

        /// <summary>
        /// Which way the car is pointing, derived from the curve itself by sampling
        /// slightly ahead. Keeps the body aligned with its own path for free - no
        /// separate steering state to keep in sync.
        /// </summary>
        public float HeadingDegrees(float distance, float lookAhead)
        {
            var here = Evaluate(distance);
            var ahead = Evaluate(distance + lookAhead);
            return Mathf.Atan2(ahead - here, lookAhead) * Mathf.Rad2Deg;
        }
    }
}
