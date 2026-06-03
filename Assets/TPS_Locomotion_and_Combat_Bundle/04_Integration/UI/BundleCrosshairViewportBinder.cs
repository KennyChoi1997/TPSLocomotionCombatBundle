using TPSLocomotionCombatBundle.Integration.CameraSystem;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.UI
{
    /// <summary>
    /// Places the crosshair UI at the same viewport point used by BundleAimProvider.
    /// 
    /// This ensures:
    /// - The visible crosshair position
    /// - The camera aim ray
    /// - The actual hit point
    /// 
    /// all use the same screen-space aiming reference.
    /// </summary>
    public sealed class BundleCrosshairViewportBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BundleAimProvider aimProvider;
        [SerializeField] private RectTransform crosshairRect;
        [SerializeField] private Canvas canvas;

        [Header("Behaviour")]

        [Tooltip("Continuously update the crosshair position. Disable if the viewport point never changes.")]
        [SerializeField] private bool updateEveryFrame = true;

        private void OnEnable()
        {
            ApplyPosition();
        }

        private void LateUpdate()
        {
            if (!updateEveryFrame)
            {
                return;
            }

            ApplyPosition();
        }

        public void ApplyPosition()
        {
            if (aimProvider == null || crosshairRect == null)
            {
                return;
            }

            RectTransform canvasRect = GetCanvasRect();
            if (canvasRect == null)
            {
                return;
            }

            Vector2 viewport = aimProvider.ViewportPoint;

            float x = (viewport.x - 0.5f) * canvasRect.rect.width;
            float y = (viewport.y - 0.5f) * canvasRect.rect.height;

            crosshairRect.anchoredPosition = new Vector2(x, y);
        }

        private RectTransform GetCanvasRect()
        {
            if (canvas != null)
            {
                return canvas.transform as RectTransform;
            }

            Canvas parentCanvas = crosshairRect.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                return null;
            }

            return parentCanvas.transform as RectTransform;
        }
    }
}