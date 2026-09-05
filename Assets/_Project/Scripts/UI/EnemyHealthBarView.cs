using UnityEngine;
using UnityEngine.UI;

namespace TurretRush.UI
{
    /// <summary>One enemy's health bar, floating over its head.</summary>
    public sealed class EnemyHealthBarView : MonoBehaviour
    {
        [Tooltip("Image with Type set to Filled.")]
        [SerializeField] private Image fill;

        private RectTransform _rect;

        public void Show(Vector2 anchoredPosition, float normalized)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            _rect.anchoredPosition = anchoredPosition;

            if (fill != null)
                fill.fillAmount = Mathf.Clamp01(normalized);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        private void Awake() => _rect = (RectTransform)transform;
    }
}
