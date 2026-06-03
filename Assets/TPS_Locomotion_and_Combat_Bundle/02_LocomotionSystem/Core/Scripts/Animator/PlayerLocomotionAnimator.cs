using UnityEngine;
using LocomotionSystem.Core;

namespace LocomotionSystem.Animation
{
    /// <summary>
    /// Bridges <see cref="PlayerLocomotion"/> data to the Animator.
    /// 
    /// Responsibilities:
    /// - Sends "Speed" (0..1) for free locomotion blend trees.
    /// - Sends "MoveX" / "MoveZ" (local planar velocity) for strafe / lock-on blend tree.
    /// - Sends "IsSprinting", "IsLockOn", "IsJumping" boolean flags.
    /// - Sends "VerticalVelocity" for jump / air blend trees.
    /// 
    /// This component is intended to sit on the same GameObject as the Animator,
    /// and reas its data from a PlayerLocomotion instance (usually on the parent).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerLocomotionAnimator : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Reference")]
        [Tooltip("PlayerLocomotion component providing movement data. If not assigned, it will be auto-fetched from parent.")]
        [SerializeField] private PlayerLocomotion _locomotion;

        [Header("Damping")]
        [Tooltip("Damp time for Speed parameter (0..1). Higher values result in smoother acceleration in animations.")]
        [SerializeField] private float _speedDampTime = 0.2f;

        [Tooltip("Damp time for MoveX / MoveZ parameters used in strafe / lock-on blend trees.")]
        [SerializeField] private float _directionDampTime = 0.1f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Animator components used to drive animation parameters.
        /// </summary>
        private Animator _anim;

        // Animator parameter hashes
        private int _speedHash;
        private int _moveXHash;
        private int _moveZHash;
        private int _isSprintingHash;
        private int _isLockOnHash;
        private int _isJumpingHash;
        private int _verticalVelHash;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            // Auto-find locomotion in parent if not assigned
            if (_locomotion == null)
            {
                _locomotion = GetComponentInParent<PlayerLocomotion>();
            }

            CacheAnimatorParameterHashes();
        }

        void Update()
        {
            if (_locomotion == null || _anim == null)
                return;

            UpdateLocomotionAnimation();
        }

        #endregion

        #region Setup

        /// <summary>
        /// Caches Animator parameter hashes for better performance.
        /// </summary>
        private void CacheAnimatorParameterHashes()
        {
            _speedHash = Animator.StringToHash("Speed");
            _moveXHash = Animator.StringToHash("MoveX");
            _moveZHash = Animator.StringToHash("MoveZ");
            _isSprintingHash = Animator.StringToHash("IsSprinting");
            _isLockOnHash = Animator.StringToHash("IsLockOn");
            _isJumpingHash = Animator.StringToHash("IsJumping");
            _verticalVelHash = Animator.StringToHash("VerticalVelocity");
        }

        #endregion

        #region Animation Update

        /// <summary>
        /// Updates all locomotion-related Animator parameters from PlayerLocomotion.
        /// </summary>
        private void UpdateLocomotionAnimation()
        {
            // 1) Speed (0..1) for free locomotion blend tree
            float normalizedSpeed = _locomotion.NormalizedSpeed;
            _anim.SetFloat(_speedHash, normalizedSpeed, _speedDampTime, Time.deltaTime);

            // 2) Local planar velocity (for lock-on strafing blend tree)
            Vector3 planar = _locomotion.PlanarVelocity;
            Vector3 localPlanar = transform.InverseTransformDirection(planar);

            float moveX = Mathf.Clamp(localPlanar.x, -1f, 1f);
            float moveZ = Mathf.Clamp(localPlanar.z, -1f, 1f);

            _anim.SetFloat(_moveXHash, moveX, _directionDampTime, Time.deltaTime);
            _anim.SetFloat(_moveZHash, moveZ, _directionDampTime, Time.deltaTime);

            // 3) Boolean flags (sprint / lock-on / jump)
            _anim.SetBool(_isSprintingHash, _locomotion.IsSprinting);
            _anim.SetBool(_isLockOnHash, _locomotion.IsLockOn);
            _anim.SetBool(_isJumpingHash, _locomotion.IsJumping);

            // 4) Vertical velocity for jump / air blend tree
            _anim.SetFloat(_verticalVelHash, _locomotion.VerticalVelocity);
        }

        #endregion
    }
}