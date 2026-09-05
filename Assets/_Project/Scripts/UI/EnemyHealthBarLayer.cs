using System.Collections.Generic;
using UnityEngine;

namespace TurretRush.UI
{
    /// <summary>
    /// Draws health bars over enemies, one canvas for all of them.
    ///
    /// The obvious alternative - a world-space Canvas on the enemy prefab - costs a
    /// separate Canvas per pooled body, and a Canvas rebuilds whenever anything on
    /// it changes. Twenty enemies taking fire would mean twenty rebuilds a frame,
    /// which is the most expensive thing UI does on a phone.
    ///
    /// The interface is immediate mode: the caller redraws whatever should be
    /// visible each frame and anything it did not draw is hidden. Nothing has to
    /// remember which bar belongs to which enemy, so a body returning to the pool
    /// cannot leave a bar behind.
    /// </summary>
    public sealed class EnemyHealthBarLayer : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private EnemyHealthBarView prefab;
        [SerializeField] private RectTransform parent;

        [Tooltip("Lifts the bar off the head. Enemies stand about 2.2 m tall.")]
        [SerializeField] private Vector3 worldOffset = new(0f, 2.6f, 0f);

        private readonly List<EnemyHealthBarView> _bars = new();

        private int _used;

        public void BeginFrame() => _used = 0;

        public void Draw(Vector3 worldPosition, float normalized)
        {
            if (prefab == null)
                return;

            if (!ScreenSpace.TryToCanvasPoint(canvas, worldCamera, worldPosition + worldOffset, out var point))
                return;

            Take().Show(point, normalized);
        }

        public void EndFrame()
        {
            for (var i = _used; i < _bars.Count; i++)
                _bars[i].Hide();
        }

        public void HideAll()
        {
            BeginFrame();
            EndFrame();
        }

        // The list only ever grows, to whatever the busiest moment of the level
        // needed. That ceiling is a couple of dozen bars, so there is nothing to be
        // gained by giving them back.
        private EnemyHealthBarView Take()
        {
            if (_used == _bars.Count)
                _bars.Add(Instantiate(prefab, parent != null ? parent : (RectTransform)canvas.transform));

            return _bars[_used++];
        }
    }
}
