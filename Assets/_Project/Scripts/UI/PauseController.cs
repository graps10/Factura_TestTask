using System;
using TurretRush.Level;
using UnityEngine;
using UnityEngine.Events;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// Pause, in full. It is this small because every gameplay system already steps
    /// itself by Time.deltaTime, so zeroing the time scale stops the car, the enemies,
    /// the bullets and the rate of fire at once. The two things it does not stop are
    /// handled where they live: UI tweens run unscaled, and TurretAim ignores a frame
    /// in which no time passed.
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

            // Time scale is global and survives into the next play session.
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
