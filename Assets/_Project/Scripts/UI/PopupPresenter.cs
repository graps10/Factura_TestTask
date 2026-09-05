using System;
using TurretRush.Combat;
using TurretRush.Enemies;
using TurretRush.Level;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// Everything that pops up over the world: what a kill paid, and what a shot
    /// took off. Split from the HUD presenter because the two answer different
    /// questions - one keeps a permanent readout current, this one reacts to
    /// moments - and because both were starting to make one class too wide.
    /// </summary>
    public sealed class PopupPresenter : IStartable, IDisposable
    {
        private readonly FloatingTextPool _popups;
        private readonly CoinReward _reward;
        private readonly ProjectileSystem _projectiles;
        private readonly EnemySystem _enemies;

        public PopupPresenter(
            FloatingTextPool popups,
            CoinReward reward,
            ProjectileSystem projectiles,
            EnemySystem enemies)
        {
            _popups = popups;
            _reward = reward;
            _projectiles = projectiles;
            _enemies = enemies;
        }

        public void Start()
        {
            _reward.Awarded += OnCoinsAwarded;
            _projectiles.Hit += OnShotLanded;
            _enemies.CarHit += OnCarHit;
        }

        public void Dispose()
        {
            _reward.Awarded -= OnCoinsAwarded;
            _projectiles.Hit -= OnShotLanded;
            _enemies.CarHit -= OnCarHit;
        }

        private void OnCoinsAwarded(Vector3 position, int amount) => _popups.ShowCoins(position, amount);

        private void OnShotLanded(Vector3 point, int damage) => _popups.ShowDamage(point, damage);

        private void OnCarHit(Vector3 point, int damage) => _popups.ShowPlayerDamage(point, damage);
    }
}
