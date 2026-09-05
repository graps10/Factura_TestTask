using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TurretRush.Level;
using UnityEngine;
using UnityEngine.UI;

namespace TurretRush.UI
{
    /// <summary>
    /// The three full-screen panels the game flow steps through. Each method is a
    /// beat of that sequence and finishes when the beat is over, so the flow can be
    /// written as straight-line awaits instead of a callback per screen.
    /// </summary>
    public sealed class GameScreensView : MonoBehaviour
    {
        [Header("Start")]
        [SerializeField] private CanvasGroup startPanel;
        [SerializeField] private Button startButton;

        [Header("Hint")]
        [SerializeField] private CanvasGroup hintPanel;

        [Header("Result")]
        [SerializeField] private CanvasGroup resultPanel;
        [SerializeField] private TextMeshProUGUI resultLabel;
        [SerializeField] private string winText = "You win";
        [SerializeField] private string loseText = "You lose";

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.22f;

        private bool _startPressed;

        /// <summary>Fades the GO screen in, waits for the button, fades it out.</summary>
        public async UniTask WaitForStartAsync(CancellationToken cancellationToken)
        {
            _startPressed = false;

            await FadeAsync(startPanel, 1f, true, cancellationToken);
            await UniTask.WaitUntil(() => _startPressed, cancellationToken: cancellationToken);
            await FadeAsync(startPanel, 0f, false, cancellationToken);
        }

        public UniTask ShowHintAsync(CancellationToken cancellationToken)
            => FadeAsync(hintPanel, 1f, false, cancellationToken);

        public UniTask HideHintAsync(CancellationToken cancellationToken)
            => FadeAsync(hintPanel, 0f, false, cancellationToken);

        public UniTask ShowResultAsync(LevelResult result, CancellationToken cancellationToken)
        {
            if (resultLabel != null)
                resultLabel.text = result == LevelResult.Win ? winText : loseText;

            return FadeAsync(resultPanel, 1f, false, cancellationToken);
        }

        public UniTask HideResultAsync(CancellationToken cancellationToken)
            => FadeAsync(resultPanel, 0f, false, cancellationToken);

        public void HideAllImmediate()
        {
            SetImmediate(startPanel, 0f, false);
            SetImmediate(hintPanel, 0f, false);
            SetImmediate(resultPanel, 0f, false);
        }

        private void Awake()
        {
            // Added once, for the lifetime of the view. Nothing subscribes per screen,
            // so there is nothing to unsubscribe.
            if (startButton != null)
                startButton.onClick.AddListener(() => _startPressed = true);

            HideAllImmediate();
        }

        private async UniTask FadeAsync(
            CanvasGroup panel,
            float alpha,
            bool interactable,
            CancellationToken cancellationToken)
        {
            if (panel == null)
                return;

            // Raycasts are switched off before the fade rather than after, so a panel
            // on its way out cannot swallow the tap meant for whatever comes next.
            panel.blocksRaycasts = interactable;
            panel.interactable = interactable;

            panel.gameObject.SetActive(true);

            await panel.DOFade(alpha, fadeDuration)
                .SetUpdate(true)
                .ToUniTask(cancellationToken: cancellationToken);

            if (alpha <= 0f)
                panel.gameObject.SetActive(false);
        }

        private static void SetImmediate(CanvasGroup panel, float alpha, bool interactable)
        {
            if (panel == null)
                return;

            panel.DOKill();
            panel.alpha = alpha;
            panel.blocksRaycasts = interactable;
            panel.interactable = interactable;
            panel.gameObject.SetActive(alpha > 0f);
        }
    }
}
