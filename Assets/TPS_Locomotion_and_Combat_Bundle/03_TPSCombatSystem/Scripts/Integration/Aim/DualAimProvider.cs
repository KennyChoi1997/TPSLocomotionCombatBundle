using TPSCombatSystem.Interfaces;
using TPSCombatSystem.Input;
using TPSCombatSystem.CameraSystem;
using UnityEngine;

namespace TPSCombatSystem.Aim
{
    /// <summary>
    /// Aim ray provider that switches between free-look and aim camera sources.
    /// 
    /// Behaviour:
    /// - Not aiming: returns a ray from the free-look camera.
    /// - Aiming: returns a ray from the aim camera controller.
    /// 
    /// Notes:
    /// - This class does not depend on locomotion systems directly.
    /// - It only selects between already assigned camera sources.
    /// </summary>
    public sealed class DualAimProvider : MonoBehaviour, IAimProvider
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private CombatInputReader input;
        [SerializeField] private Camera freeLookCamera;
        [SerializeField] private CombatAimCameraController aimController;

        [Header("Ray")]
        [Tooltip("Viewport position used when generating a ray from the free-look camera.")]
        [SerializeField] private Vector2 viewportPoint = new Vector2(0.5f, 0.5f);

        #endregion

        #region State

        private bool isAiming;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (input != null)
            {
                input.AimChanged += OnAimChanged;
                isAiming = input.IsAiming;
            }
                
        }

        private void OnDisable()
        {
            if(input != null)
            {
                input.AimChanged -= OnAimChanged;
            }    
        }

        #endregion

        #region Event Handling

        private void OnAimChanged(bool aiming)
        {
            isAiming = aiming;
        }

        #endregion

        #region IAimProvider

        /// <summary>
        /// Returns the current aim ray based on whether the player is aiming.
        /// </summary>
        public Ray GetAimRay()
        {
            if (isAiming && aimController != null)
            {
                // Aim camera is inside aimController
                // Use its public method if you prefer
                return aimController.GetCentreAimRay();
            }

            Camera cameraSource = freeLookCamera != null ? freeLookCamera : Camera.main;
            if (cameraSource == null)
            {
                // Fail-safe fallback when no camera is available.
                return new Ray(transform.position, transform.forward);
            }

            return cameraSource.ViewportPointToRay(
                new Vector3(viewportPoint.x, viewportPoint.y, 0f)
                );
        }

        #endregion
    }
}