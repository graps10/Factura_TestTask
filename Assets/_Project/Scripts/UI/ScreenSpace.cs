using UnityEngine;

namespace TurretRush.UI
{
    /// <summary>
    /// Puts a point in the world onto the canvas. Shared, because getting it wrong is
    /// subtle: a screen point assigned straight to anchoredPosition looks right at the
    /// reference resolution and piles into a corner at any other.
    /// </summary>
    internal static class ScreenSpace
    {
        /// <summary>False when the point is behind the camera - WorldToScreenPoint
        /// mirrors those onto the screen, and they surface as labels for enemies the
        /// car has already passed.</summary>
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

            // An overlay canvas takes a null camera; anything else needs its own.
            var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform, screenPoint, uiCamera, out canvasPoint);
        }
    }
}
