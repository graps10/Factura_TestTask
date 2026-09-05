using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Enemies;
using TurretRush.Input;
using TurretRush.Player;
using TurretRush.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// The single composition root of the game. Everything the gameplay needs is
    /// registered here and nowhere else, so there is exactly one place to look to
    /// see how the systems fit together.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private TurretConfig turretConfig;
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private EnemyConfig enemyConfig;

        [Header("Scene")]
        [SerializeField] private Camera cam;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelConfig);
            builder.RegisterInstance(carConfig);
            builder.RegisterInstance(cameraConfig);
            builder.RegisterInstance(turretConfig);
            builder.RegisterInstance(weaponConfig);
            builder.RegisterInstance(enemyConfig);

            builder.RegisterComponent(cam);
            builder.RegisterComponentInHierarchy<CarView>();
            builder.RegisterComponentInHierarchy<TurretView>();

            builder.Register<IInputService, PointerInputService>(Lifetime.Singleton);

            // The car's hit points, and the only Health the container hands out -
            // enemies build their own, one per body, as they come out of the pool.
            builder.Register(_ => new Health(carConfig.MaxHealth), Lifetime.Singleton);

            // Entry points run in registration order, and that order is the frame's
            // execution order. Read top to bottom it is the shape of one frame: the
            // car moves, the barrel is pointed, the gun fires down that barrel, the
            // bullets already in the air advance, the enemies react to where the car
            // now is, the road catches up, the camera looks at the result. Nothing
            // here reads a value another system has yet to write this frame.
            builder.RegisterEntryPoint<CarMovement>().AsSelf();
            builder.RegisterEntryPoint<TurretAim>().AsSelf();
            builder.RegisterEntryPoint<Weapon>().AsSelf();
            builder.RegisterEntryPoint<ProjectileSystem>().AsSelf();
            builder.RegisterEntryPoint<EnemySystem>().AsSelf();
            builder.RegisterEntryPoint<GroundStreamer>().AsSelf();
            builder.RegisterEntryPoint<CameraRig>().AsSelf();

            // Temporary, removed once GameFlow owns starting the level.
            builder.RegisterEntryPoint<DebugAutoStart>();
        }
    }
}
