using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.CameraSystem
{
    /// <summary>
    /// Changes the BundleAimProvider viewport point based on camera aim mode.
    /// 
    /// Purpose:
    /// - Keep hip-fire aiming centred.
    /// - Move aim-mode aiming slightly over the shoulder.
    /// - Ensure the crosshair and hit point remain aligned.
    /// </summary>
    public sealed class BundleAimViewportController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraModeController cameraModeController;
        [SerializeField] private BundleAimProvider aimProvider;

        [Header("Viewport Points")]
        [SerializeField] private Vector2 freeLookViewportPoint = new Vector2(0.5f, 0.5f);
        [SerializeField] private Vector2 aimViewportPoint = new Vector2(0.58f, 0.47f);

        private void OnEnable()
        {
            if (cameraModeController != null)
            {
                cameraModeController.AimModeChanged += OnAimModeChanged;
                Apply(cameraModeController.IsAiming);
            }
        }

        private void OnDisable()
        {
            if (cameraModeController != null)
            {
                cameraModeController.AimModeChanged -= OnAimModeChanged;
            }
        }

        private void OnAimModeChanged(bool isAiming)
        {
            Apply(isAiming);
        }

        private void Apply(bool isAiming)
        {
            if (aimProvider == null)
            {
                return;
            }

            Vector2 viewport = isAiming 
                ? aimViewportPoint 
                : freeLookViewportPoint;

            aimProvider.SetViewportPoint(viewport);
        }
    }
}