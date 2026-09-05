using System.Collections.Generic;
using UnityEngine;

namespace TurretRush.UI
{
    /// <summary>
    /// Health bars over enemies, drawn by one canvas. A world-space Canvas on the
    /// prefab would cost one per pooled body, and a Canvas rebuilds whenever anything
    /// on it changes - twenty rebuilds a frame during a firefight.
    ///
    /// Immediate mode: the caller redraws what should be visible and anything it did
    /// not draw is hidden. Nothing remembers which bar belongs to which enemy, so a
    /// body returning to the pool cannot leave one behind.
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

        // Grows to the busiest moment of the level and stays there - a couple of
        // dozen bars, not worth handing back.
        private EnemyHealthBarView Take()
        {
            if (_used == _bars.Count)
                _bars.Add(Instantiate(prefab, parent != null ? parent : (RectTransform)canvas.transform));

            return _bars[_used++];
        }
    }
}
