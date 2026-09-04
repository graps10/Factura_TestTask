using System;
using NUnit.Framework;
using TurretRush.Combat;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class FireTimerTests
    {
        private const float Interval = 0.25f;
        private const float InitialDelay = 0.6f;

        private static FireTimer Create() => new FireTimer(Interval, InitialDelay);

        private static int FireOver(FireTimer timer, float seconds, float frame)
        {
            var shots = 0;
            for (var t = 0f; t < seconds; t += frame)
                shots += timer.Step(frame);

            return shots;
        }

        [Test]
        public void Step_StaysSilentThroughTheOpeningDelay()
        {
            var timer = Create();

            var shots = FireOver(timer, 0.55f, 1f / 60f);

            Assert.That(shots, Is.EqualTo(0));
        }

        [Test]
        public void Step_FiresOnceTheOpeningDelayHasPassed()
        {
            var timer = Create();

            var shots = FireOver(timer, 0.62f, 1f / 60f);

            Assert.That(shots, Is.EqualTo(1));
        }

        [Test]
        public void Step_KeepsToTheIntervalAfterTheFirstShot()
        {
            var timer = Create();
            FireOver(timer, 0.62f, 1f / 60f);

            var shots = FireOver(timer, Interval, 1f / 60f);

            Assert.That(shots, Is.EqualTo(1));
        }

        [Test]
        public void Step_OneLongFrameRepaysEveryShotItSwallowed()
        {
            var timer = Create();

            // One second covers the 0.6 s opening shot and the 0.85 s one. Anything
            // that only fires once per call would quietly halve the gun's damage
            // every time the frame rate dipped.
            var shots = timer.Step(1f);

            Assert.That(shots, Is.EqualTo(2));
        }

        [TestCase(1f / 30f)]
        [TestCase(1f / 60f)]
        [TestCase(1f / 144f)]
        public void Step_RateOfFireDoesNotDependOnFrameRate(float frame)
        {
            var timer = Create();

            var shots = FireOver(timer, 10f, frame);

            // 10 s minus the 0.6 s delay, one shot every 0.25 s, plus the opening one.
            Assert.That(shots, Is.EqualTo(38).Within(1),
                "Damage output changed with frame rate.");
        }

        [Test]
        public void Step_DoesNotDriftOverALongLevel()
        {
            var timer = Create();

            var shots = FireOver(timer, 300f, 1f / 60f);

            // Five minutes of accumulation. A countdown reset every shot would round
            // its way to a visibly different total by here.
            Assert.That(shots, Is.EqualTo(1198).Within(2));
        }

        [Test]
        public void Step_ZeroDeltaFiresNothing()
        {
            var timer = Create();
            FireOver(timer, 5f, 1f / 60f);

            Assert.That(timer.Step(0f), Is.EqualTo(0), "A paused game must not shoot.");
        }

        [Test]
        public void Step_NegativeDelta_Throws()
        {
            var timer = Create();

            Assert.Throws<ArgumentOutOfRangeException>(() => timer.Step(-0.1f));
        }

        [Test]
        public void Reset_BringsBackTheOpeningDelay()
        {
            var timer = Create();
            FireOver(timer, 5f, 1f / 60f);

            timer.Reset();

            Assert.That(FireOver(timer, 0.55f, 1f / 60f), Is.EqualTo(0));
            Assert.That(FireOver(timer, 0.1f, 1f / 60f), Is.EqualTo(1));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_NonPositiveInterval_Throws(float interval)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FireTimer(interval, InitialDelay));
        }

        [Test]
        public void Constructor_NegativeInitialDelay_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FireTimer(Interval, -1f));
        }

        [Test]
        public void Constructor_ZeroInitialDelay_FiresOnTheFirstStep()
        {
            var timer = new FireTimer(Interval, 0f);

            Assert.That(timer.Step(0f), Is.EqualTo(1));
        }
    }
}
