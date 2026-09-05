using System;
using TurretRush.Config;
using TurretRush.Level;
using TurretRush.Player;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Combat
{
    /// <summary>
    /// Fires on its own once the level starts; the player only aims. Registered after
    /// <see cref="TurretAim"/> so shots leave a barrel already pointed this frame.
    /// </summary>
    public sealed class Weapon : IStartable, ITickable, IResettable
    {
        private readonly TurretView _view;
        private readonly ProjectileSystem _projectiles;
        private readonly FireTimer _timer;

        public Weapon(TurretView view, ProjectileSystem projectiles, WeaponConfig config)
        {
            _view = view;
            _projectiles = projectiles;
            _timer = new FireTimer(config.FireInterval, config.InitialDelay);
        }

        /// <summary>Raised once per round leaving the barrel.</summary>
        public event Action Fired;

        public bool IsFiring { get; private set; }

        public void Begin()
        {
            _timer.Reset();
            IsFiring = true;
        }

        public void Halt() => IsFiring = false;

        public void ResetToStart()
        {
            IsFiring = false;
            _timer.Reset();
        }

        public void Start() => ResetToStart();

        public void Tick()
        {
            if (!IsFiring)
                return;

            var shots = _timer.Step(Time.deltaTime);
            for (var i = 0; i < shots; i++)
            {
                // Spawned before the kick, so the recoil cannot move the muzzle the
                // bullet is leaving from.
                _projectiles.Spawn(_view.MuzzlePosition, _view.AimDirection);
                _view.PlayShot();
                Fired?.Invoke();
            }
        }
    }
}
