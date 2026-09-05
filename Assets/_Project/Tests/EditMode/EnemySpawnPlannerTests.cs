using System;
using System.Linq;
using NUnit.Framework;
using TurretRush.Enemies;
using UnityEngine;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class EnemySpawnPlannerTests
    {
        private const float FromZ = 40f;
        private const float ToZ = 430f;
        private const float HalfWidth = 8.7f;
        private const int Count = 45;
        private const float SpacingMargin = 0.25f;
        private const int Seed = 20260904;

        private static EnemySpawnPlanner Create(
            int count = Count,
            float spacingMargin = SpacingMargin)
            => new EnemySpawnPlanner(FromZ, ToZ, HalfWidth, count, spacingMargin);

        [Test]
        public void Plan_ProducesTheRequestedNumberOfEnemies()
        {
            Assert.That(Create().Plan(Seed).Count, Is.EqualTo(Count));
        }

        [Test]
        public void Plan_IsIdenticalForTheSameSeed()
        {
            var a = Create().Plan(Seed);
            var b = Create().Plan(Seed);

            // The level must replay exactly, or losing stops being the player's fault.
            for (var i = 0; i < a.Count; i++)
            {
                Assert.That(a[i].X, Is.EqualTo(b[i].X));
                Assert.That(a[i].Z, Is.EqualTo(b[i].Z));
            }
        }

        [Test]
        public void Plan_DiffersBetweenSeeds()
        {
            var a = Create().Plan(Seed);
            var b = Create().Plan(Seed + 1);

            var identical = a.Zip(b, (x, y) => Mathf.Approximately(x.Z, y.Z) && Mathf.Approximately(x.X, y.X))
                .All(same => same);

            Assert.That(identical, Is.False);
        }

        [Test]
        public void Plan_KeepsTheOpeningStretchClear()
        {
            var plan = Create().Plan(Seed);

            // Nothing may spawn inside the run-up, or the player is under attack
            // before they have looked at the road.
            Assert.That(plan.Min(p => p.Z), Is.GreaterThanOrEqualTo(FromZ));
        }

        [Test]
        public void Plan_StopsBeforeTheFinish()
        {
            var plan = Create().Plan(Seed);

            Assert.That(plan.Max(p => p.Z), Is.LessThanOrEqualTo(ToZ));
        }

        [Test]
        public void Plan_KeepsEveryEnemyOnTheRoad()
        {
            var plan = Create().Plan(Seed);

            // An enemy on the sand is unreachable scenery that still counts against
            // the player's kill tally.
            foreach (var point in plan)
                Assert.That(Mathf.Abs(point.X), Is.LessThanOrEqualTo(HalfWidth));
        }

        [Test]
        public void Plan_IsOrderedByDistance()
        {
            var plan = Create().Plan(Seed);

            // The spawner walks this with a single forward cursor. Out of order, it
            // would skip enemies permanently.
            for (var i = 1; i < plan.Count; i++)
                Assert.That(plan[i].Z, Is.GreaterThan(plan[i - 1].Z));
        }

        [Test]
        public void Plan_NeverPutsTwoEnemiesOnTopOfEachOther()
        {
            var planner = Create();
            var plan = planner.Plan(Seed);

            for (var i = 1; i < plan.Count; i++)
            {
                Assert.That(plan[i].Z - plan[i - 1].Z,
                    Is.GreaterThanOrEqualTo(planner.MinimumSpacing - 0.001f),
                    $"Enemies {i - 1} and {i} are closer than the guaranteed gap.");
            }
        }

        [Test]
        public void Plan_UsesTheWholeStretchRatherThanClumping()
        {
            var planner = Create();
            var plan = planner.Plan(Seed);

            // Uniform random over the whole track leaves empty stretches next to
            // walls of enemies. Stratifying is what stops that, and this is the
            // symptom it prevents: both ends of the level are populated.
            Assert.That(plan[0].Z, Is.LessThan(FromZ + planner.BandLength));
            Assert.That(plan[plan.Count - 1].Z, Is.GreaterThan(ToZ - planner.BandLength));
        }

        [Test]
        public void Plan_SpreadsAcrossBothSidesOfTheRoad()
        {
            var plan = Create().Plan(Seed);

            Assert.That(plan.Any(p => p.X < -HalfWidth * 0.3f), Is.True);
            Assert.That(plan.Any(p => p.X > HalfWidth * 0.3f), Is.True);
        }

        [Test]
        public void Plan_ZeroSpacingMargin_UsesTheFullBand()
        {
            var planner = Create(spacingMargin: 0f);

            Assert.That(planner.MinimumSpacing, Is.EqualTo(0f));
            Assert.That(planner.Plan(Seed).Count, Is.EqualTo(Count));
        }

        [Test]
        public void Plan_SingleEnemy_LandsInsideTheStretch()
        {
            var plan = Create(count: 1).Plan(Seed);

            Assert.That(plan.Count, Is.EqualTo(1));
            Assert.That(plan[0].Z, Is.InRange(FromZ, ToZ));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_NoEnemies_Throws(int count)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(count));
        }

        [Test]
        public void Constructor_EmptyStretch_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new EnemySpawnPlanner(100f, 100f, HalfWidth, Count, SpacingMargin));
        }

        [Test]
        public void Constructor_NoRoadWidth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new EnemySpawnPlanner(FromZ, ToZ, 0f, Count, SpacingMargin));
        }
    }
}
