using UnityEngine;
using UnityEngine.InputSystem;
using TPSCombatSystem.Interfaces;

namespace TPSCombatSystem.Aim
{
    /// <summary>
    /// Basic aim provider that generates an aim ray from a single camera.
    /// 
    /// Behaviour:
    /// - By default, uses the screen centre for TPS-style aiming.
    /// - Optionally uses the current mouse position for editor/testing scenarios.
    /// 
    /// Notes:
    /// - If no camera is assigned, this class falls back to <see cref="Camera.main"/>.
    /// - if no valid camera exists, it returns a safe forward ray from this transform.
    /// </summary>
    public sealed class CameraAimProvider : MonoBehaviour, IAimProvider
    {
        #region Inspecter Fields

        [Tooltip("Camera used to generate the aim ray.")]
        [SerializeField] private Camera aimCamera;

        [Tooltip("If true, use screen centre. If false, use current mouse position when available.")]
        [SerializeField] private bool useScreenCentre = true;

        #endregion

        #region Unity Lifestyle

        private void Reset()
        {
            // Auto-assign main camera for convenience.
            aimCamera = Camera.main;
        }

        #endregion

        #region IAimProvider

        /// <summary>
        /// Returns an aim ray generated from the configured camera.
        /// </summary>
        public Ray GetAimRay()
        {
            if (aimCamera == null)
            {
                // Fallback to Camera.main if not explicitly assigned.
                aimCamera = Camera.main;
            }

            if (aimCamera == null)
            {
                // Safe fallback if no camera is available at all.
                return new Ray(transform.position, transform.forward);
            }

            Vector3 screenPosition;

            if (useScreenCentre)
            {
                screenPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            }
            else
            {
                // Useful for editor testing or cursor-driven aim setups.
                screenPosition = Mouse.current != null
                    ? (Vector3)Mouse.current.position.ReadValue()
                    : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            }

            return aimCamera.ScreenPointToRay(screenPosition);
        }

        #endregion
    }
}