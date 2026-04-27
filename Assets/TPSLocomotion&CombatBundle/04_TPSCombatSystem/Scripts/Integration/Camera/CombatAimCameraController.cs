using TPSCombatSystem.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSCombatSystem.CameraSystem
{
    /// <summary>
    /// Minimal over-shoulder aim camera without Cinemachine.
    /// 
    /// Responsibilities:
    /// 1) Follow this assigned target while aiming.
    /// 2) Read look input and maintain aim yaw/pitch.
    /// 3) Position the aim camera in an over-shoulder configuration.
    /// 4) Provide a centre-screen aim ray for hitscan systems.
    /// 5) Optionally rotate the player root toward aim direction.
    /// 
    /// Note:
    /// - This component does not enable or disable input actions.
    /// - Look input is only read while aiming is active.
    /// - Action map awitching should be handled externally.
    /// </summary>
    public sealed class CombatAimCameraController : MonoBehaviour, IAimOrientationProvider, IAimCameraRig
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform pivot;
        [SerializeField] private Camera aimCamera;

        [Header("Player Rotation")]
        [SerializeField] private Transform playerRoot;
        [SerializeField] private float playerRotateSpeed = 15f;

        [Header("Input")]
        [Tooltip("Bind to Combat/Look (Vector2). This script does NOT enable/disable the action.")]
        [SerializeField] private InputActionReference look;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private bool invertY = false;

        [Header("Over-Shoulder")]
        [Tooltip("Local shoulder offset applied to the aim camera.")]
        [SerializeField] private Vector3 pivotLocalOffset = new Vector3(0.35f, 0.15f, 0f);

        [Header("Distance")]
        [SerializeField] private float distance = 1.8f;

        [Header("Smoothing")]
        [SerializeField] private bool smoothFollow = true;
        [SerializeField] private float followSharpness = 20f;

        [Header("Free Look Sync Source")]
        [SerializeField] private Transform freeLookRigRoot;
        [SerializeField] private Transform freeLookPivot;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        #endregion

        #region Properties

        /// <summary> 
        /// Current world-space yaw used by the aim rig. 
        /// </summary>
        public float CurrentYaw => yaw;

        /// <summary> 
        /// Current pitch used by the aim rig. 
        /// </summary>
        public float CurrentPitch => pitch;

        /// <summary> 
        /// Pivot transform used as yaw/pitch reference. 
        /// </summary>
        public Transform Pivot => pivot;

        #endregion

        #region State

        private float yaw;
        private float pitch;
        private bool isAiming;

        #endregion

        #region Unity Lifecycle

        private void Reset()
        {
            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>(true);
            }

            if (pivot == null)
            {
                Transform found = transform.Find("AimPivot");
                if (found != null)
                {
                    pivot = found;
                }
            }
        }

        private void Awake()
        {
            if (pivot == null)
            {
                Transform found = transform.Find("AimPivot");
                if (found != null) pivot = found;
            }

            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>(true);
            }

            InitializeYawPitchFromCurrentPivot();
        }

        private void LateUpdate()
        {
            if (!isAiming)
            {
                return;
            }   

            if (followTarget == null || pivot == null || aimCamera == null)
            {
                return;
            }

            FollowTarget();
            UpdateAimRotation();
            ApplyRigTransform();
            ApplyCameraPlacement();
            RotatePlayerTowardAim();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Returns a centre-screen aim ray from the aim camera.
        /// </summary>
        public Ray GetCentreAimRay()
        {
            if (aimCamera == null)
            {
                return new Ray(transform.position, transform.forward);
            }

            return aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }

        /// <summary>
        /// Synchronize current yaw/pitch from the free-look rig or current pivot.
        /// Useful when entering aim mode to avoid visible snapping.
        /// </summary>
        public void SyncFromCurrent()
        {
            if (freeLookRigRoot != null)
            {
                yaw = freeLookRigRoot.eulerAngles.y;
            }
            else if (pivot != null)
            {
                yaw = pivot.rotation.eulerAngles.y;
            }

            if (freeLookPivot != null)
            {
                pitch = NormalizeAngle(freeLookPivot.localEulerAngles.x);
            }
            else if (pivot != null)
            {
                pitch = NormalizeAngle(pivot.localEulerAngles.x);
            }
            
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Enables or disables active aiming behaviour for this rig.
        /// </summary>
        public void SetAiming(bool aiming)
        {
            Log($"[AimCam] SetAiming({aiming}) aimCameraNull={(aimCamera == null)}");

            isAiming = aiming;

            if (aiming)
            {
                SyncFromCurrent();
            }
        }

        /// <summary>
        /// Returns current aim world rotation using the pivot when available.
        /// </summary>
        /// <returns></returns>
        public Quaternion GetAimWorldRotation()
        {
            if (pivot == null)
            {
                return transform.rotation;
            }

            return pivot.rotation;
        }

        #endregion

        #region Internal Flow

        /// <summary>
        /// Initializes yaw and pitch using the current pivot transform.
        /// </summary>
        private void InitializeYawPitchFromCurrentPivot()
        {
            if (pivot == null)
            {
                return;
            }

            Vector3 euler = pivot.rotation.eulerAngles;
            yaw = euler.y;

            pitch = euler.x;
            if (pitch > 180f)
            {
                pitch -= 360f;
            }

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Follows the assigned target in world space.
        /// </summary>
        private void FollowTarget()
        {
            Vector3 targetPos = followTarget.position;

            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    1f - Mathf.Exp(-followSharpness * Time.deltaTime)
                );
            }
            else
            {
                transform.position = targetPos;
            }
        }

        /// <summary>
        /// Updates yaw/pitch from look input while aiming.
        /// </summary>
        private void UpdateAimRotation()
        {
            Vector2 lookDelta = ReadLook();

            float dx = lookDelta.x;
            float dy = lookDelta.y * (invertY ? 1f : -1f);

            // Treat as stick-style degrees per second.
            // Mouse scaling should be handled in the action processor if needed.
            yaw += dx * rotationSpeed * Time.deltaTime;
            pitch += dy * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Applies rig rotation and pivot pitch transform.
        /// </summary>
        private void ApplyRigTransform()
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            pivot.localPosition = Vector3.zero;
            pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>
        /// Places the aim camera behind the pivot with shoulder offset.
        /// </summary>
        private void ApplyCameraPlacement()
        {
            aimCamera.transform.localPosition =
                new Vector3(pivotLocalOffset.x, pivotLocalOffset.y, -distance);

            aimCamera.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Smoothly rotates the player root toward the current aim forward direction.
        /// </summary>
        private void RotatePlayerTowardAim()
        {
            if (playerRoot == null)
            {
                return;
            }

            Vector3 forward = pivot.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);
            playerRoot.rotation = Quaternion.Slerp(
                playerRoot.rotation,
                targetRot,
                playerRotateSpeed * Time.deltaTime
                );
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Reads current look input from the assigned action references.
        /// </summary>
        private Vector2 ReadLook()
        {
            if (look == null || look.action == null)
            {
                return Vector2.zero;
            } 

            // This component does not force-enable the action.
            // If the action map is disabled, this will naturally return zero.
            return look.action.ReadValue<Vector2>();
        }

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message, this);
            }
        }

        private static float NormalizeAngle(float a)
        {
            while (a > 180f)
            {
                a -= 360f;
            }

            while (a < -180f)
            {
                a += 360f;
            }

            return a;
        }

        #endregion
    }
}