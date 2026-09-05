using System;
using TurretRush.Level;
using UnityEngine;
using UnityEngine.Events;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// Pause, in full.
    /// </summary>
    public sealed class PauseController : IStartable, IResettable, IDisposable
    {
        private readonly HudView _hud;
        private readonly PauseView _view;

        private UnityAction _onPausePressed;
        private UnityAction _onResumePressed;

        public PauseController(HudView hud, PauseView view)
        {
            _hud = hud;
            _view = view;
        }

        public bool IsPaused { get; private set; }

        public void Start()
        {
            _onPausePressed = () => SetPaused(true);
            _onResumePressed = () => SetPaused(false);

            if (_hud.PauseButton != null)
                _hud.PauseButton.onClick.AddListener(_onPausePressed);

            if (_view.ResumeButton != null)
                _view.ResumeButton.onClick.AddListener(_onResumePressed);

            SetPaused(false);
        }

        public void ResetToStart() => SetPaused(false);

        public void Dispose()
        {
            if (_hud.PauseButton != null)
                _hud.PauseButton.onClick.RemoveListener(_onPausePressed);

            if (_view.ResumeButton != null)
                _view.ResumeButton.onClick.RemoveListener(_onResumePressed);

            // Time scale is global and survives leaving play mode into the next
            // entry, so it is put back no matter how the scope goes away.
            Time.timeScale = 1f;
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            _view.SetVisible(paused);
        }
    }
}
