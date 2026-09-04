using TurretRush.Player;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// Temporary. Launches the car the moment play starts so the driving, streaming
    /// and camera can be judged before any UI exists.
    ///
    /// Replaced by GameFlow (GO button, camera intro, tap to start) once the flow is
    /// built. Nothing else depends on it, so deleting this file and its registration
    /// is the whole removal.
    /// </summary>
    public sealed class DebugAutoStart : IStartable
    {
        private readonly CarMovement _car;

        public DebugAutoStart(CarMovement car) => _car = car;

        public void Start() => _car.Begin();
    }
}
