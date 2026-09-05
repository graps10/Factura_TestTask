using System;

namespace TurretRush.Level
{
    /// <summary>Coins earned during a run. Plain state with a change event, so the
    /// counter and the floating label are both just listeners.</summary>
    public sealed class CoinWallet
    {
        public event Action<int> Changed;

        public int Balance { get; private set; }

        public void Add(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Coins cannot be taken away by adding.");

            if (amount == 0)
                return;

            Balance += amount;
            Changed?.Invoke(Balance);
        }

        public void Reset()
        {
            Balance = 0;
            Changed?.Invoke(Balance);
        }
    }
}
