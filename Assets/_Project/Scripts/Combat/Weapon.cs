using TurretRush.Config;
using TurretRush.Level;
using TurretRush.Player;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Combat
{
    /// <summary>
    /// Fires on its own once the level starts - the player aims, the gun keeps
    /// time. Registered after <see cref="TurretAim"/> so shots leave a barrel that
    /// has already been pointed this frame rather than trailing the beam by one.
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
                // Spawned before the kick, so the bullet leaves the barrel where the
                // player was aiming rather than where the recoil has just put it.
                _projectiles.Spawn(_view.MuzzlePosition, _view.AimDirection);
                _view.PlayShot();
            }
        }
    }
}
