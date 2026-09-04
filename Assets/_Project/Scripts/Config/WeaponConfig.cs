using TurretRush.Combat;
using UnityEngine;

namespace TurretRush.Config
{
    [CreateAssetMenu(menuName = "TurretRush/Weapon Config", fileName = "WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("Rate of fire")]
        [Tooltip("Seconds between shots.")]
        [SerializeField, Min(0.01f)] private float fireInterval = 0.22f;

        [Tooltip("Quiet beat after the level starts, so the first shot lands after " +
                 "the player has had a moment to look at the road.")]
        [SerializeField, Min(0f)] private float initialDelay = 0.6f;

        [Header("Projectile")]
        [SerializeField] private ProjectileView projectilePrefab;
        [SerializeField, Min(1)] private int damage = 10;

        [Tooltip("Metres per second. Fast enough to feel like a rifle, slow enough " +
                 "that the tracer is readable and leading a runner matters.")]
        [SerializeField, Min(1f)] private float speed = 90f;

        [Tooltip("Cast radius. A little forgiveness so grazing shots still count.")]
        [SerializeField, Min(0.01f)] private float radius = 0.2f;

        [SerializeField, Min(10f)] private float maxRange = 200f;

        [Tooltip("What bullets can hit. Enemy only.")]
        [SerializeField] private LayerMask hitMask;

        [Header("Pool")]
        [SerializeField, Min(1)] private int poolCapacity = 32;
        [SerializeField, Min(1)] private int poolMaxSize = 128;

        public float FireInterval => fireInterval;

        public float InitialDelay => initialDelay;

        public ProjectileView ProjectilePrefab => projectilePrefab;

        public int Damage => damage;

        public float Speed => speed;

        public float Radius => radius;

        public float MaxRange => maxRange;

        public LayerMask HitMask => hitMask;

        public int PoolCapacity => poolCapacity;

        public int PoolMaxSize => poolMaxSize;
    }
}
