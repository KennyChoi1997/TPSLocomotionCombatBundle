using LocomotionSystem.Core;
using LocomotionSystem.Input;
using TPSLocomotionCombatBundle.Integration.CameraSystem;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.Rotation
{
    /// <summary>
    /// Controls player rotation based on camera mode.
    /// 
    /// Responsibilities:
    /// - In free-look / lock-on: do nothing.
    /// - In combat aim mode: disable locomotion-owned rotation.
    /// - In combat aim mode: rotate player to face camera forward.
    /// - In combat aim mode while backward: suppress rotation for backpedaling.
    /// </summary>
    public sealed class PlayerRotationCoordinator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraModeController cameraModeController;
        [SerializeField] private PlayerInputReader locomotionInput;
        [SerializeField] private PlayerLocomotion locomotion;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform cameraTransform;

        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Backpedal")]
        [SerializeField] private bool suppressRotationWhileBAckpedaling = true;
        [SerializeField] private float backpedalThreshold = -0.2f;

        private void LateUpdate()
        {
            if (cameraModeController == null || playerRoot == null || cameraTransform == null)
            {
                return;
            }

            bool isAiming = cameraModeController.IsAiming;

            // FreeLook / Lock-On / Soulslike movement:
            // Let PLayerLocomotion own rotation.
            locomotion?.SetAllowRotation(!isAiming);

            if (!isAiming)
            {
                return;
            }

            // Combat aim + backward input:
            // Neither locomotion nor this coordinator rotates the player.
            if (ShouldSuppressRotation())
            {
                return;
            }

            RotatePlayerToCameraForward();
        }

        private bool ShouldSuppressRotation()
        {
            if (!suppressRotationWhileBAckpedaling || locomotionInput == null)
            {
                return false;
            }

            return locomotionInput.MoveInput.y < backpedalThreshold;
        }

        private void RotatePlayerToCameraForward()
        {
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

        private void OnDisable()
        {
            // Safety: restore locomotion rotation if this component is disabled.
            locomotion?.SetAllowRotation(true);
        }
    }
}