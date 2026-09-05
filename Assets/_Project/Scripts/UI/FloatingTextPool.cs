using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace TurretRush.UI
{
    /// <summary>
    /// Pops a label over a point in the world. Lives on the canvas because every
    /// part of it is presentation: pooling labels, turning a world position into a
    /// canvas one, and animating the result.
    /// </summary>
    public sealed class FloatingTextPool : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private FloatingTextView prefab;
        [SerializeField] private RectTransform parent;

        [Header("Motion")]
        [Tooltip("How far up the screen the label drifts, in canvas units.")]
        [SerializeField, Min(0f)] private float rise = 90f;

        [SerializeField, Min(0.05f)] private float duration = 0.8f;

        [Tooltip("Lifts the label off the body it came from.")]
        [SerializeField] private Vector3 worldOffset = new(0f, 1.8f, 0f);

        [Header("Pool")]
        [SerializeField, Min(1)] private int capacity = 12;
        [SerializeField, Min(1)] private int maxSize = 32;

        private ObjectPool<FloatingTextView> _pool;
        private RectTransform _canvasRect;

        public void Show(Vector3 worldPosition, string text)
        {
            if (prefab == null || _pool == null)
                return;

            var view = _pool.Get();
            view.Present(text, ToCanvasPosition(worldPosition + worldOffset));

            // Released by the fade's own completion rather than by a timer somewhere
            // else, so retuning the animation cannot leave labels stuck on screen.
            view.Play(rise, duration).OnComplete(() => _pool.Release(view));
        }

        private void Awake()
        {
            _canvasRect = canvas != null ? (RectTransform)canvas.transform : null;

            _pool = new ObjectPool<FloatingTextView>(
                createFunc: () => Instantiate(prefab, parent != null ? parent : _canvasRect),
                actionOnGet: view => view.gameObject.SetActive(true),
                actionOnRelease: view =>
                {
                    if (view != null)
                        view.gameObject.SetActive(false);
                },
                actionOnDestroy: view =>
                {
                    if (view != null)
                        Destroy(view.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: capacity,
                maxSize: maxSize);
        }

        private void OnDestroy() => _pool?.Dispose();

        private Vector2 ToCanvasPosition(Vector3 worldPosition)
        {
            if (_canvasRect == null || worldCamera == null)
                return Vector2.zero;

            var screenPoint = worldCamera.WorldToScreenPoint(worldPosition);

            // An overlay canvas takes a null camera here; anything else needs the one
            // that renders it. Getting this wrong puts every label in a corner.
            var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPoint, uiCamera, out var local);

            return local;
        }
    }
}
