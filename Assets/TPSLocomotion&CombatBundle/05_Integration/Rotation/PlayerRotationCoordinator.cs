using TPSLocomotionCombatBundle.Integration.CameraSystem;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.Rotation
{
    /// <summary>
    /// Controls player rotation based on camera mode.
    /// 
    /// Responsibilities:
    /// - In free-look: do nothing (locomotion controls rotation)
    /// - In aim mode: rotate player to face camera forward (XZ plane)
    /// </summary>
    public sealed class PlayerRotationCoordinator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraModeController cameraModeController;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform cameraTransform;

        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 12f;

        private void LateUpdate()
        {
            if (cameraModeController == null || playerRoot == null || cameraTransform == null)
            {
                return;
            }

            if (!cameraModeController.IsAiming)
            {
                return;
            }

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);

            playerRoot.rotation = Quaternion.Slerp(
                playerRoot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
                );
        }
    }
}