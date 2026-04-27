using TPSCombatSystem.CameraSystem;
using TPSCombatSystem.Input;
using UnityEngine;

namespace TPSCombatSystem.Bridge
{
    /// <summary>
    /// Rotates the player root to match aim camera yaw while aiming.
    /// 
    /// Responsibilities:
    /// 1) Listen to aim state changes from <see cref="CombatInputActions"/>.
    /// 2) While aiming, smoothly rotate the player toward the aim camera yaw.
    /// 
    /// Notes:
    /// - Does not affect locomotion system directly.
    /// - Only applies horizontal (yaw) rotation.
    /// </summary>
    public sealed class PlayerAimFacing : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Transform playerRoot;
        [SerializeField] private CombatInputReader input;
        [SerializeField] private CombatAimCameraController aimCamera;

        [Header("Tuning")]
        [SerializeField] private float rotateSpeed = 14f;

        #endregion

        #region State

        private bool _isAiming;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if(playerRoot == null)
            {
                playerRoot = transform;
            }
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.AimChanged += OnAimChanged;
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
            if (!_isAiming)
            {
                return;
            }

            if (playerRoot == null || aimCamera == null)
            {
                return;
            }

            float targetYaw = aimCamera.CurrentYaw;

            Quaternion desiredRotation = Quaternion.Euler(0f, targetYaw, 0f);

            playerRoot.rotation = Quaternion.Slerp(
                playerRoot.rotation,
                desiredRotation,
                rotateSpeed * Time.deltaTime);
        }

        #endregion

        #region Event Handling

        private void OnAimChanged(bool aiming)
        {
            _isAiming = aiming;
        }

        #endregion
    }
}