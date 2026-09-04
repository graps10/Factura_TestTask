using UnityEngine;

namespace TurretRush.Combat
{
    /// <summary>
    /// A single tracer in the scene. Carries no movement of its own - the whole
    /// flight is stepped by <see cref="ProjectileSystem"/> in one loop, so a
    /// hundred bullets in flight cost one Update rather than a hundred.
    /// </summary>
    public sealed class ProjectileView : MonoBehaviour
    {
        [SerializeField] private TrailRenderer trail;

        public void PrepareForLaunch(Vector3 position, Vector3 direction)
        {
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));

            // Must happen after the move. A pooled trail still remembers where the
            // bullet died last time, and reusing it without clearing paints a streak
            // straight across the screen from the old position to the muzzle.
            if (trail != null)
                trail.Clear();
        }
    }
}
