using TurretRush.Config;
using TurretRush.Input;
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
    public sealed class TurretAim : IStartable, ITickable
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

        /// <summary>Aiming stays live before the level starts - the player is told to
        /// swipe before the car moves - and is switched off while paused.</summary>
        public bool IsEnabled { get; private set; } = true;

        public float CurrentYaw => _state.CurrentYaw;

        public void Enable() => IsEnabled = true;

        public void Disable() => IsEnabled = false;

        public void ResetToStart()
        {
            _state.Reset();
            _view.SetYaw(0f);
        }

        public void Start() => ResetToStart();

        public void Tick()
        {
            if (IsEnabled && _input.IsPressed)
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
