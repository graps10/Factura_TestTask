using System;
using TurretRush.Combat;
using TurretRush.Level;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// Keeps the permanent readouts current. Health and coins are pushed on change,
    /// progress is pulled every frame - because that is how often each one moves.
    /// </summary>
    public sealed class HudPresenter : IStartable, ITickable, IDisposable
    {
        private readonly HudView _hud;
        private readonly LevelSession _session;
        private readonly Health _carHealth;
        private readonly CoinWallet _wallet;

        public HudPresenter(HudView hud, LevelSession session, Health carHealth, CoinWallet wallet)
        {
            _hud = hud;
            _session = session;
            _carHealth = carHealth;
            _wallet = wallet;
        }

        public void Start()
        {
            _carHealth.Changed += OnHealthChanged;
            _wallet.Changed += OnCoinsChanged;

            // Events only fire on change, so the opening values are pushed by hand.
            OnHealthChanged(_carHealth.Current, _carHealth.Max);
            OnCoinsChanged(_wallet.Balance);
            _hud.SetProgress(0f);
        }

        public void Tick() => _hud.SetProgress(_session.Progress);

        public void Dispose()
        {
            _carHealth.Changed -= OnHealthChanged;
            _wallet.Changed -= OnCoinsChanged;
        }

        private void OnHealthChanged(int current, int max)
            => _hud.SetHealth(max == 0 ? 0f : (float)current / max);

        private void OnCoinsChanged(int balance) => _hud.SetCoins(balance);
    }
}
