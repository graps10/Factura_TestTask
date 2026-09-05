using System;

namespace TurretRush.Combat
{
    public sealed class Health : IDamageable
    {
        public event Action<int, int> Changed;

        /// <summary>Raised exactly once, on the hit that brings current to zero.</summary>
        public event Action Died;

        public int Max { get; private set; }

        public int Current { get; private set; }

        public bool IsAlive => Current > 0;

        public float Normalized => Max == 0 ? 0f : (float)Current / Max;

        public Health(int max) => Reset(max);

        public void TakeDamage(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Damage must not be negative.");

            if (!IsAlive || amount == 0)
                return;

            Current = Math.Max(0, Current - amount);
            Changed?.Invoke(Current, Max);

            if (Current == 0)
                Died?.Invoke();
        }

        /// <summary>Restores to full. Used when a body comes back out of a pool.</summary>
        public void Reset(int max)
        {
            if (max <= 0)
                throw new ArgumentOutOfRangeException(nameof(max), "Max health must be positive.");

            Max = max;
            Current = max;
            Changed?.Invoke(Current, Max);
        }
    }
}
