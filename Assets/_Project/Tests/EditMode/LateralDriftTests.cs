using NUnit.Framework;
using TurretRush.Player;
using UnityEngine;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class LateralDriftTests
    {
        private const float Amplitude = 4f;
        private const float Frequency = 0.035f;

        private static LateralDrift Create(float amplitude = Amplitude, float phase = 0f)
            => new LateralDrift(amplitude, Frequency, phase);

        [Test]
        public void Evaluate_NeverLeavesTheAllowedCorridor()
        {
            var drift = Create();

            // Two kilometres is far more than any level we will ship, sampled finely
            // enough that no peak can slip between samples.
            for (var d = 0f; d < 2000f; d += 0.25f)
            {
                var offset = drift.Evaluate(d);

                Assert.That(Mathf.Abs(offset), Is.LessThanOrEqualTo(Amplitude + 0.0001f),
                    $"Drift left the corridor at {d} m: {offset}");
            }
        }

        [Test]
        public void Evaluate_IsDeterministicAcrossInstances()
        {
            var a = Create();
            var b = Create();

            for (var d = 0f; d < 500f; d += 7.3f)
                Assert.That(a.Evaluate(d), Is.EqualTo(b.Evaluate(d)).Within(1e-6f));
        }

        [Test]
        public void Evaluate_IsContinuous()
        {
            var drift = Create();
            const float step = 0.1f;

            // Nothing the car drives over should ever be a jump. A metre-per-metre
            // slope well under 1 keeps the heading angle sane too.
            for (var d = 0f; d < 1000f; d += step)
            {
                var jump = Mathf.Abs(drift.Evaluate(d + step) - drift.Evaluate(d));

                Assert.That(jump, Is.LessThan(step), $"Discontinuity at {d} m: {jump}");
            }
        }

        [Test]
        public void Evaluate_ZeroAmplitude_StaysOnTheCentreLine()
        {
            var drift = Create(amplitude: 0f);

            for (var d = 0f; d < 500f; d += 3.1f)
                Assert.That(drift.Evaluate(d), Is.EqualTo(0f));
        }

        [Test]
        public void Evaluate_NegativeAmplitude_IsTreatedAsZero()
        {
            var drift = Create(amplitude: -5f);

            Assert.That(drift.Amplitude, Is.EqualTo(0f));
            Assert.That(drift.Evaluate(123f), Is.EqualTo(0f));
        }

        [Test]
        public void Evaluate_DifferentPhase_ProducesADifferentRoad()
        {
            var a = Create(phase: 0f);
            var b = Create(phase: 1.3f);

            var different = false;
            for (var d = 0f; d < 500f && !different; d += 5f)
                different = Mathf.Abs(a.Evaluate(d) - b.Evaluate(d)) > 0.01f;

            Assert.That(different, Is.True, "Phase had no effect on the generated road.");
        }

        [Test]
        public void HeadingDegrees_PointsTheWayTheCurveIsGoing()
        {
            var drift = Create();
            const float lookAhead = 2f;

            for (var d = 0f; d < 1000f; d += 1.7f)
            {
                var rising = drift.Evaluate(d + lookAhead) - drift.Evaluate(d);
                var heading = drift.HeadingDegrees(d, lookAhead);

                // Sideways offset grows to +X, so the nose must turn to +Y yaw.
                Assert.That(Mathf.Sign(heading), Is.EqualTo(Mathf.Sign(rising)),
                    $"Heading and path disagree at {d} m.");
            }
        }

        [Test]
        public void HeadingDegrees_StaysGentle()
        {
            var drift = Create();

            // A car that yaws more than 15 degrees off the road axis looks like it is
            // changing lanes, not drifting. Guards against someone raising amplitude
            // or frequency until the motion turns into slalom.
            for (var d = 0f; d < 1000f; d += 1.7f)
                Assert.That(Mathf.Abs(drift.HeadingDegrees(d, 2f)), Is.LessThan(15f));
        }

        [Test]
        public void HeadingDegrees_ZeroAmplitude_IsStraightAhead()
        {
            var drift = Create(amplitude: 0f);

            Assert.That(drift.HeadingDegrees(42f, 2f), Is.EqualTo(0f));
        }
    }
}
