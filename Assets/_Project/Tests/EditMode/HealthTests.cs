using System;
using NUnit.Framework;
using TurretRush.Combat;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class HealthTests
    {
        [Test]
        public void NewHealth_IsFullAndAlive()
        {
            var health = new Health(100);

            Assert.That(health.Max, Is.EqualTo(100));
            Assert.That(health.Current, Is.EqualTo(100));
            Assert.That(health.IsAlive, Is.True);
            Assert.That(health.Normalized, Is.EqualTo(1f));
        }

        [Test]
        public void TakeDamage_ReducesCurrent()
        {
            var health = new Health(100);

            health.TakeDamage(30);

            Assert.That(health.Current, Is.EqualTo(70));
            Assert.That(health.Normalized, Is.EqualTo(0.7f).Within(0.0001f));
        }

        [Test]
        public void TakeDamage_ClampsAtZero()
        {
            var health = new Health(50);

            health.TakeDamage(999);

            Assert.That(health.Current, Is.EqualTo(0));
            Assert.That(health.IsAlive, Is.False);
        }

        [Test]
        public void TakeDamage_RaisesChangedWithCurrentAndMax()
        {
            var health = new Health(100);
            var current = -1;
            var max = -1;
            health.Changed += (c, m) =>
            {
                current = c;
                max = m;
            };

            health.TakeDamage(25);

            Assert.That(current, Is.EqualTo(75));
            Assert.That(max, Is.EqualTo(100));
        }

        [Test]
        public void TakeDamage_RaisesDiedExactlyOnce()
        {
            var health = new Health(10);
            var deaths = 0;
            health.Died += () => deaths++;

            health.TakeDamage(10);
            health.TakeDamage(10);
            health.TakeDamage(10);

            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void TakeDamage_WhenDead_IsIgnored()
        {
            var health = new Health(10);
            health.TakeDamage(10);

            var changes = 0;
            health.Changed += (_, _) => changes++;

            health.TakeDamage(5);

            Assert.That(health.Current, Is.EqualTo(0));
            Assert.That(changes, Is.EqualTo(0), "A dead target must not keep emitting Changed.");
        }

        [Test]
        public void TakeDamage_Zero_DoesNotRaiseChanged()
        {
            var health = new Health(100);
            var changes = 0;
            health.Changed += (_, _) => changes++;

            health.TakeDamage(0);

            Assert.That(changes, Is.EqualTo(0));
        }

        [Test]
        public void TakeDamage_Negative_Throws()
        {
            var health = new Health(100);

            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(-5));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_NonPositiveMax_Throws(int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(max));
        }

        [Test]
        public void Reset_RevivesAndRestoresToNewMax()
        {
            var health = new Health(10);
            health.TakeDamage(10);

            health.Reset(40);

            Assert.That(health.IsAlive, Is.True);
            Assert.That(health.Current, Is.EqualTo(40));
            Assert.That(health.Max, Is.EqualTo(40));
        }
    }
}
