using NUnit.Framework;
using TurretRush.Player;
using UnityEngine;

namespace TurretRush.Tests.PlayMode
{
    /// <summary>
    /// The melee rule. An enemy reaching the car is resolved against the car's box
    /// in code rather than by a trigger, so this arithmetic decides every hit the
    /// player takes - including while the car is turned, which is most of the time.
    /// </summary>
    [TestFixture]
    public sealed class CarFootprintTests
    {
        private const float HalfWidth = 0.95f;
        private const float HalfLength = 2.125f;

        private GameObject _object;
        private CarView _car;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("Car");

            var box = _object.AddComponent<BoxCollider>();
            box.size = new Vector3(HalfWidth * 2f, 1.67f, HalfLength * 2f);
            box.center = new Vector3(0f, 0.96f, 0f);

            _car = _object.AddComponent<CarView>();
            TestObjects.SetField(_car, "body", box);
        }

        [TearDown]
        public void TearDown()
        {
            if (_object != null)
                Object.Destroy(_object);
        }

        [Test]
        public void Overlaps_UnderTheBody_IsTrue()
        {
            Assert.That(_car.Overlaps(new Vector3(0f, 0f, 1f), 0f), Is.True);
        }

        [Test]
        public void Overlaps_BeyondTheNose_IsFalse()
        {
            Assert.That(_car.Overlaps(new Vector3(0f, 0f, HalfLength + 0.5f), 0f), Is.False);
        }

        [Test]
        public void Overlaps_AlongsideTheBody_IsFalse()
        {
            Assert.That(_car.Overlaps(new Vector3(HalfWidth + 0.5f, 0f, 0f), 0f), Is.False);
        }

        [Test]
        public void Overlaps_MarginReachesFurtherOut()
        {
            var justOutside = new Vector3(0f, 0f, HalfLength + 0.3f);

            Assert.That(_car.Overlaps(justOutside, 0f), Is.False);
            Assert.That(_car.Overlaps(justOutside, 0.35f), Is.True);
        }

        [Test]
        public void Overlaps_IgnoresHeight()
        {
            // Everything in this game stands on one plane, and an enemy's position is
            // at its feet while the box is centred on the body. Testing Y would mean
            // enemies walking under the car untouched.
            Assert.That(_car.Overlaps(new Vector3(0f, 0f, 1f), 0f), Is.True);
            Assert.That(_car.Overlaps(new Vector3(0f, 40f, 1f), 0f), Is.True);
        }

        [Test]
        public void Overlaps_TurnsWithTheCar()
        {
            // Beside the car when it points down the road, and along its length once
            // it has swung ninety degrees. The car weaves the whole level, so a check
            // written in world axes would be wrong for most of it.
            var point = new Vector3(1.4f, 0f, 0f);

            Assert.That(_car.Overlaps(point, 0f), Is.False);

            _object.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            Assert.That(_car.Overlaps(point, 0f), Is.True);
        }

        [Test]
        public void Overlaps_MovesWithTheCar()
        {
            var point = new Vector3(0f, 0f, 30f);

            Assert.That(_car.Overlaps(point, 0f), Is.False);

            _object.transform.position = new Vector3(0f, 0f, 29f);

            Assert.That(_car.Overlaps(point, 0f), Is.True);
        }
    }
}
