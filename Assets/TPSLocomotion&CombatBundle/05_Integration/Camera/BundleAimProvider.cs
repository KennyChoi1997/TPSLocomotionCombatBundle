using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.CameraSystem
{
    /// <summary>
    /// Bundle-specific aim provider for the integrated locomotion + combat demo.
    /// 
    /// Purpose:
    /// - Provides a centre-screen aim ray from a single active camera.
    /// - Avoids switching between separate Freelook and Aim cameras.
    /// - Keeps combat aiming independent from the locomotion camera implementation.
    /// 
    /// Recommended usage:
    /// - Assign this component to ShooterCore.Aim Provider Behaviour.
    /// - Use Main Camera or the current active gameplay camera as Camera Source.
    /// </summary>
    public sealed class BundleAimProvider : MonoBehaviour, IAimProvider
    {
        #region Inspector Fields

        [Header("Camera")]
        [Tooltip("Camera used to generate the aim ray. If empty, Camera.main will be used.")]
        [SerializeField] private Camera cameraSource;

        [Header("Ray")]
        [Tooltip("Viewport position used generating the aim ray.")]
        [SerializeField] private Vector2 viewportPoint = new Vector2(0.5f, 0.5f);

        [Header("Fallback")]
        [Tooltip("Fallback transform used when no camera is available.")]
        [SerializeField] private Transform fallbackForwardSource;

        [Header("Debug")]
        [SerializeField] private bool enableDebugRay = true;
        [SerializeField] private float debugRayLength = 50f;

        public Vector2 ViewportPoint => viewportPoint;

        #endregion

        #region IAimProvider

        /// <summary>
        /// Returns an aim ray from the assigned gameplay camera.
        /// </summary>
        public Ray GetAimRay()
        {
            Camera activeCamera = cameraSource != null ? cameraSource : Camera.main;

            if (activeCamera != null)
            {
                Ray ray = activeCamera.ViewportPointToRay(
                    new Vector3(viewportPoint.x, viewportPoint.y, 0f)
                    );

                if (enableDebugRay)
                {
                    Debug.DrawRay(ray.origin, ray.direction * debugRayLength);
                }
                return ray;
            }

            Transform fallback = fallbackForwardSource != null ? fallbackForwardSource : transform;
            Ray fallbackRay = new Ray(fallback.position, fallback.forward);

            if (enableDebugRay)
            {
                Debug.DrawRay(fallbackRay.origin, fallbackRay.direction * debugRayLength);
            }

            return fallbackRay;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Allows integration controllers to replace the camera source at runtime.
        /// </summary>
        public void SetCameraSource(Camera newCameraSource)
        {
            cameraSource = newCameraSource;
        }

        public Camera GetCameraSource()
        {
            return cameraSource != null ? cameraSource : Camera.main;
        }

        public void SetViewportPoint(Vector2 newViewportPoint)
        {
            viewportPoint = newViewportPoint;
        }

        #endregion
    }
}