using UnityEngine;
using TPSCombatSystem.Interfaces;

namespace TPSCombatSystem.Aiming
{
    /// <summary>
    /// Drives a weapon socket toward an aim pose using-space aim orientation.
    /// 
    /// Responsibilities:
    /// 1) Read world aim rotation from an <see cref="IAimOrientationProvider"/>.
    /// 2) Conveert that rotataion into local socket space.
    /// 3) Apply optional pitch/yaw adjustment to the weapon socket.
    /// 4) Smoothly blend into and out of the aim pose.
    /// 
    /// Notes:
    /// - Intended for lightweight produral socket aiming.
    /// - Character/root yaw may already be handled elsewhere, so yaw application is optional.
    /// - Current aiming-state lookup supports <see cref="TPSCombatSystem.Input.CombatInputReader"/>.
    /// </summary>
    public sealed class AimPoseDriver : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Target")]
        [SerializeField] private Transform weaponSocket;

        [Header("Source")]
        [SerializeField] private MonoBehaviour aimOrientationProviderBehaviour; // IAimOrientationProvider
        [SerializeField] private MonoBehaviour aimInputBehaviour;               // optional: CombatInputReader

        [Header("Apply")]
        [Tooltip("Apply pitch (up/down) to the socket")]
        [SerializeField] private bool applyPitch = true;

        [Tooltip("Apply yaw (left/right) to the socket as well Usually false if character root already handles yaw.")]
        [SerializeField] private bool applyYaw = false;

        [Tooltip("Minimum pitch angle in degrees).")]
        [SerializeField] private float pitchMin = -35f;

        [Tooltip("Maximum pitch angle in degrees).")]
        [SerializeField] private float pitchMax = 60f;

        [Tooltip("Blend speed when entering aim pose.")]
        [SerializeField] private float aimBlendSpeed = 14f;

        [Tooltip("Blend speed when returning to default pose.")]
        [SerializeField] private float returnBlendSpeed = 10f;

        [Header("Axis Fix (optional)")]
        [Tooltip("Optional local rotation offset to compensate for weapon/socket model orientation.")]
        [SerializeField] private Vector3 socketLocalRotationOffsetEuler = Vector3.zero;

        #endregion

        #region Cached References / State

        private IAimOrientationProvider aimOrientationProvider;
        private Quaternion defaultLocalRotation;
        private float currentWeight;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (weaponSocket == null)
            {
                weaponSocket = transform;
            }

            aimOrientationProvider = aimOrientationProviderBehaviour as IAimOrientationProvider;
            if (aimOrientationProvider == null)
            {
                Debug.LogError("[WeaponSocketAimDriver] AimOrientationProvider missing/invalid.", this);
            }  

            defaultLocalRotation = weaponSocket.localRotation;
        }

        private void LateUpdate()
        {
            if (weaponSocket == null || aimOrientationProvider == null)
            {
                return;
            }

            bool isAiming = ReadIsAiming();

            UpdateBlendWeight(isAiming);

            if (currentWeight <= 0.0001f)
            {
                weaponSocket.localRotation = defaultLocalRotation;
                return;
            }

            Quaternion targetLocalRotation = BuildTargetLocalRotation();
            Quaternion blendedRotation = Quaternion.Slerp(
                defaultLocalRotation,
                targetLocalRotation, 
                currentWeight
                );

            weaponSocket.localRotation = blendedRotation;
        }

        #endregion

        #region Internal Flow

        /// <summary>
        /// Updates blend weight based on aiming state.
        /// </summary>
        private void UpdateBlendWeight(bool isAiming)
        {
            float targetWeight = isAiming ? 1f : 0f;
            float speed = isAiming ? aimBlendSpeed : returnBlendSpeed;

            currentWeight = Mathf.MoveTowards(
                currentWeight,
                targetWeight, 
                speed * Time.deltaTime
                );
        }

        /// <summary>
        /// Builds the target local socket rotation from current aim orientation.
        /// </summary>
        private Quaternion BuildTargetLocalRotation()
        {
            Quaternion aimWorldRotation = aimOrientationProvider.GetAimWorldRotation();

            Transform parent = weaponSocket.parent;
            Quaternion aimLocalRotation = parent != null
                ? Quaternion.Inverse(parent.rotation) * aimWorldRotation
                : aimWorldRotation;

            Vector3 euler = aimLocalRotation.eulerAngles;
            float yaw = NormalizeAngle(euler.y);
            float pitch = NormalizeAngle(euler.x);

            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            Quaternion targetLocalRotation = Quaternion.identity;

            if (applyYaw && applyPitch)
            {
                targetLocalRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            else if (applyPitch)
            {
                targetLocalRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
            else if (applyYaw)
            {
                targetLocalRotation = Quaternion.Euler(0f, yaw, 0f);
            }

            Quaternion offsetRotation = Quaternion.Euler(socketLocalRotationOffsetEuler);
            return offsetRotation * targetLocalRotation;
        }

        /// <summary>
        /// Reads whether aiming is currently active.
        /// Currently supports CombatInputReader directly.
        /// </summary>
        private bool ReadIsAiming()
        {
            if (aimInputBehaviour == null)
            {
                return false;
            }

            var reader = aimInputBehaviour as TPSCombatSystem.Input.CombatInputReader;
            if (reader != null)
            {
                return reader.IsAiming;
            }

            return false;
        }

        #endregion

        #region Helpers

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