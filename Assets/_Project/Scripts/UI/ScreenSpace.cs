using UnityEngine;

namespace TurretRush.UI
{
    /// <summary>
    /// Puts a point in the world onto the canvas. Shared by every overlay that
    /// tracks something in the scene, because getting it slightly wrong is subtle:
    /// assigning a screen point straight to anchoredPosition looks right at the
    /// reference resolution and piles everything into a corner at any other.
    /// </summary>
    internal static class ScreenSpace
    {
        /// <summary>
        /// False when the point is behind the camera. Worth checking: WorldToScreenPoint
        /// happily returns a mirrored on-screen position for anything behind the lens,
        /// which shows up as labels for enemies the player has already driven past.
        /// </summary>
        public static bool TryToCanvasPoint(
            Canvas canvas,
            Camera worldCamera,
            Vector3 worldPosition,
            out Vector2 canvasPoint)
        {
            canvasPoint = Vector2.zero;

            if (canvas == null || worldCamera == null)
                return false;

            var screenPoint = worldCamera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z < 0f)
                return false;

            // An overlay canvas takes a null camera here; anything else needs the one
            // that renders it.
            var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform, screenPoint, uiCamera, out canvasPoint);
        }
    }
}
