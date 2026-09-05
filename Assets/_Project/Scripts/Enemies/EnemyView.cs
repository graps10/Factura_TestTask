using DG.Tweening;
using TurretRush.Combat;
using UnityEngine;

namespace TurretRush.Enemies
{
    /// <summary>
    /// One enemy's body: the collider bullets sweep against and the animator that
    /// shows what it is doing. Hit points and behaviour belong to
    /// <see cref="EnemySystem"/>, which binds them on spawn.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour, IDamageable
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");

        [SerializeField] private Animator animator;

        [SerializeField] private HitFlash hitFlash;

        [Header("Hit reaction")]
        [Tooltip("The child carrying the Animator. Its own local rotation is free - " +
                 "the animator writes to the bones under it, not to it.")]
        [SerializeField] private Transform body;

        [SerializeField, Range(0f, 45f)] private float hitTiltDegrees = 14f;

        [SerializeField, Min(0.02f)] private float hitDuration = 0.2f;

        private IDamageable _target;
        private Tween _hitTween;

        public bool IsAlive => _target != null && _target.IsAlive;

        public void Bind(IDamageable target) => _target = target;

        /// <summary>Cuts the body loose from its hit points, so a bullet resolved
        /// later in the same frame cannot wound whoever is spawned into it next.</summary>
        public void Unbind() => _target = null;

        public void TakeDamage(int amount)
        {
            // So a dead body does not blink at a bullet that was already in the air.
            if (_target == null || !_target.IsAlive)
                return;

            _target.TakeDamage(amount);

            if (hitFlash != null)
                hitFlash.Play();

            PlayHitReaction();
        }

        /// <summary>
        /// A jerk on the animated child's own local rotation, which the animator does
        /// not write to - so it layers over the run instead of interrupting it. An
        /// animation clip would need a masked override layer, and such a layer replaces
        /// the base with a default pose in its empty state: idle enemies would stand in
        /// a T-pose until shot.
        /// </summary>
        private void PlayHitReaction()
        {
            if (body == null)
                return;

            StopHitReaction();

            // Negative pitch tips away from the car, where the shot came from.
            _hitTween = body.DOPunchRotation(
                new Vector3(-hitTiltDegrees, 0f, 0f),
                hitDuration,
                vibrato: 1,
                elasticity: 0.6f);
        }

        private void StopHitReaction()
        {
            if (_hitTween.IsActive())
                _hitTween.Kill();

            _hitTween = null;

            if (body != null)
                body.localRotation = Quaternion.identity;
        }

        public void PrepareForSpawn(Vector3 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            StopHitReaction();

            if (animator == null)
                return;

            // A pooled animator otherwise resumes where the previous occupant left
            // off, and the enemy appears mid-stride.
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

        // A body released mid-flinch would come back out of the pool still leaning.
        private void OnDisable() => StopHitReaction();
    }
}
