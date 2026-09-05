using DG.Tweening;
using UnityEngine;

namespace TurretRush.Combat
{
    /// <summary>
    /// Whites out a body for a moment when it is hit, by driving _FlashAmount on
    /// the Palette Flash shader.
    ///
    /// </summary>
    public sealed class HitFlash : MonoBehaviour
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [Tooltip("Every renderer that should light up. Skinned meshes included.")]
        [SerializeField] private Renderer[] renderers;

        [Tooltip("Short enough to read as an impact rather than a state.")]
        [SerializeField, Min(0.01f)] private float duration = 0.14f;

        private MaterialPropertyBlock _block;
        private Tween _tween;
        private float _amount;

        public void Play()
        {
            KillTween();

            _amount = 1f;
            Apply();

            _tween = DOTween.To(() => _amount, value =>
                {
                    _amount = value;
                    Apply();
                }, 0f, duration)
                .SetEase(Ease.OutQuad);
        }

        public void Clear()
        {
            KillTween();

            _amount = 0f;
            Apply();
        }

        private void Awake() => _block = new MaterialPropertyBlock();

        // Covers the pool: a body released mid-flash would otherwise come back white,
        // and its tween would keep writing to a renderer nobody is looking at.
        private void OnDisable() => Clear();

        /// <summary>
        /// IsActive() rather than a plain null check. A tween that finished on its own
        /// is auto-killed by DOTween, and the field is left pointing at a dead one.
        /// Right now DOTween is configured with recycling off, so killing a dead tween
        /// is merely pointless - but recycling is a single checkbox in the DOTween
        /// panel, and with it on that stale reference would eventually name a tween
        /// belonging to something else entirely. This kills only a tween that is still
        /// ours.
        /// </summary>
        private void KillTween()
        {
            if (_tween.IsActive())
                _tween.Kill();

            _tween = null;
        }

        private void Apply()
        {
            if (renderers == null || _block == null)
                return;

            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                renderers[i].GetPropertyBlock(_block);
                _block.SetFloat(FlashAmountId, _amount);
                renderers[i].SetPropertyBlock(_block);
            }
        }
    }
}
