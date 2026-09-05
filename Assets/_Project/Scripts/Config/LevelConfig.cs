using UnityEngine;

namespace TurretRush.Config
{
    /// <summary>
    /// Everything about the track itself: how long it is, and how the ground is
    /// tiled underneath the car.
    /// </summary>
    [CreateAssetMenu(menuName = "TurretRush/Level Config", fileName = "LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Header("Level")]
        [Tooltip("Distance in metres the car must cover to win.")]
        [SerializeField, Min(1f)] private float length = 450f;

        [Header("Ground")]
        [SerializeField] private Transform groundTilePrefab;

        [Tooltip("Length of one tile along Z. The supplied ground model is 75 m and " +
                 "its edges are authored to line up, so it tiles seamlessly.")]
        [SerializeField, Min(1f)] private float tileLength = 75f;

        [Tooltip("How many tiles exist at once. Four gives at least 75 m behind and " +
                 "150 m ahead of the car at all times.")]
        [SerializeField, Range(3, 8)] private int tileCount = 4;

        [Header("Road")]
        [Tooltip("Half the width of the asphalt. Measured from the model: 10.7 m.")]
        [SerializeField, Min(0f)] private float roadHalfWidth = 10.7f;

        [Tooltip("Keep-out strip next to the kerb so the car never straddles the edge.")]
        [SerializeField, Min(0f)] private float roadEdgeMargin = 2f;

        public float Length => length;

        public Transform GroundTilePrefab => groundTilePrefab;

        public float TileLength => tileLength;

        public int TileCount => tileCount;

        /// <summary>How far from the centre line the car is allowed to wander.</summary>
        public float DrivableHalfWidth => Mathf.Max(0f, roadHalfWidth - roadEdgeMargin);
    }
}
