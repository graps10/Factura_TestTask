using System;
using TurretRush.Config;
using TurretRush.Enemies;
using UnityEngine;
using VContainer.Unity;

namespace TurretRush.Level
{
    /// <summary>
    /// Pays out for kills. The third listener on EnemySystem.Killed, next to the
    /// debris and the tally - which is why that event lives on the system: a new
    /// reason to care about a death costs one subscription, not a change to the enemy.
    /// </summary>
    public sealed class CoinReward : IStartable, IResettable, IDisposable
    {
        private readonly EnemySystem _enemies;
        private readonly CoinWallet _wallet;
        private readonly EnemyConfig _config;

        public CoinReward(EnemySystem enemies, CoinWallet wallet, EnemyConfig config)
        {
            _enemies = enemies;
            _wallet = wallet;
            _config = config;
        }

        /// <summary>Where it was earned and how much, for the floating label.</summary>
        public event Action<Vector3, int> Awarded;

        public void Start() => _enemies.Killed += OnEnemyKilled;

        public void ResetToStart() => _wallet.Reset();

        public void Dispose() => _enemies.Killed -= OnEnemyKilled;

        private void OnEnemyKilled(Vector3 position)
        {
            var amount = _config.CoinsPerKill;
            if (amount <= 0)
                return;

            _wallet.Add(amount);
            Awarded?.Invoke(position, amount);
        }
    }
}
