using TurretRush.Config;
using TurretRush.Input;
using TurretRush.Level;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Player
{
    /// <summary>
    /// Turns drag into barrel rotation and keeps the laser sight on the resulting
    /// line. Everything about "where is the turret pointing" lives here, including
    /// the beam, because the beam is that answer drawn on screen rather than a
    /// separate concern.
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

        /// <summary>
        /// Off until the flow says otherwise. Aiming comes alive one beat before the
        /// car does, while the hint is telling the player to swipe, and goes away
        /// again the moment the level is decided.
        /// </summary>
        public bool IsEnabled { get; private set; }

        public float CurrentYaw => _state.CurrentYaw;

        public void Enable() => IsEnabled = true;

        public void Disable()
        {
            IsEnabled = false;

            // The beam is the sight, so it belongs to aiming. Leaving it drawn over a
            // result screen would suggest the player can still shoot.
            _view.HideBeam();
        }

        public void ResetToStart()
        {
            Disable();
            _state.Reset();
            _view.SetYaw(0f);
        }

        public void Start() => ResetToStart();

        public void Tick()
        {
            if (!IsEnabled)
                return;

            if (_input.IsPressed)
                _state.AddInput(_input.DragDelta.x / Mathf.Max(1, Screen.width));

            _state.Step(Time.deltaTime);
            _view.SetYaw(_state.CurrentYaw);

            UpdateBeam();
        }

        private void UpdateBeam()
        {
            var length = _config.LaserMaxLength;

            // Stopping the beam on the first enemy in the way turns it from decoration
            // into feedback: the player can see they are on target before the shot
            // has finished travelling.
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
