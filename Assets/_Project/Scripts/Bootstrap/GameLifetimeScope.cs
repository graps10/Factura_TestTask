using TurretRush.Combat;
using TurretRush.Config;
using TurretRush.Enemies;
using TurretRush.Input;
using TurretRush.Level;
using TurretRush.Player;
using TurretRush.UI;
using TurretRush.Vfx;
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
        [SerializeField] private VfxConfig vfxConfig;

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
            builder.RegisterInstance(vfxConfig);

            builder.RegisterComponent(cam);
            builder.RegisterComponentInHierarchy<CarView>();
            builder.RegisterComponentInHierarchy<TurretView>();
            builder.RegisterComponentInHierarchy<GameScreensView>();
            builder.RegisterComponentInHierarchy<HudView>();
            builder.RegisterComponentInHierarchy<PauseView>();
            builder.RegisterComponentInHierarchy<FloatingTextPool>();
            builder.RegisterComponentInHierarchy<EnemyHealthBarLayer>();

            builder.Register<IInputService, PointerInputService>(Lifetime.Singleton);

            // The car's hit points, and the only Health the container hands out -
            // enemies build their own, one per body, as they come out of the pool.
            builder.Register(_ => new Health(carConfig.MaxHealth), Lifetime.Singleton);
            builder.Register(_ => new LevelProgress(levelConfig.Length), Lifetime.Singleton);
            builder.Register<CoinWallet>(Lifetime.Singleton);
            builder.Register<LevelSession>(Lifetime.Singleton);

            builder.RegisterEntryPoint<DisplaySettings>();

            // Entry points run in registration order, and that order is the frame's
            // execution order. Read top to bottom it is the shape of one frame: the
            // car moves, the barrel is pointed, the gun fires down that barrel, the
            // bullets already in the air advance, the enemies react to where the car
            // now is, the road catches up, the camera looks at the result.
            //
            // It is also the order a restart runs in, since LevelSession walks every
            // IResettable in registration order - which is what puts the car back on
            // the start line before the ground streamer lays its tiles around it.
            builder.RegisterEntryPoint<CarMovement>().AsSelf();
            builder.RegisterEntryPoint<TurretAim>().AsSelf();
            builder.RegisterEntryPoint<Weapon>().AsSelf();
            builder.RegisterEntryPoint<ProjectileSystem>().AsSelf();
            builder.RegisterEntryPoint<EnemySystem>().AsSelf();
            builder.RegisterEntryPoint<GroundStreamer>().AsSelf();
            builder.RegisterEntryPoint<CameraRig>().AsSelf();

            // Presentation only. Both hang off events rather than being called by the
            // systems that raise them, so gameplay stays unaware that feedback exists
            // and either could be removed without touching a rule.
            builder.RegisterEntryPoint<VfxSystem>().AsSelf();
            builder.RegisterEntryPoint<CarFeedback>();
            builder.RegisterEntryPoint<CoinReward>().AsSelf();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<PopupPresenter>();
            builder.RegisterEntryPoint<EnemyHealthBarPresenter>();
            builder.RegisterEntryPoint<PauseController>();

            // Registered last so its Start runs after every system has placed itself,
            // and the first thing the player sees is a settled scene.
            builder.RegisterEntryPoint<GameFlow>();
        }
    }
}
