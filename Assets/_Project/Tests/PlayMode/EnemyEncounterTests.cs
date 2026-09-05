using System.Collections;
using NUnit.Framework;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Enemies;
using TurretRush.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace TurretRush.Tests.PlayMode
{
    /// <summary>
    /// The two ways an encounter can end, driven through the real system: the enemy
    /// gets there, or it dies on the way. Both chains run through a pool, and both
    /// are the kind of thing that only breaks once everything is assembled.
    /// </summary>
    [TestFixture]
    public sealed class EnemyEncounterTests
    {
        private const float FixedStep = 0.05f;
        private const int CarHealth = 100;

        private GameObject _carObject;
        private GameObject _enemyPrefabObject;
        private EnemySystem _system;
        private Health _carHitPoints;
        private EnemyConfig _enemyConfig;
        private LevelConfig _levelConfig;

        // UnitySetUp rather than SetUp, and it opens by letting a frame pass.
        // Object.Destroy is deferred to the end of the frame, so without that pause
        // the previous test's enemy is still in the scene when this one looks for
        // the body the pool just handed out.
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;

            var enemyLayer = TestObjects.RequireLayer("Enemy");

            _carObject = new GameObject("Car");
            var box = _carObject.AddComponent<BoxCollider>();
            box.size = new Vector3(1.9f, 1.67f, 4.25f);
            box.center = new Vector3(0f, 0.96f, 0f);

            var car = _carObject.AddComponent<CarView>();
            TestObjects.SetField(car, "body", box);

            _enemyPrefabObject = new GameObject("Enemy") { layer = enemyLayer };
            var capsule = _enemyPrefabObject.AddComponent<CapsuleCollider>();
            capsule.height = 1.8f;
            capsule.radius = 0.3f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            var prefab = _enemyPrefabObject.AddComponent<EnemyView>();

            // Inactive, so the only active EnemyView in the scene is the one the pool
            // hands out - which is how the test finds it.
            _enemyPrefabObject.SetActive(false);

            // A one-enemy level a couple of car lengths long, so the encounter takes
            // about a second instead of the forty a real level would.
            _levelConfig = TestObjects.Config<LevelConfig>(
                ("length", 12f),
                ("roadHalfWidth", 1f),
                ("roadEdgeMargin", 0f));

            _enemyConfig = TestObjects.Config<EnemyConfig>(
                ("prefab", prefab),
                ("count", 1),
                ("safeStartDistance", 4f),
                ("finishMargin", 0f),
                ("detectRadius", 50f));

            _carHitPoints = new Health(CarHealth);

            _system = new EnemySystem(_enemyConfig, _levelConfig, car, _carHitPoints);
            _system.Start();
            _system.ResetToStart();

            Time.captureDeltaTime = FixedStep;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;

            _system?.Dispose();
            _system = null;

            if (_carObject != null)
                Object.Destroy(_carObject);

            if (_enemyPrefabObject != null)
                Object.Destroy(_enemyPrefabObject);

            if (_enemyConfig != null)
                Object.Destroy(_enemyConfig);

            if (_levelConfig != null)
                Object.Destroy(_levelConfig);
        }

        [UnityTest]
        public IEnumerator ResetToStart_PlacesTheEnemyBeforeTheLevelBegins()
        {
            // The intro is watched from behind a parked car, so the road ahead has to
            // be populated before anything starts moving.
            Assert.That(_system.AliveCount, Is.EqualTo(1));
            Assert.That(_system.IsRunning, Is.False);

            yield break;
        }

        [UnityTest]
        public IEnumerator Enemy_ReachesTheCarAndSpendsItselfOnTheHit()
        {
            _system.Begin();

            yield return RunUntil(() => _carHitPoints.Current < CarHealth, frames: 200);

            Assert.That(_carHitPoints.Current, Is.EqualTo(CarHealth - _enemyConfig.DamageToCar),
                "The car took the wrong amount, or was hit more than once.");
            Assert.That(_system.AliveCount, Is.EqualTo(0),
                "The enemy survived its own attack and is still on the road.");
            Assert.That(_system.KillCount, Is.EqualTo(0),
                "An enemy that spends itself on the car is not a kill.");
        }

        [UnityTest]
        public IEnumerator Enemy_KilledByDamage_IsReportedAndReturnedToThePool()
        {
            _system.Begin();

            var view = Object.FindFirstObjectByType<EnemyView>();
            Assert.That(view, Is.Not.Null, "Nothing came out of the pool.");

            var reported = 0;
            var reportedAt = Vector3.zero;
            _system.Killed += position =>
            {
                reported++;
                reportedAt = position;
            };

            // Straight through the view, the way a bullet's cast arrives.
            view.TakeDamage(_enemyConfig.MaxHealth);

            yield return RunUntil(() => _system.AliveCount == 0, frames: 10);

            Assert.That(reported, Is.EqualTo(1), "The death was announced the wrong number of times.");
            Assert.That(reportedAt.z, Is.GreaterThan(0f), "The death was reported at the wrong place.");
            Assert.That(_system.KillCount, Is.EqualTo(1));
            Assert.That(_carHitPoints.Current, Is.EqualTo(CarHealth), "A dead enemy still reached the car.");
        }

        private IEnumerator RunUntil(System.Func<bool> done, int frames)
        {
            for (var i = 0; i < frames && !done(); i++)
            {
                yield return null;
                _system.Tick();
            }

            Assert.That(done(), Is.True, $"Nothing happened within {frames} frames.");
        }
    }
}
