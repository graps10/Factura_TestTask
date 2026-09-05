using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TurretRush.Input;
using TurretRush.Level;
using TurretRush.Player;
using TurretRush.UI;
using TurretRush.World;
using VContainer.Unity;

namespace TurretRush.Bootstrap
{
    /// <summary>
    /// The whole shape of the game as one method read top to bottom. The same
    /// sequence as a state machine would be five states, a switch in Update and a
    /// set of flags recording which transition is half finished; as awaits, the
    /// ordering is enforced by the language.
    /// </summary>
    public sealed class GameFlow : IStartable, IDisposable
    {
        private readonly LevelSession _session;
        private readonly GameScreensView _screens;
        private readonly CameraRig _camera;
        private readonly HudView _hud;
        private readonly CarView _car;
        private readonly IInputService _input;

        private readonly CancellationTokenSource _lifetime = new();

        public GameFlow(
            LevelSession session,
            GameScreensView screens,
            CameraRig camera,
            HudView hud,
            CarView car,
            IInputService input)
        {
            _session = session;
            _screens = screens;
            _camera = camera;
            _hud = hud;
            _car = car;
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
                    ShowLevelReadouts(false);

                    await _screens.WaitForStartAsync(cancellationToken);
                    await _camera.PlayIntroAsync(cancellationToken);

                    _session.EnableAiming();
                    await _screens.ShowHintAsync(cancellationToken);
                    await WaitForTapAsync(cancellationToken);
                    await _screens.HideHintAsync(cancellationToken);

                    ShowLevelReadouts(true);
                    var result = await PlayLevelAsync(cancellationToken);

                    _session.Halt();
                    ShowLevelReadouts(false);

                    await _screens.ShowResultAsync(result, cancellationToken);
                    await WaitForTapAsync(cancellationToken);
                    await _screens.HideResultAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // The scene is going away; nothing here needs unwinding.
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

        /// <summary>Both belong to the level being played - and hiding the HUD also
        /// takes the pause button away everywhere pausing makes no sense.</summary>
        private void ShowLevelReadouts(bool visible)
        {
            _hud.SetVisible(visible);
            _car.SetHealthBarVisible(visible);
        }

        private UniTask WaitForTapAsync(CancellationToken cancellationToken)
            => UniTask.WaitUntil(() => _input.TapStarted, cancellationToken: cancellationToken);
    }
}
