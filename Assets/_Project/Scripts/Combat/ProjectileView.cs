using UnityEngine;

namespace TurretRush.Combat
{
    /// <summary>A single tracer. Moved by <see cref="ProjectileSystem"/>, not by
    /// itself.</summary>
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private TrailRenderer trail;

        public void PrepareForLaunch(Vector3 position, Vector3 direction)
        {
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));

            // After the move, not before: a pooled trail remembers where the last
            // bullet died and would paint a streak from there to the muzzle.
            if (trail != null)
                trail.Clear();
        }
    }
}
