using DG.Tweening;
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

        /// <summary>
        /// Cuts the body loose from its hit points. A bullet resolved later in the
        /// same frame would otherwise land on an enemy that is already back in the
        /// pool, and damage whoever is unlucky enough to be spawned into that body
        /// next.
        /// </summary>
        public void Unbind() => _target = null;

        public void TakeDamage(int amount)
        {
            // Checked before forwarding so an already-dead body does not blink at a
            // bullet that was in flight when it died.
            if (_target == null || !_target.IsAlive)
                return;

            _target.TakeDamage(amount);

            if (hitFlash != null)
                hitFlash.Play();

            PlayHitReaction();
        }

        /// <summary>
        /// A short jerk backwards, layered over whatever the animator is doing rather
        /// than replacing it.
        ///
        /// A Mixamo hit clip would have to live on a masked override layer, and an
        /// override layer with an empty default state overrides the base with a
        /// default pose - so idle enemies would stand in a T-pose until shot. Getting
        /// that right needs the layer weight driven from code, which is more moving
        /// parts than this effect is worth. It would also fight the gameplay: three
        /// hits kill, fired 0.22 s apart, so a 0.8 s clip would never finish and the
        /// enemy would simply stop charging.
        /// </summary>
        private void PlayHitReaction()
        {
            if (body == null)
                return;

            StopHitReaction();

            // Negative pitch tips the top away from the car, which is where the shot
            // came from for anything actually charging at it.
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

        // A body released mid-flinch would otherwise come back out of the pool still
        // leaning, with a tween writing into it.
        private void OnDisable() => StopHitReaction();
    }
}
