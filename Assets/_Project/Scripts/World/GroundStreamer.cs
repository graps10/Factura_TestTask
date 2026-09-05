using System;
using TurretRush.Config;
using TurretRush.Level;
using TurretRush.Player;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace TurretRush.World
{
    /// <summary>Adapter between <see cref="TileCycler"/> and the scene. All the logic
    /// worth testing lives in the cycler.</summary>
    public sealed class GroundStreamer : IStartable, ITickable, IResettable, IDisposable
    {
        private readonly LevelConfig _config;
        private readonly CarMovement _car;

        private Transform _root;
        private Transform[] _tiles;
        private TileCycler _cycler;

        public GroundStreamer(LevelConfig config, CarMovement car)
        {
            _config = config;
            _car = car;
        }

        public void Start()
        {
            _root = new GameObject("Ground").transform;

            // One length behind the origin, so the car starts mid-tile with road
            // already visible in the mirror.
            _cycler = new TileCycler(_config.TileCount, _config.TileLength, -_config.TileLength);
            _tiles = new Transform[_cycler.TileCount];

            for (var i = 0; i < _tiles.Length; i++)
            {
                _tiles[i] = Object.Instantiate(_config.GroundTilePrefab, _root);
                _tiles[i].name = $"GroundTile_{i}";
            }

            SyncTransforms();
        }

        public void Tick()
        {
            if (_cycler.Advance(_car.DistanceTravelled) > 0)
                SyncTransforms();
        }

        public void ResetToStart()
        {
            _cycler.Reset();
            SyncTransforms();
        }

        public void Dispose()
        {
            if (_root != null)
                Object.Destroy(_root.gameObject);
        }

        private void SyncTransforms()
        {
            for (var i = 0; i < _tiles.Length; i++)
                _tiles[i].localPosition = new Vector3(0f, 0f, _cycler.GetZ(i));
        }
    }
}
