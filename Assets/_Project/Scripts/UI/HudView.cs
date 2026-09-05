using DG.Tweening;
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

        [Tooltip("How much the counter jumps when it pays out. Zero disables it.")]
        [SerializeField, Range(0f, 1f)] private float coinPunch = 0.35f;

        [SerializeField, Min(0.05f)] private float coinPunchDuration = 0.25f;

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
            if (coinLabel == null)
                return;

            coinLabel.SetText("{0}", balance);

            // Not on the way back to zero: that is a restart, not a payout.
            if (coinPunch <= 0f || balance == 0)
                return;

            var scale = coinLabel.transform;
            scale.DOKill();
            scale.localScale = Vector3.one;

            scale.DOPunchScale(Vector3.one * coinPunch, coinPunchDuration, 1, 0.6f)
                .SetUpdate(true);
        }

        /// <summary>Alpha rather than SetActive, so the container can still find the
        /// components under here by hierarchy.</summary>
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
