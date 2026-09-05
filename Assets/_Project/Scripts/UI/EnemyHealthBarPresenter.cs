using TurretRush.Enemies;
using TurretRush.Level;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// Shows a bar over every enemy that has been shot at least once.
    ///
    /// Hiding them until the first hit is what makes them readable: forty untouched
    /// enemies with full bars down the road would be noise, while a bar means "this
    /// one is already under fire, and here is how much is left" - which is the
    /// decision the player actually makes, keep firing or swing to the next.
    /// </summary>
    public sealed class EnemyHealthBarPresenter : ITickable, IResettable
    {
        private readonly EnemySystem _enemies;
        private readonly EnemyHealthBarLayer _layer;

        public EnemyHealthBarPresenter(EnemySystem enemies, EnemyHealthBarLayer layer)
        {
            _enemies = enemies;
            _layer = layer;
        }

        public void Tick()
        {
            // Bars belong to the level being played. Left running they would hang over
            // the wounded survivors standing behind a You lose screen.
            if (!_enemies.IsRunning)
            {
                _layer.HideAll();
                return;
            }

            _layer.BeginFrame();

            for (var i = 0; i < _enemies.AliveCount; i++)
            {
                var enemy = _enemies.GetLive(i);

                if (enemy.NormalizedHealth >= 1f)
                    continue;

                _layer.Draw(enemy.Position, enemy.NormalizedHealth);
            }

            _layer.EndFrame();
        }

        public void ResetToStart() => _layer.HideAll();
    }
}
