using System.Collections.Generic;
using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Enemies;
using TurretRush.Player;

namespace TurretRush.Level
{
    /// <summary>
    /// The level as something that can be restarted, started, stopped and asked
    /// whether it is over. It knows the rules and nothing about screens; the order
    /// those appear in is <c>GameFlow</c>'s job.
    /// </summary>
    public sealed class LevelSession
    {
        private readonly IReadOnlyList<IResettable> _resettables;
        private readonly CarMovement _car;
        private readonly Weapon _weapon;
        private readonly EnemySystem _enemies;
        private readonly TurretAim _aim;
        private readonly Health _carHealth;
        private readonly CarConfig _carConfig;
        private readonly LevelProgress _progress;

        public LevelSession(
            IReadOnlyList<IResettable> resettables,
            CarMovement car,
            Weapon weapon,
            EnemySystem enemies,
            TurretAim aim,
            Health carHealth,
            CarConfig carConfig,
            LevelProgress progress)
        {
            _resettables = resettables;
            _car = car;
            _weapon = weapon;
            _enemies = enemies;
            _aim = aim;
            _carHealth = carHealth;
            _carConfig = carConfig;
            _progress = progress;
        }

        public float Progress => _progress.Evaluate(_car.DistanceTravelled);

        /// <summary>Null while the level is still being played.</summary>
        public LevelResult? Outcome
        {
            get
            {
                if (!_carHealth.IsAlive)
                    return LevelResult.Lose;

                if (_progress.IsComplete(_car.DistanceTravelled))
                    return LevelResult.Win;

                return null;
            }
        }

        public void ResetToStart()
        {
            // First, because the systems below react to it.
            _carHealth.Reset(_carConfig.MaxHealth);

            // Registration order, which is also tick order. It matters: the streamer
            // lays its tiles around wherever the car is, so the car has to be back on
            // the start line first.
            for (var i = 0; i < _resettables.Count; i++)
                _resettables[i].ResetToStart();
        }

        /// <summary>Aiming wakes before the car does, because the hint asks for a
        /// swipe while everything is still standing still.</summary>
        public void EnableAiming() => _aim.Enable();

        public void Begin()
        {
            _car.Begin();
            _weapon.Begin();
            _enemies.Begin();
        }

        public void Halt()
        {
            _car.Halt();
            _weapon.Halt();
            _enemies.Halt();
            _aim.Disable();
        }
    }
}
