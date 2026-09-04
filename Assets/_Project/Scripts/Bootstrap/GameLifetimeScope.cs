using TurretRush.Config;
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

        [Header("Scene")]
        [SerializeField] private Camera cam;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(levelConfig);
            builder.RegisterInstance(carConfig);
            builder.RegisterInstance(cameraConfig);

            builder.RegisterComponent(cam);
            builder.RegisterComponentInHierarchy<CarView>();

            // Entry points run in registration order, and that order is the frame's
            // execution order. The car moves first; everything that reads its position
            // in the same frame comes after it, so nothing lags a frame behind.
            builder.RegisterEntryPoint<CarMovement>().AsSelf();
            builder.RegisterEntryPoint<GroundStreamer>().AsSelf();
            builder.RegisterEntryPoint<CameraRig>().AsSelf();

            // Temporary, removed once GameFlow owns starting the level.
            builder.RegisterEntryPoint<DebugAutoStart>();
        }
    }
}
