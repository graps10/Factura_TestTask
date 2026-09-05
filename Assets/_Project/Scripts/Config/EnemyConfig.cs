using TurretRush.Enemies;
using UnityEngine;

namespace TurretRush.Config
{
    [CreateAssetMenu(menuName = "TurretRush/Enemy Config", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Prefab")]
        [SerializeField] private EnemyView prefab;
        [SerializeField, Min(1)] private int poolCapacity = 24;
        [SerializeField, Min(1)] private int poolMaxSize = 64;

        [Header("Stats")]
        [SerializeField, Min(1)] private int maxHealth = 30;
        [SerializeField, Min(1)] private int damageToCar = 12;

        [Header("Movement")]
        [Tooltip("Metres per second while charging the car. Must stay under the car's " +
                 "speed, or an enemy noticed late could never be outrun.")]
        [SerializeField, Min(0.1f)] private float runSpeed = 6.5f;

        [Tooltip("How far the enemy strays from where it spawned while idle.")]
        [SerializeField, Min(0f)] private float wanderRadius = 1.4f;

        [SerializeField, Min(0.01f)] private float wanderFrequency = 0.55f;

        [SerializeField, Min(1f)] private float turnSpeed = 540f;

        [Header("Aggro")]
        [Tooltip("Distance at which an idle enemy notices the car and charges.")]
        [SerializeField, Min(1f)] private float detectRadius = 24f;

        [Tooltip("Slack added around the car's collider before a charging enemy is " +
                 "counted as having reached it.")]
        [SerializeField, Min(0f)] private float impactMargin = 0.35f;

        [Header("Population")]
        [Tooltip("Changing this reshuffles the whole level while keeping it repeatable.")]
        [SerializeField] private int seed = 20260904;

        [SerializeField, Min(1)] private int count = 45;

        [Tooltip("Opening stretch with no enemies, so the player sees the road before " +
                 "anything runs at them.")]
        [SerializeField, Min(0f)] private float safeStartDistance = 40f;

        [SerializeField, Min(0f)] private float finishMargin = 20f;

        [Tooltip("Fraction of each band kept clear at both ends. Higher values spread " +
                 "enemies more evenly; zero lets two neighbours end up touching.")]
        [SerializeField, Range(0f, 0.45f)] private float spacingMargin = 0.25f;

        [Header("Streaming")]
        [Tooltip("How far ahead of the car enemies come out of the pool. Must be less " +
                 "than the ground the streamer guarantees, or they appear in mid-air.")]
        [SerializeField, Min(10f)] private float activationLead = 110f;

        [Tooltip("Anything this far behind the car is returned to the pool unkilled.")]
        [SerializeField, Min(1f)] private float despawnBehind = 25f;

        public EnemyView Prefab => prefab;

        public int PoolCapacity => poolCapacity;

        public int PoolMaxSize => poolMaxSize;

        public int MaxHealth => maxHealth;

        public int DamageToCar => damageToCar;

        public float RunSpeed => runSpeed;

        public float WanderRadius => wanderRadius;

        public float WanderFrequency => wanderFrequency;

        public float TurnSpeed => turnSpeed;

        public float DetectRadius => detectRadius;

        public float ImpactMargin => impactMargin;

        public int Seed => seed;

        public int Count => count;

        public float SafeStartDistance => safeStartDistance;

        public float FinishMargin => finishMargin;

        public float SpacingMargin => spacingMargin;

        public float ActivationLead => activationLead;

        public float DespawnBehind => despawnBehind;
    }
}
