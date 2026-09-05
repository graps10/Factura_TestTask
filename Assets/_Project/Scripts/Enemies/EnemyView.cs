using TurretRush.Combat;
using UnityEngine;

namespace TurretRush.Enemies
{
    /// <summary>
    /// One enemy's body in the scene: the collider bullets sweep against and the
    /// animator that shows what it is doing. It carries no hit points and no
    /// behaviour - <see cref="EnemySystem"/> owns both and binds them on spawn.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour, IDamageable
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");

        [SerializeField] private Animator animator;

        private IDamageable _target;

        public bool IsAlive => _target != null && _target.IsAlive;

        public void Bind(IDamageable target) => _target = target;

        /// <summary>
        /// Cuts the body loose from its hit points. A bullet resolved later in the
        /// same frame would otherwise land on an enemy that is already back in the
        /// pool, and damage whoever is unlucky enough to be spawned into that body
        /// next.
        /// </summary>
        public void Unbind() => _target = null;

        public void TakeDamage(int amount) => _target?.TakeDamage(amount);

        public void PrepareForSpawn(Vector3 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);

            if (animator == null)
                return;

            // A pooled animator resumes wherever the previous occupant left off,
            // which shows up as enemies appearing mid-stride or frozen in a pose.
            animator.Rebind();
            animator.Update(0f);
        }

        public void SetSpeed(float metresPerSecond)
        {
            if (animator != null)
                animator.SetFloat(SpeedId, metresPerSecond);
        }

        public void FaceDirection(Vector3 direction, float degreesPerSecond, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction),
                degreesPerSecond * deltaTime);
        }

        public void MoveTo(Vector3 position) => transform.position = position;
    }
}
