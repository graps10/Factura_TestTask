using UnityEngine;
using UnityEngine.UI;

namespace TurretRush.UI
{
    /// <summary>The pause overlay. Shown and hidden by <see cref="PauseController"/>.</summary>
    public sealed class PauseView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Button resumeButton;

        public Button ResumeButton => resumeButton;

        public void SetVisible(bool visible)
        {
            if (group == null)
                return;

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private void Awake() => SetVisible(false);
    }
}
