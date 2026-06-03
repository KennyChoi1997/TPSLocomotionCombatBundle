using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.CameraSystem
{
    /// <summary>
    /// Applies a shoulder-style camera offset during aim mode.
    /// 
    /// Responsibilities:
    /// - Keep free-look camera centred.
    /// - Move the camera slightly to the right while aiming.
    /// - Maintain a single active camera.
    /// 
    /// This class does not rotate the player.
    /// Rotation should remain handled by PlayerRotationCoordinator.
    /// </summary>
    public sealed class CameraShoulderOffsetController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CameraModeController cameraModeController;
        [SerializeField] private Transform shoulderPivot;

        [Header("Offsets")]
        [SerializeField] private Vector3 freeLookLocalOffset = Vector3.zero;

        [SerializeField] private Vector3 aimLocalOffset = new Vector3(0.65f, 0.05f, 0f);

        [Header("Smoothing")]
        [SerializeField] private float lerpSpeed = 12f;

        private void LateUpdate()
        {
            if (cameraModeController == null || shoulderPivot == null)
            {
                return;
            }

            Vector3 targetOffset = 
                cameraModeController.IsAiming 
                ? aimLocalOffset 
                : freeLookLocalOffset;

            shoulderPivot.localPosition = Vector3.Lerp(
                shoulderPivot.localPosition,
                targetOffset,
                lerpSpeed * Time.deltaTime
                );
        }
    }
}