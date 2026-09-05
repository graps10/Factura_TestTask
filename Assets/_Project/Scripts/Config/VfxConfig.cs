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

        [Header("Car hit")]
        [SerializeField, Min(0f)] private float hitShakeStrength = 0.35f;
        [SerializeField, Min(0.01f)] private float hitShakeDuration = 0.22f;

        public ParticleSystem EnemyDeathPrefab => enemyDeathPrefab;

        public int PoolCapacity => poolCapacity;

        public int PoolMaxSize => poolMaxSize;

        public float HitShakeStrength => hitShakeStrength;

        public float HitShakeDuration => hitShakeDuration;
    }
}
