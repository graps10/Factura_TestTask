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

        [Tooltip("Lifts a payout clear of the body it came from. Kept well above the " +
                 "height shots land at, so a coin and a damage number from two " +
                 "different enemies do not end up on the same line.")]
        [SerializeField] private Vector3 worldOffset = new(0f, 2.9f, 0f);

        [Header("Coins")]
        [SerializeField] private Color coinColor = new(1f, 0.84f, 0.22f);
        [SerializeField, Min(0.1f)] private float coinScale = 1f;

        [Header("Damage")]
        [Tooltip("Deliberately quieter than the coins. The payout is the reward and " +
                 "should win the eye; the damage number is secondary chatter.")]
        [SerializeField] private Color damageColor = new(1f, 1f, 1f, 0.8f);

        [SerializeField, Min(0.1f)] private float damageScale = 0.65f;

        [Header("Damage taken")]
        [Tooltip("Louder than the damage dealt. Losing health is the one number the " +
                 "player has to notice.")]
        [SerializeField] private Color playerDamageColor = new(1f, 0.29f, 0.24f);

        [SerializeField, Min(0.1f)] private float playerDamageScale = 0.95f;

        [Header("Pool")]
        [SerializeField, Min(1)] private int capacity = 12;
        [SerializeField, Min(1)] private int maxSize = 48;

        private ObjectPool<FloatingTextView> _pool;

        public void ShowCoins(Vector3 worldPosition, int amount)
            => Show(worldPosition + worldOffset, "+{0}", amount, coinColor, coinScale);

        public void ShowDamage(Vector3 worldPosition, int amount)
            => Show(worldPosition, "{0}", amount, damageColor, damageScale);

        public void ShowPlayerDamage(Vector3 worldPosition, int amount)
            => Show(worldPosition + worldOffset, "-{0}", amount, playerDamageColor, playerDamageScale);

        private void Awake()
        {
            _pool = new ObjectPool<FloatingTextView>(
                createFunc: () => Instantiate(prefab, parent != null ? parent : (RectTransform)canvas.transform),
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

        private void Show(Vector3 worldPosition, string format, int value, Color color, float scale)
        {
            if (prefab == null || _pool == null)
                return;

            if (!ScreenSpace.TryToCanvasPoint(canvas, worldCamera, worldPosition, out var point))
                return;

            var view = _pool.Get();
            view.Present(point, format, value, color, scale);

            // Released by the fade's own completion rather than by a timer somewhere
            // else, so retuning the animation cannot leave labels stuck on screen.
            view.Play(rise, duration).OnComplete(() => _pool.Release(view));
        }
    }
}
