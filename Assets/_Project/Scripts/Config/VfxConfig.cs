using UnityEngine;

namespace TurretRush.Config
{
    [CreateAssetMenu(menuName = "TurretRush/Vfx Config", fileName = "VfxConfig")]
    public sealed class VfxConfig : ScriptableObject
    {
        [Header("Enemy death")]
        [Tooltip("One-shot burst played where an enemy falls.")]
        [SerializeField] private ParticleSystem enemyDeathPrefab;

        [SerializeField, Min(1)] private int poolCapacity = 12;
        [SerializeField, Min(1)] private int poolMaxSize = 32;

        [Header("Car destroyed")]
        [SerializeField] private ParticleSystem carExplosionPrefab;

        [SerializeField, Min(0f)] private float explosionShakeStrength = 1.1f;
        [SerializeField, Min(0.01f)] private float explosionShakeDuration = 0.6f;

        [Header("Car hit")]
        [SerializeField, Min(0f)] private float hitShakeStrength = 0.35f;
        [SerializeField, Min(0.01f)] private float hitShakeDuration = 0.22f;

        public ParticleSystem EnemyDeathPrefab => enemyDeathPrefab;

        public int PoolCapacity => poolCapacity;

        public int PoolMaxSize => poolMaxSize;

        public ParticleSystem CarExplosionPrefab => carExplosionPrefab;

        public float ExplosionShakeStrength => explosionShakeStrength;

        public float ExplosionShakeDuration => explosionShakeDuration;

        public float HitShakeStrength => hitShakeStrength;

        public float HitShakeDuration => hitShakeDuration;
    }
}
