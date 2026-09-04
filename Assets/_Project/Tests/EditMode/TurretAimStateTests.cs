using NUnit.Framework;
using TurretRush.Player;
using UnityEngine;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class TurretAimStateTests
    {
        private const float MaxYaw = 60f;
        private const float DegreesPerScreenWidth = 130f;
        private const float SmoothTime = 0.05f;

        private static TurretAimState Create(float smoothTime = SmoothTime)
            => new TurretAimState(MaxYaw, DegreesPerScreenWidth, smoothTime);

        [Test]
        public void AddInput_TurnsProportionallyToTheDrag()
        {
            var aim = Create();

            // A quarter of the screen is 32.5 degrees here, comfortably inside the
            // 60 degree arc - proportionality is only observable below the clamp,
            // and half a screen would already saturate it.
            aim.AddInput(0.25f);

            Assert.That(aim.TargetYaw, Is.EqualTo(DegreesPerScreenWidth * 0.25f).Within(0.001f));
        }

        [Test]
        public void AddInput_DragToTheRightTurnsRight()
        {
            var aim = Create();

            aim.AddInput(0.1f);

            Assert.That(aim.TargetYaw, Is.GreaterThan(0f));
        }

        [Test]
        public void AddInput_Accumulates()
        {
            var aim = Create();

            aim.AddInput(0.1f);
            aim.AddInput(0.1f);

            Assert.That(aim.TargetYaw, Is.EqualTo(DegreesPerScreenWidth * 0.2f).Within(0.001f));
        }

        [TestCase(1f)]
        [TestCase(-1f)]
        public void AddInput_CannotSwingPastTheLimit(float direction)
        {
            var aim = Create();

            // Ten full-screen swipes in one direction. Without a clamp the barrel
            // would end up pointing back at the player.
            for (var i = 0; i < 10; i++)
                aim.AddInput(direction);

            Assert.That(Mathf.Abs(aim.TargetYaw), Is.EqualTo(MaxYaw).Within(0.001f));
        }

        [Test]
        public void AddInput_UnwindsImmediatelyAfterBeingClamped()
        {
            var aim = Create();
            for (var i = 0; i < 10; i++)
                aim.AddInput(1f);

            // Dragging back must respond on the first pixel. If the clamp had been
            // applied to an unbounded accumulator instead, the player would swipe
            // through nine screen widths of dead input before the barrel moved.
            aim.AddInput(-0.1f);

            Assert.That(aim.TargetYaw, Is.EqualTo(MaxYaw - DegreesPerScreenWidth * 0.1f).Within(0.001f));
        }

        [Test]
        public void Step_ConvergesOnTheTarget()
        {
            var aim = Create();
            aim.AddInput(0.3f);

            for (var i = 0; i < 200; i++)
                aim.Step(1f / 60f);

            Assert.That(aim.CurrentYaw, Is.EqualTo(aim.TargetYaw).Within(0.01f));
        }

        [Test]
        public void Step_NeverOvershootsTheTarget()
        {
            var aim = Create();
            aim.AddInput(0.3f);

            for (var i = 0; i < 200; i++)
            {
                aim.Step(1f / 60f);

                Assert.That(aim.CurrentYaw, Is.LessThanOrEqualTo(aim.TargetYaw + 0.001f),
                    "Smoothing overshot, which would show up as the barrel wobbling.");
            }
        }

        [Test]
        public void Step_ZeroSmoothTime_SnapsStraightToTheTarget()
        {
            var aim = Create(smoothTime: 0f);
            aim.AddInput(0.4f);

            aim.Step(1f / 60f);

            Assert.That(aim.CurrentYaw, Is.EqualTo(aim.TargetYaw));
        }

        [Test]
        public void CurrentYaw_StaysInsideTheLimitsUnderRandomInput()
        {
            var aim = Create();
            var random = new System.Random(99);

            // Whatever the player does with their finger, the barrel must never leave
            // the firing arc - not even transiently while the smoothing catches up.
            for (var i = 0; i < 5000; i++)
            {
                aim.AddInput(((float)random.NextDouble() - 0.5f) * 0.4f);
                aim.Step(1f / 60f);

                Assert.That(Mathf.Abs(aim.CurrentYaw), Is.LessThanOrEqualTo(MaxYaw + 0.001f));
            }
        }

        [Test]
        public void Reset_RecentresTheBarrel()
        {
            var aim = Create();
            aim.AddInput(0.5f);
            for (var i = 0; i < 60; i++)
                aim.Step(1f / 60f);

            aim.Reset();

            Assert.That(aim.TargetYaw, Is.EqualTo(0f));
            Assert.That(aim.CurrentYaw, Is.EqualTo(0f));
        }

        [Test]
        public void Reset_ClearsSmoothingVelocity()
        {
            var aim = Create();
            aim.AddInput(1f);
            aim.Step(1f / 60f);

            aim.Reset();
            aim.Step(1f / 60f);

            // A leftover velocity would drag the barrel off centre on the first frame
            // of the next run, which reads as the turret twitching on restart.
            Assert.That(aim.CurrentYaw, Is.EqualTo(0f));
        }
    }
}
