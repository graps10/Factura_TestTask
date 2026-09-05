using System;
using NUnit.Framework;
using TurretRush.Level;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class LevelProgressTests
    {
        private const float Length = 450f;

        private static LevelProgress Create() => new LevelProgress(Length);

        [Test]
        public void Evaluate_IsZeroOnTheStartLine()
        {
            Assert.That(Create().Evaluate(0f), Is.EqualTo(0f));
        }

        [Test]
        public void Evaluate_IsOneAtTheFinish()
        {
            Assert.That(Create().Evaluate(Length), Is.EqualTo(1f));
        }

        [Test]
        public void Evaluate_ReportsTheFractionCovered()
        {
            Assert.That(Create().Evaluate(Length * 0.25f), Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void Evaluate_ClampsPastTheFinish()
        {
            // The car keeps rolling for a moment after the level is already won, and
            // a fill bar cannot show 104 per cent.
            Assert.That(Create().Evaluate(Length * 2f), Is.EqualTo(1f));
        }

        [Test]
        public void Evaluate_ClampsBehindTheStart()
        {
            Assert.That(Create().Evaluate(-50f), Is.EqualTo(0f));
        }

        [Test]
        public void IsComplete_OnlyAtOrPastTheFinish()
        {
            var progress = Create();

            Assert.That(progress.IsComplete(Length - 0.01f), Is.False);
            Assert.That(progress.IsComplete(Length), Is.True);
            Assert.That(progress.IsComplete(Length + 100f), Is.True);
        }

        [Test]
        public void IsComplete_AgreesWithEvaluateReachingOne()
        {
            var progress = Create();

            // The win condition and the progress bar read from the same object for
            // exactly this reason: a bar showing full while the level is not over,
            // or the reverse, is the kind of mismatch nobody notices until a video
            // is being recorded.
            for (var distance = 0f; distance <= Length * 1.2f; distance += 1.3f)
            {
                var full = progress.Evaluate(distance) >= 1f;
                Assert.That(progress.IsComplete(distance), Is.EqualTo(full), $"at {distance} m");
            }
        }

        [Test]
        public void RemainingMetres_CountsDownAndStopsAtZero()
        {
            var progress = Create();

            Assert.That(progress.RemainingMetres(0f), Is.EqualTo(Length));
            Assert.That(progress.RemainingMetres(100f), Is.EqualTo(Length - 100f));
            Assert.That(progress.RemainingMetres(Length + 50f), Is.EqualTo(0f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_NonPositiveLength_Throws(float length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelProgress(length));
        }
    }
}
