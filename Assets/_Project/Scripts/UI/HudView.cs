using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TurretRush.UI
{
    /// <summary>
    /// The in-game readouts. Setters only - it is told what to show and never asks.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;

        [Header("Car")]
        [Tooltip("Image with Type set to Filled.")]
        [SerializeField] private Image healthFill;

        [Header("Level")]
        [Tooltip("Image with Type set to Filled, filling upwards.")]
        [SerializeField] private Image progressFill;

        [Header("Coins")]
        [SerializeField] private TextMeshProUGUI coinLabel;

        [Header("Pause")]
        [SerializeField] private Button pauseButton;

        public Button PauseButton => pauseButton;

        public void SetHealth(float normalized)
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Clamp01(normalized);
        }

        public void SetProgress(float normalized)
        {
            if (progressFill != null)
                progressFill.fillAmount = Mathf.Clamp01(normalized);
        }

        public void SetCoins(int balance)
        {
            if (coinLabel != null)
                coinLabel.SetText("{0}", balance);
        }

        /// <summary>
        /// Alpha rather than SetActive: the pause button is a child, and deactivating
        /// the object it lives on would make the container unable to find it again if
        /// anything ever looked for it by hierarchy.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (group == null)
                return;

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
