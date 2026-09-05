using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TurretRush.Input;
using TurretRush.Level;
using TurretRush.UI;
using TurretRush.World;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// The whole shape of the game, as one method you can read top to bottom.
    /// </summary>
    public sealed class GameFlow : IStartable, IDisposable
    {
        private readonly LevelSession _session;
        private readonly GameScreensView _screens;
        private readonly CameraRig _camera;
        private readonly HudView _hud;
        private readonly IInputService _input;

        private readonly CancellationTokenSource _lifetime = new();

        public GameFlow(
            LevelSession session,
            GameScreensView screens,
            CameraRig camera,
            HudView hud,
            IInputService input)
        {
            _session = session;
            _screens = screens;
            _camera = camera;
            _hud = hud;
            _input = input;
        }

        public void Start() => RunAsync(_lifetime.Token).Forget();

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private async UniTaskVoid RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _session.ResetToStart();
                    _camera.SnapToIntro();
                    _screens.HideAllImmediate();
                    _hud.SetVisible(false);

                    await _screens.WaitForStartAsync(cancellationToken);
                    await _camera.PlayIntroAsync(cancellationToken);

                    _session.EnableAiming();
                    await _screens.ShowHintAsync(cancellationToken);
                    await WaitForTapAsync(cancellationToken);
                    await _screens.HideHintAsync(cancellationToken);

                    // The HUD belongs to the level, not to the menus around it - and
                    // hiding it also takes the pause button away everywhere pausing
                    // would make no sense.
                    _hud.SetVisible(true);
                    var result = await PlayLevelAsync(cancellationToken);

                    _session.Halt();
                    _hud.SetVisible(false);

                    await _screens.ShowResultAsync(result, cancellationToken);
                    await WaitForTapAsync(cancellationToken);
                    await _screens.HideResultAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // The scene is going away. Everything this loop touched is either
                // already gone or about to be, so there is nothing to unwind.
            }
        }

        private async UniTask<LevelResult> PlayLevelAsync(CancellationToken cancellationToken)
        {
            _session.Begin();

            await UniTask.WaitUntil(
                () => _session.Outcome.HasValue,
                cancellationToken: cancellationToken);

            return _session.Outcome.Value;
        }

        private UniTask WaitForTapAsync(CancellationToken cancellationToken)
            => UniTask.WaitUntil(() => _input.TapStarted, cancellationToken: cancellationToken);
    }
}
