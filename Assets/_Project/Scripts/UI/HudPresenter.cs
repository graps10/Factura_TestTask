using System;
using TurretRush.Combat;
using TurretRush.Level;
using VContainer.Unity;

namespace TurretRush.UI
{
    /// <summary>
    /// Keeps the permanent readouts current. Health and coins are pushed on change,
    /// because they change rarely and redrawing them every frame would be work for
    /// nothing; progress is pulled every frame, because it changes every frame anyway.
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

            // The events only fire on change, so the opening values have to be pushed
            // by hand or the HUD sits empty until the first hit.
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
