using System;
using System.Linq;
using NUnit.Framework;
using TurretRush.World;

namespace TurretRush.Tests
{
    [TestFixture]
    public sealed class TileCyclerTests
    {
        private const int TileCount = 4;
        private const float TileLength = 75f;
        private const float FirstTileZ = -TileLength;

        private static TileCycler Create() => new TileCycler(TileCount, TileLength, FirstTileZ);

        private static float[] SortedPositions(TileCycler cycler)
            => Enumerable.Range(0, cycler.TileCount).Select(cycler.GetZ).OrderBy(z => z).ToArray();

        [Test]
        public void Constructor_LaysTilesOutEndToEnd()
        {
            var cycler = Create();
            var z = SortedPositions(cycler);

            Assert.That(z[0], Is.EqualTo(FirstTileZ));
            for (var i = 1; i < z.Length; i++)
                Assert.That(z[i] - z[i - 1], Is.EqualTo(TileLength).Within(0.0001f));
        }

        [Test]
        public void Advance_LeavesTilesAloneWhileTheViewerIsStillNearby()
        {
            var cycler = Create();

            // Recycle distance is 1.5 tiles behind, so the first tile at -75 survives
            // until the viewer passes 37.5.
            Assert.That(cycler.Advance(37f), Is.EqualTo(0));
            Assert.That(SortedPositions(cycler), Is.EqualTo(new[] { -75f, 0f, 75f, 150f }));
        }

        [Test]
        public void Advance_MovesTheTrailingTileToTheFront()
        {
            var cycler = Create();

            var moved = cycler.Advance(40f);

            Assert.That(moved, Is.EqualTo(1));
            Assert.That(SortedPositions(cycler), Is.EqualTo(new[] { 0f, 75f, 150f, 225f }));
        }

        [Test]
        public void Advance_CatchesUpAfterALargeJump()
        {
            var cycler = Create();

            // A single call must handle a teleport, not recycle one tile per frame and
            // leave the viewer standing over a hole for the next few frames.
            cycler.Advance(1000f);

            Assert.That(cycler.CoverageStart, Is.LessThanOrEqualTo(1000f));
            Assert.That(cycler.CoverageEnd, Is.GreaterThanOrEqualTo(1000f));
            AssertTilesStayContiguous(cycler);
        }

        [Test]
        public void Advance_KeepsGroundUnderAndAheadOfTheViewerForAWholeDrive()
        {
            var cycler = Create();
            var random = new System.Random(1234);
            var viewerZ = 0f;

            // The property that actually matters: at no point during a long drive,
            // at any frame rate, may the road run out behind or in front of the car.
            // 40 m behind covers the camera; 140 m ahead covers what it can see.
            while (viewerZ < 5000f)
            {
                viewerZ += (float)random.NextDouble() * 3f;
                cycler.Advance(viewerZ);

                Assert.That(cycler.CoverageStart, Is.LessThanOrEqualTo(viewerZ - 40f),
                    $"Road ran out behind the car at {viewerZ} m.");
                Assert.That(cycler.CoverageEnd, Is.GreaterThanOrEqualTo(viewerZ + 140f),
                    $"Road ran out ahead of the car at {viewerZ} m.");
                AssertTilesStayContiguous(cycler);
            }
        }

        [Test]
        public void Reset_RestoresTheStartingLayout()
        {
            var cycler = Create();
            cycler.Advance(2000f);

            cycler.Reset();

            Assert.That(SortedPositions(cycler), Is.EqualTo(new[] { -75f, 0f, 75f, 150f }));
        }

        [Test]
        public void Reset_AllowsTheCyclerToBeDrivenAgain()
        {
            var cycler = Create();
            cycler.Advance(2000f);
            cycler.Reset();

            var moved = cycler.Advance(40f);

            Assert.That(moved, Is.EqualTo(1));
            Assert.That(SortedPositions(cycler), Is.EqualTo(new[] { 0f, 75f, 150f, 225f }));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void Constructor_TooFewTiles_Throws(int tileCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new TileCycler(tileCount, TileLength, FirstTileZ));
        }

        [Test]
        public void Constructor_NonPositiveTileLength_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TileCycler(TileCount, 0f, 0f));
        }

        [Test]
        public void Constructor_RecyclingTooCloseBehindTheViewer_Throws()
        {
            // Below half a tile the viewer would be standing on the tile as it moves.
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new TileCycler(TileCount, TileLength, FirstTileZ, tilesBehind: 0.5f));
        }

        private static void AssertTilesStayContiguous(TileCycler cycler)
        {
            var z = SortedPositions(cycler);

            for (var i = 1; i < z.Length; i++)
                Assert.That(z[i] - z[i - 1], Is.EqualTo(TileLength).Within(0.01f),
                    "Tiles drifted apart — there is a seam in the road.");
        }
    }
}
