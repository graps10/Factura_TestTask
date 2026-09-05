using TurretRush.Config;
using TurretRush.Input;
using TurretRush.Level;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Player
{
    /// <summary>
    /// Turns drag into barrel rotation. The beam lives here too, because it is the
    /// same answer drawn on screen.
    /// </summary>
    public sealed class TurretAim : IStartable, ITickable, IResettable
    {
        private readonly TurretView _view;
        private readonly IInputService _input;
        private readonly TurretConfig _config;
        private readonly TurretAimState _state;

        public TurretAim(TurretView view, IInputService input, TurretConfig config)
        {
            _view = view;
            _input = input;
            _config = config;
            _state = new TurretAimState(config.MaxYaw, config.DegreesPerScreenWidth, config.SmoothTime);
        }

        /// <summary>Off until the flow says otherwise: aiming wakes one beat before
        /// the car, while the hint is on screen, and stops when the level is decided.</summary>
        public bool IsEnabled { get; private set; }

        public float CurrentYaw => _state.CurrentYaw;

        public void Enable() => IsEnabled = true;

        public void Disable()
        {
            IsEnabled = false;

            // Left drawn over a result screen it would suggest the player can shoot.
            _view.HideBeam();
        }

        public void ResetToStart()
        {
            Disable();
            _state.Reset();
            _view.SetYaw(0f);
            _view.ResetShot();
        }

        public void Start() => ResetToStart();

        public void Tick()
        {
            // No time passed means paused. Input keeps arriving regardless of the
            // time scale, so without this the barrel still swings behind the menu.
            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
                return;

            if (IsEnabled && _input.IsPressed)
                _state.AddInput(_input.DragDelta.x / Mathf.Max(1, Screen.width));

            _state.Step(deltaTime);

            // Every frame, not only while aiming is live: the pivot hangs off the
            // bodywork, and a skipped frame lets the barrel drift with the roll until
            // the next write snaps it level in one jump.
            _view.SetYaw(_state.CurrentYaw);

            if (IsEnabled)
                UpdateBeam();
        }

        private void UpdateBeam()
        {
            var length = _config.LaserMaxLength;

            // Stopping on the first enemy turns the beam from decoration into
            // feedback: on target is visible before the shot arrives.
            if (Physics.Raycast(
                    _view.MuzzlePosition,
                    _view.AimDirection,
                    out var hit,
                    length,
                    _config.LaserMask,
                    QueryTriggerInteraction.Collide))
            {
                length = hit.distance;
            }

            _view.ShowBeam(length);
        }
    }
}
