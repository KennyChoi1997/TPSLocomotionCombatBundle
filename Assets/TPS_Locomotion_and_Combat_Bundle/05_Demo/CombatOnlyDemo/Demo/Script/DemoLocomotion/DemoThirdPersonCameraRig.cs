using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Demo-only third-person camera-rig (no Cinemachine).
    /// 
    /// Responsibilities:
    /// 1) Follow the assigned target with smoothing.
    /// 2) Rotate the rig with yaw and the pivot in pitch.
    /// 3) Process look input from camera-only demo interaction.
    /// 
    /// Notes:
    /// - Intended for combat demo presentation only.
    /// - This rig does not control character movement.
    /// - The followed target is expected to remain stationary in the demo scene.
    /// </summary>
    public sealed class DemoThirdPersonCameraRig : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private DemoCameraLookInput input;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform pivot;

        [Header("Follow")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField] private float followSharpness = 20f;

        [Header("Look")]
        [Tooltip("Mouse sensitivity (scaled delta).")]
        [SerializeField] private float mouseSensitivity = 2.5f;

        [Tooltip("Stick sensitivity (degrees per second).")]
        [SerializeField] private float stickSensitivity = 120f;
        
        [SerializeField] private float pitchMin = -35f;
        [SerializeField] private float pitchMax = 70f;

        #endregion

        #region State

        private float yaw;
        private float pitch;

        /// <summary>
        /// Gets the pitch pivot used by this camera rig.
        /// </summary>
        public Transform Pivot => pivot;

        #endregion

        #region Unity Lifestyle

        private void Awake()
        {
            if (pivot == null)
            {
                Transform found = transform.Find("Pivot");
                if(found != null)
                {
                    pivot = found;
                }
            }
        }

        private void LateUpdate()
        {
            if (followTarget == null || pivot == null || input == null)
            {
                return;
            }

            UpdateFollow();
            SyncRotationFromTransform();
            UpdateLookInput();
            ApplyRotation();
        }

        #endregion

        #region Follow

        /// <summary>
        /// Smoothly follows the configured target position with a fixed offset.
        /// </summary>
        private void UpdateFollow()
        {
            Vector3 targetPos = followTarget.position + followOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                1f - Mathf.Exp(-followSharpness * Time.deltaTime)
                );
        }

        #endregion

        #region Rotation Sync (Drift Fix)

        /// <summary>
        /// Keeps internal yaw and pitch values synchronized if an external system modified the rig.
        /// </summary>
        private void SyncRotationFromTransform()
        {
            float currentYaw = transform.eulerAngles.y;

            if (Mathf.Abs(Mathf.DeltaAngle(yaw, currentYaw)) > 0.1f)
            {
                yaw = currentYaw;
            }

            float currentPitch = pivot.localEulerAngles.x;
            if (currentPitch > 180f)
            {
                currentPitch -= 360f;
            }

            if (Mathf.Abs(Mathf.DeltaAngle(pitch, currentPitch)) > 0.1f)
            {
                pitch = currentPitch;
            }
        }

        #endregion

        #region Look Input

        /// <summary>
        /// Applies current look input to yaw and pitch.
        /// </summary>
        private void UpdateLookInput()
        {
            Vector2 look = input.Look;

            if (look.sqrMagnitude < 0.000001f)
            {
                return;
            }

            bool usingMouse = Mouse.current != null && Mouse.current.wasUpdatedThisFrame;

            if (usingMouse)
            {
                // Mouse delta -> scaled directly
                yaw += look.x * mouseSensitivity;
                pitch -= look.y * mouseSensitivity;
            }
            else
            {
                // Stick -> deg/sec
                yaw += look.x * stickSensitivity * Time.deltaTime;
                pitch -= look.y * stickSensitivity * Time.deltaTime;
            }

            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        }

        #endregion

        #region Apply

        /// <summary>
        /// Applies the final yaw and pitch rotation to the rig and pivot.
        /// </summary>
        private void ApplyRotation()
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        #endregion
    }
}