using UnityEngine;

namespace TurretRush.Config
{
    [CreateAssetMenu(menuName = "TurretRush/Turret Config", fileName = "TurretConfig")]
    public sealed class TurretConfig : ScriptableObject
    {
        [Header("Aiming")]
        [Tooltip("How far either side of straight ahead the barrel can swing.")]
        [SerializeField, Range(10f, 89f)] private float maxYaw = 60f;

        [Tooltip("Degrees swept by a drag across the full screen width. Expressed " +
                 "per screen width rather than per pixel so the gesture feels the " +
                 "same on every device instead of scaling with resolution.")]
        [SerializeField, Min(1f)] private float degreesPerScreenWidth = 130f;

        [Tooltip("Takes the jitter out of raw touch deltas. Keep it small - this is " +
                 "direct manipulation, and anything above ~0.1 reads as lag. Zero " +
                 "disables smoothing entirely.")]
        [SerializeField, Range(0f, 0.3f)] private float smoothTime = 0.05f;

        [Header("Laser sight")]
        [SerializeField, Min(1f)] private float laserMaxLength = 200f;

        [Tooltip("What the beam stops on. Enemy only.")]
        [SerializeField] private LayerMask laserMask;

        public float MaxYaw => maxYaw;

        public float DegreesPerScreenWidth => degreesPerScreenWidth;

        public float SmoothTime => smoothTime;

        public float LaserMaxLength => laserMaxLength;

        public LayerMask LaserMask => laserMask;
    }
}
