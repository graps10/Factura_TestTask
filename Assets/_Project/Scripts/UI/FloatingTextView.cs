using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TurretRush.UI
{
    /// <summary>One "+10" on its way up the screen.</summary>
    public sealed class FloatingTextView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup group;

        private RectTransform _rect;

        public void Present(string text, Vector2 anchoredPosition)
        {
            // Any tween from the previous use has to go before the new position is
            // written, or the pooled label finishes drifting to where the last one
            // was heading.
            Stop();

            _rect.anchoredPosition = anchoredPosition;
            group.alpha = 1f;
            label.text = text;
        }

        /// <summary>Returns the fade, which is the one that decides when it is done.</summary>
        public Tween Play(float rise, float duration)
        {
            _rect.DOAnchorPos(_rect.anchoredPosition + Vector2.up * rise, duration)
                .SetEase(Ease.OutCubic);

            return group.DOFade(0f, duration).SetEase(Ease.InQuad);
        }

        public void Stop()
        {
            _rect.DOKill();
            group.DOKill();
        }

        private void Awake() => _rect = (RectTransform)transform;

        private void OnDisable() => Stop();
    }
}
