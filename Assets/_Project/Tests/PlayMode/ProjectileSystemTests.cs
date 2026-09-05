using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Enemies;
using UnityEngine;
using UnityEngine.TestTools;

namespace TurretRush.Tests.PlayMode
{
    /// <summary>
    /// The damage chain end to end: a cast that actually sweeps, a layer mask that
    /// actually filters, and damage that actually reaches hit points through a
    /// collider. None of it can be checked without the physics engine, and all of it
    /// fails silently - a wrong mask does not throw, it just means nothing ever dies.
    /// </summary>
    [TestFixture]
    public sealed class ProjectileSystemTests
    {
        private const int Damage = 10;
        private const int TargetHealth = 1000;

        /// <summary>Frames are pinned so a step is an exact distance, not whatever
        /// the editor happened to render in.</summary>
        private const float FixedStep = 0.05f;

        private readonly List<GameObject> _spawned = new();

        private int _enemyLayer;
        private ProjectileView _prefab;
        private ProjectileSystem _system;
        private WeaponConfig _config;

        [SetUp]
        public void SetUp()
        {
            _enemyLayer = TestObjects.RequireLayer("Enemy");

            _prefab = NewObject("Projectile").AddComponent<ProjectileView>();
            _prefab.gameObject.SetActive(false);

            Time.captureDeltaTime = FixedStep;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;

            _system?.Dispose();
            _system = null;

            if (_config != null)
                Object.Destroy(_config);

            _config = null;

            foreach (var spawned in _spawned)
                if (spawned != null)
                    Object.Destroy(spawned);

            _spawned.Clear();
        }

        [UnityTest]
        public IEnumerator Projectile_HitsATargetInItsPath()
        {
            _system = CreateSystem(speed: 20f);
            var target = CreateTarget(new Vector3(0f, 0f, 3f), _enemyLayer);

            _system.Spawn(new Vector3(0f, 1f, 0f), Vector3.forward);
            yield return Step(6);

            Assert.That(target.Current, Is.EqualTo(TargetHealth - Damage));
            Assert.That(_system.LiveCount, Is.EqualTo(0), "The shot carried on after landing.");
        }

        [UnityTest]
        public IEnumerator Projectile_SweepsThroughATargetItWouldOtherwiseStepOver()
        {
            // 4.5 m of travel in a single step, against a body 1 m thick sitting
            // entirely between this frame's position and the next. A projectile that
            // tested the point it landed on would pass straight through - which is
            // exactly what happens at 90 m/s without a sweep.
            _system = CreateSystem(speed: 90f);
            var target = CreateTarget(new Vector3(0f, 0f, 2.2f), _enemyLayer);

            _system.Spawn(new Vector3(0f, 1f, 0f), Vector3.forward);
            yield return Step(1);

            Assert.That(target.Current, Is.EqualTo(TargetHealth - Damage),
                "The shot tunnelled through the target.");
        }

        [UnityTest]
        public IEnumerator Projectile_StaysAliveUntilItsRangeIsSpent()
        {
            _system = CreateSystem(speed: 20f, maxRange: 5f);

            _system.Spawn(Vector3.up, Vector3.forward);

            yield return Step(4);
            Assert.That(_system.LiveCount, Is.EqualTo(1), "Retired before reaching its range.");

            yield return Step(3);
            Assert.That(_system.LiveCount, Is.EqualTo(0), "Still flying beyond its range.");
        }

        [UnityTest]
        public IEnumerator Projectile_IgnoresCollidersOutsideItsMask()
        {
            _system = CreateSystem(speed: 20f, maxRange: 50f);

            // Same body, same path, wrong layer.
            var target = CreateTarget(new Vector3(0f, 0f, 3f), layer: 0);

            _system.Spawn(new Vector3(0f, 1f, 0f), Vector3.forward);
            yield return Step(8);

            Assert.That(target.Current, Is.EqualTo(TargetHealth));
            Assert.That(_system.LiveCount, Is.EqualTo(1), "The shot stopped on something it cannot hit.");
        }

        [UnityTest]
        public IEnumerator Projectile_SpendsItselfOnTheFirstTargetOnly()
        {
            _system = CreateSystem(speed: 20f, maxRange: 50f);

            var near = CreateTarget(new Vector3(0f, 0f, 3f), _enemyLayer);
            var far = CreateTarget(new Vector3(0f, 0f, 9f), _enemyLayer);

            _system.Spawn(new Vector3(0f, 1f, 0f), Vector3.forward);
            yield return Step(12);

            Assert.That(near.Current, Is.EqualTo(TargetHealth - Damage));
            Assert.That(far.Current, Is.EqualTo(TargetHealth), "One bullet damaged two bodies.");
        }

        private ProjectileSystem CreateSystem(float speed, float radius = 0.3f, float maxRange = 200f)
        {
            _config = TestObjects.Config<WeaponConfig>(
                ("projectilePrefab", _prefab),
                ("damage", Damage),
                ("speed", speed),
                ("radius", radius),
                ("maxRange", maxRange),
                ("hitMask", (LayerMask)(1 << _enemyLayer)));

            var system = new ProjectileSystem(_config);
            system.Start();

            return system;
        }

        private Health CreateTarget(Vector3 position, int layer)
        {
            var body = NewObject("Target");
            body.layer = layer;
            body.transform.position = position;

            var capsule = body.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0f, 1f, 0f);

            var health = new Health(TargetHealth);
            body.AddComponent<EnemyView>().Bind(health);

            return health;
        }

        private GameObject NewObject(string name)
        {
            var created = new GameObject(name);
            _spawned.Add(created);

            return created;
        }

        private IEnumerator Step(int frames)
        {
            for (var i = 0; i < frames; i++)
            {
                yield return null;
                _system.Tick();
            }
        }
    }
}
