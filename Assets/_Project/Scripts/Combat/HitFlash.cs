using DG.Tweening;
using UnityEngine;

namespace TurretRush.Combat
{
    /// <summary>
    /// Whites out a body for a moment when it is hit, by driving _FlashAmount on the
    /// Palette Flash shader. Written through a MaterialPropertyBlock, because
    /// touching renderer.material would clone the shared material once per pooled
    /// body.
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

        // A body released mid-flash would come back out of the pool still white.
        private void OnDisable() => Clear();

        // IsActive() rather than a null check: a finished tween is auto-killed and
        // leaves the field pointing at a dead one. Harmless while DOTween recycling
        // is off, but recycling is one checkbox away, and then that stale reference
        // would name a tween belonging to something else.
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
