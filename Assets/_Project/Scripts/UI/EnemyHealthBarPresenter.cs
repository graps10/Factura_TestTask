using TurretRush.Enemies;
using TurretRush.Level;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// A bar over every enemy that has been shot at least once. Hidden until then,
    /// because a road lined with full bars is noise - a bar that appears means "this
    /// one is already under fire", which is the choice the player is making.
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
            // Otherwise they hang over the wounded survivors behind a result screen.
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
