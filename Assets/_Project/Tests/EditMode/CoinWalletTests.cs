using System;
using NUnit.Framework;
using TurretRush.Level;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class CoinWalletTests
    {
        [Test]
        public void NewWallet_IsEmpty()
        {
            Assert.That(new CoinWallet().Balance, Is.EqualTo(0));
        }

        [Test]
        public void Add_Accumulates()
        {
            var wallet = new CoinWallet();

            wallet.Add(10);
            wallet.Add(10);
            wallet.Add(5);

            Assert.That(wallet.Balance, Is.EqualTo(25));
        }

        [Test]
        public void Add_ReportsTheNewBalance()
        {
            var wallet = new CoinWallet();
            var reported = -1;
            wallet.Changed += balance => reported = balance;

            wallet.Add(10);

            Assert.That(reported, Is.EqualTo(10));
        }

        [Test]
        public void Add_Zero_ChangesNothingAndSaysNothing()
        {
            var wallet = new CoinWallet();
            var notifications = 0;
            wallet.Changed += _ => notifications++;

            wallet.Add(0);

            Assert.That(wallet.Balance, Is.EqualTo(0));
            Assert.That(notifications, Is.EqualTo(0), "An empty payout must not redraw the counter.");
        }

        [Test]
        public void Add_Negative_Throws()
        {
            var wallet = new CoinWallet();

            // Taking coins away is not a thing this game does, so a negative reward in
            // a config should fail loudly rather than quietly draining the counter.
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add(-5));
        }

        [Test]
        public void Reset_EmptiesTheWallet()
        {
            var wallet = new CoinWallet();
            wallet.Add(120);

            wallet.Reset();

            Assert.That(wallet.Balance, Is.EqualTo(0));
        }

        [Test]
        public void Reset_ReportsTheEmptyBalance()
        {
            var wallet = new CoinWallet();
            wallet.Add(120);

            var reported = -1;
            wallet.Changed += balance => reported = balance;
            wallet.Reset();

            // The counter has to be told, or a new run starts showing the last one's
            // total until the first kill.
            Assert.That(reported, Is.EqualTo(0));
        }

        [Test]
        public void Add_AfterReset_StartsFromZero()
        {
            var wallet = new CoinWallet();
            wallet.Add(120);
            wallet.Reset();

            wallet.Add(10);

            Assert.That(wallet.Balance, Is.EqualTo(10));
        }
    }
}
