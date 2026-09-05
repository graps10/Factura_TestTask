using DG.Tweening;
using UnityEngine;

namespace TurretRush.Player
{
    /// <summary>
    /// The turret's presence in the scene: the pivot that rotates, the point shots
    /// leave from, and the laser sight. Holds no aiming rules.
    /// </summary>
    public sealed class TurretView : MonoBehaviour
    {
        [Tooltip("Empty parent of the turret model. Its rotation is written in world " +
                 "space, so it must be a child of the car for position but must not " +
                 "have its own authored rotation.")]
        [SerializeField] private Transform yawPivot;

        [Tooltip("Tip of the barrel, forward axis pointing down the bore.")]
        [SerializeField] private Transform muzzle;

        [SerializeField] private LineRenderer laser;

        [Header("Firing")]
        [SerializeField] private ParticleSystem muzzleFlash;

        [Tooltip("The turret mesh. Kicked back on each shot.")]
        [SerializeField] private Transform recoilRoot;

        [SerializeField, Min(0f)] private float recoilDistance = 0.18f;

        [SerializeField, Min(0.02f)] private float recoilDuration = 0.12f;

        private Tween _recoil;

        public Vector3 MuzzlePosition => muzzle.position;

        public Vector3 AimDirection => muzzle.forward;

        public void SetYaw(float worldYawDegrees)
            => yawPivot.rotation = Quaternion.Euler(0f, worldYawDegrees, 0f);

        public void ShowBeam(float length)
        {
            if (laser == null)
                return;

            laser.enabled = true;
            laser.SetPosition(0, muzzle.position);
            laser.SetPosition(1, muzzle.position + muzzle.forward * length);
        }

        public void HideBeam()
        {
            if (laser != null)
                laser.enabled = false;
        }

        /// <summary>
        /// A position punch, not a rotation one: DOPunchPosition moves localPosition,
        /// so the muzzle slides back with its forward axis untouched. Tilting the bore
        /// would throw shots over the enemies it already passes close above.
        /// </summary>
        public void PlayShot()
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Clear(true);
                muzzleFlash.Play(true);
            }

            if (recoilRoot == null)
                return;

            KillRecoil();
            _recoil = recoilRoot.DOPunchPosition(Vector3.back * recoilDistance, recoilDuration, 1, 0.6f);
        }

        public void ResetShot()
        {
            KillRecoil();

            if (muzzleFlash != null)
                muzzleFlash.Clear(true);
        }

        private void KillRecoil()
        {
            if (_recoil.IsActive())
                _recoil.Kill();

            _recoil = null;
        }

        private void Awake()
        {
            if (laser == null)
                return;

            // Not trusted to the prefab: a line renderer left in local space drags the
            // beam along with the turret and bends it.
            laser.useWorldSpace = true;
            laser.positionCount = 2;
        }
    }
}
