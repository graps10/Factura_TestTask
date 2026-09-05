using System;

namespace TurretRush.World
{
    /// <summary>
    /// Ring buffer of ground tile positions: a tile that falls far enough behind is
    /// teleported to the front, so four of them cover any length of level.
    ///
    /// Knows nothing about Transforms on purpose. A seam in the road only shows up
    /// hundreds of metres in, so this is kept as arithmetic a test can drive across
    /// kilometres in milliseconds.
    /// </summary>
    public sealed class TileCycler
    {
        private readonly float[] _z;
        private readonly float _tileLength;
        private readonly float _firstTileZ;
        private readonly float _recycleDistance;

        private int _back;

        /// <param name="tilesBehind">How many tile lengths a tile must fall behind the
        /// viewer before it is recycled. Must exceed 0.5 so a tile is never moved
        /// while the viewer is still standing on it.</param>
        public TileCycler(int tileCount, float tileLength, float firstTileZ, float tilesBehind = 1.5f)
        {
            if (tileCount < 2)
                throw new ArgumentOutOfRangeException(nameof(tileCount), "At least two tiles are needed to cycle.");
            if (tileLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(tileLength), "Tile length must be positive.");
            if (tilesBehind <= 0.5f)
                throw new ArgumentOutOfRangeException(nameof(tilesBehind), "Must leave at least half a tile behind the viewer.");

            _z = new float[tileCount];
            _tileLength = tileLength;
            _firstTileZ = firstTileZ;
            _recycleDistance = tileLength * tilesBehind;

            Reset();
        }

        public int TileCount => _z.Length;

        /// <summary>Z of the far edge behind the viewer that is still covered by ground.</summary>
        public float CoverageStart => _z[_back] - _tileLength * 0.5f;

        /// <summary>Z of the far edge ahead of the viewer that is still covered by ground.</summary>
        public float CoverageEnd => CoverageStart + _tileLength * _z.Length;

        public float GetZ(int index) => _z[index];

        public void Reset()
        {
            for (var i = 0; i < _z.Length; i++)
                _z[i] = _firstTileZ + i * _tileLength;

            _back = 0;
        }

        /// <summary>
        /// Moves any tile that has fallen behind to the front. Returns how many tiles
        /// moved - zero on almost every frame, so the caller can skip writing to
        /// Transforms when nothing changed.
        /// </summary>
        public int Advance(float viewerZ)
        {
            var span = _tileLength * _z.Length;
            var moved = 0;

            // A loop, not a branch: one long frame or a restart can strand several.
            while (viewerZ - _z[_back] > _recycleDistance)
            {
                _z[_back] += span;
                _back = (_back + 1) % _z.Length;
                moved++;
            }

            return moved;
        }
    }
}
