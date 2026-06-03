using LocomotionSystem.CameraSystem;
using TPSCombatSystem.Input;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.CameraSystem
{
    /// <summary>
    /// Controls camera distance for the integrated locomotion + combat bundle.
    /// 
    /// Responsibilities:
    /// - Listen to combat aim input.
    /// - Blend the gameplay camera distance between free-look and aim mode.
    /// - Keep the bundle using a single active camera.
    /// 
    /// This class does not rotate the player.
    /// Player rotation should be handled by PlayerRotationCoordinator.
    /// </summary>
    public sealed class CameraModeController : MonoBehaviour
    {
        public enum CameraMode
        {
            FreeLook,
            Aim
        }

        #region Inspector Fields

        [Header("Input")]
        [SerializeField] private CombatInputReader input;

        [Header("Locomotion Camera")]
        [SerializeField] private ThirdPersonCameraController thirdPersonCamera;

        [Header("Distance")]
        [SerializeField] private float freeLookDistance = 3f;
        [SerializeField] private float aimDistance = 2.1f;
        [SerializeField] private float distanceLerpSpeed = 12f;

        #endregion

        #region State

        private CameraMode currentMode = CameraMode.FreeLook;
        private float currentDistance;

        #endregion

        #region Public Properties

        public CameraMode CurrentMode => currentMode;
        public bool IsAiming => currentMode == CameraMode.Aim;
        public event System.Action<bool> AimModeChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (thirdPersonCamera != null)
            {
                currentDistance = thirdPersonCamera.GetFreeLookDistance();
                freeLookDistance = currentDistance;
            }
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.AimChanged += OnAimChanged;
                SetMode(input.IsAiming ? CameraMode.Aim : CameraMode.FreeLook);
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.AimChanged -= OnAimChanged;
            }
        }

        private void LateUpdate()
        {
            if (thirdPersonCamera == null)
            {
                return;
            }

            float targetDistance = currentMode == CameraMode.Aim 
                ? aimDistance 
                : freeLookDistance;

            currentDistance = Mathf.Lerp(
                currentDistance,
                targetDistance,
                distanceLerpSpeed * Time.deltaTime
                );

            thirdPersonCamera.SetFreeLookDistance(currentDistance);
        }

        #endregion

        #region Event Handling

        private void OnAimChanged(bool aiming)
        {
            SetMode(aiming ? CameraMode.Aim : CameraMode.FreeLook);
        }

        #endregion

        #region Public API

        public void SetMode(CameraMode mode)
        {
            if (currentMode == mode)
            {
                return;
            }

            currentMode = mode;
            AimModeChanged?.Invoke(currentMode == CameraMode.Aim);
        }

        #endregion
    }
}