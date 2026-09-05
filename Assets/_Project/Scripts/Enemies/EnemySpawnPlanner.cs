using System;
using System.Collections.Generic;
using UnityEngine;

namespace TurretRush.Enemies
{
    public readonly struct SpawnPoint
    {
        public readonly float X;
        public readonly float Z;

        public SpawnPoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        public Vector3 ToPosition() => new(X, 0f, Z);
    }
    
    public sealed class EnemySpawnPlanner
    {
        private readonly float _fromZ;
        private readonly float _toZ;
        private readonly float _halfWidth;
        private readonly int _count;
        private readonly float _spacingMargin;

        /// <param name="spacingMargin">Fraction of each band left clear at both ends,
        /// which is what puts a floor under the gap between neighbours.</param>
        public EnemySpawnPlanner(float fromZ, float toZ, float halfWidth, int count, float spacingMargin)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "A level needs at least one enemy.");
            if (toZ <= fromZ)
                throw new ArgumentOutOfRangeException(nameof(toZ), "The populated stretch must have length.");
            if (halfWidth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(halfWidth), "Enemies need road to stand on.");

            _fromZ = fromZ;
            _toZ = toZ;
            _halfWidth = halfWidth;
            _count = count;
            _spacingMargin = Mathf.Clamp(spacingMargin, 0f, 0.45f);
        }

        public float BandLength => (_toZ - _fromZ) / _count;

        /// <summary>Guaranteed minimum gap along Z between neighbours.</summary>
        public float MinimumSpacing => BandLength * _spacingMargin * 2f;

        /// <summary>Ordered by Z, so the spawner can walk it with a single cursor.</summary>
        public IReadOnlyList<SpawnPoint> Plan(int seed)
        {
            var random = new System.Random(seed);
            var band = BandLength;
            var usable = 1f - _spacingMargin * 2f;
            var points = new SpawnPoint[_count];

            for (var i = 0; i < _count; i++)
            {
                var t = _spacingMargin + (float)random.NextDouble() * usable;
                var z = _fromZ + (i + t) * band;
                var x = ((float)random.NextDouble() * 2f - 1f) * _halfWidth;

                points[i] = new SpawnPoint(x, z);
            }

            return points;
        }
    }
}
