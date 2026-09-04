using TurretRush.Combat;
using TurretRush.Player;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// Temporary. Launches the car and the gun the moment play starts so driving,
    /// aiming and shooting can be judged before any UI exists.
    ///
    /// Replaced by GameFlow (GO button, camera intro, tap to start) once the flow is
    /// built. Nothing else depends on it, so deleting this file and its registration
    /// is the whole removal.
    /// </summary>
    public sealed class DebugAutoStart : IStartable
    {
        private readonly CarMovement _car;
        private readonly Weapon _weapon;

        public DebugAutoStart(CarMovement car, Weapon weapon)
        {
            _car = car;
            _weapon = weapon;
        }

        public void Start()
        {
            _car.Begin();
            _weapon.Begin();
        }
    }
}
