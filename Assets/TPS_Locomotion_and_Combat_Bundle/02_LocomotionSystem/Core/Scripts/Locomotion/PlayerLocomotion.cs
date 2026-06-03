using UnityEngine;
using LocomotionSystem.Input;

namespace LocomotionSystem.Core
{
    /// <summary>
    /// Handles player movement using CharacterController.
    /// 
    /// Features:
    /// - Camera-relative movement (classic third-person controls)
    /// - Acceleration / deceleration for responsive but smooth motion
    /// - Smooth character rotation
    /// - Jumping using custom gravity
    /// - Lock-on aware movement (strafe when locked-on)
    ///  
    /// This component is designed to be a reusable locomotion module
    /// for soulslike-style projects.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerLocomotion : MonoBehaviour
    {
        #region Public Properties

        /// <summary>
        /// Normalized movement speed (0..1) based on runSpeed and sprintMultiplier.
        /// Intended to drive locomotion blend trees in the animator.
        /// </summary>
        public float NormalizedSpeed { get; private set; }

        /// <summary>
        /// World-space planar velocity (Y = 0) for animation or additional logic.
        /// </summary>
        public Vector3 PlanarVelocity { get; private set; }

        /// <summary> 
        /// True while the player is sprinting.
        /// </summary>
        public bool IsSprinting { get; private set; }

        public bool AllowRotation {  get; private set; }

        /// <summary>
        /// Maximum planar speed used for animation normalization.
        /// </summary>
        public float MaxPlanarSpeed => runSpeed * sprintMultiplier;

        /// <summary> 
        /// True while lock-on is active. Typically set by the camera system.
        /// </summary>
        public bool IsLockOn => _isLockOn;

        /// <summary> 
        /// Current vertical velocity, including jump and gravity.
        /// </summary>
        public float VerticalVelocity => _verticalVelocity;

        /// <summary>
        /// True while the character is in a jump state (from take-off until landing detected).
        /// </summary>
        public bool IsJumping { get; private set; }

        #endregion

        #region Serialized Fields - References

        [Header("References")]

        /// <summary>
        /// Input reader feeding movement / sprint / jump data to this locomotion component.
        /// If not assigned, the component will try to find it on the same GameObject.
        /// </summary>
        [SerializeField] private PlayerInputReader _input;

        /// <summary>
        /// Camera transform used to convert input into camera-relative movement.
        /// If not assigned, Camera.main will be used at runtime.
        /// </summary>
        [SerializeField] private Transform _cameraTransform;

        #endregion

        #region Serialized Fields - Movement

        [Header("Movement Settings")]

        /// <summary>
        /// Walking speed (used for low analog input values).
        /// </summary>
        [SerializeField] private float walkSpeed = 2.5f;

        /// <summary>
        /// Running speed (used for high input magnitude).
        /// </summary>
        [SerializeField] private float runSpeed = 5f;

        /// <summary>
        /// Multiplier applied to running speed while sprint is held.
        /// </summary>
        [SerializeField] private float sprintMultiplier = 1.5f;

        [Header("Gravity")]

        /// <summary>
        /// Base gravity value (typically negative). Combined with gravityMultiplier.
        /// </summary>
        [SerializeField] private float gravity = -9.81f;

        /// <summary>
        /// Additional multiplier applied to gravity for a stronger or weaker fall.
        /// </summary>
        [SerializeField] private float gravityMultiplier = 3f;

        [Header("Acceleration")]

        /// <summary>
        /// Acceleration rate when there is movement input.
        /// </summary>
        [SerializeField] private float acceleration = 15f;

        /// <summary>
        /// Deceleration rate when there is no movement input.
        /// </summary>
        [SerializeField] private float deceleration = 20f;

        [Header("Rotation Settings")]

        /// <summary>
        /// Speed at which the character rotates towards the movement direction.
        /// </summary>
        [SerializeField] private float rotationSpeed = 10f;

        #endregion

        #region Serialized Fields - Jump

        [Header("Jump Settings")]

        /// <summary>
        /// Jump height in world units. Used to compute the initial jump velocity.
        /// </summary>
        [SerializeField] private float jumpHeight = 2.0f;

        /// <summary>
        /// Vertical velocity used to keep the character grounded and stable on slopes.
        /// </summary>
        [SerializeField] private float groundedGravity = -2f;

        #endregion

        #region Runtime State

        /// <summary>
        /// Full 3D velocity used when calling CharacterController.Move.
        /// </summary>
        private Vector3 _velocity;

        /// <summary>
        /// Horizontal movement direction and magnitude (without vertical component).
        /// </summary>
        private Vector3 _moveDirection;

        /// <summary>
        /// Internal horizontal velocity used for acceleration/deceleration.
        /// </summary>
        private Vector3 _horizontalVelocity;

        /// <summary>
        /// Current vertical velocity (gravity & jump).
        /// </summary>
        private float _verticalVelocity;

        /// <summary>
        /// CharacterController used for collision and movement.
        /// </summary>
        private CharacterController _controller;

        /// <summary>
        /// Whether lock-on mode is currently active (set via SetLockOnState).
        /// </summary>
        private bool _isLockOn;

        /// <summary>
        /// True of the character is considered grounded based on the controller.
        /// </summary>
        private bool _isGrounded;

        /// <summary>
        /// Grounded states from the previous frame, used for landing detection.
        /// </summary>
        private bool _wasGrounded;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            // Auto-wire input if not assigned.
            if (_input == null)
                _input = GetComponent<PlayerInputReader>();

            // Auto-assigh camera if not set.
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            _wasGrounded = _controller.isGrounded;

            AllowRotation = true;
        } 

        void Update()
        {
            if (_input == null)
                return;

            UpdateGroundedState();
            HandleMovement();
            HandleJump();
            HandleRotation();
            ApplyGravity();
            MoveCharacter();
            HandleLandingLogic();
        }

        #endregion

        #region Core Movement

        /// <summary>
        /// Converts camera-relative movement based on input
        /// and applies acceleration / deceleration on the horizontal plane.
        /// </summary>
        private void HandleMovement()
        {
            Vector2 moveInput = _input.MoveInput;

            // Lock-On movement uses a different pattern (strafe).
            if (_isLockOn)
            {
                HandleLockOnMovement(moveInput);
                return;
            }

            // 1) Determine movement basis from camera or character.
            Vector3 forward = _cameraTransform ? _cameraTransform.forward : transform.forward;
            Vector3 right = _cameraTransform ? _cameraTransform.right : transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // 2) Desired movement direction from input.
            Vector3 desiredDir = (moveInput.sqrMagnitude > 0.0001f)
                ? (forward * moveInput.y + right * moveInput.x).normalized
                : Vector3.zero;

            // 3) Compute target speed (walk/run) based on input magnitude.
            float inputMag = moveInput.magnitude;
            float speed = (inputMag > 0.6f) ? runSpeed : walkSpeed;;

            // Sprint
            if (_input.SprintHeld && inputMag > 0.1f)
            {
                speed *= sprintMultiplier;
                IsSprinting = true;
            }
            else
            {
                IsSprinting = false;
            }

            Vector3 desiredVelocity = desiredDir * speed;

            // 4) Accelerate / Decelerate towards the desired velocity.
            float accel = desiredVelocity.sqrMagnitude > 0.001f
                ? acceleration   // Input present
                : deceleration;  // No input

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, 
                desiredVelocity, 
                accel * Time.deltaTime);

            // 5) Final horizontal movement direction used by MoveCharacter.
            _moveDirection = _horizontalVelocity;

            // Full stop when input and velocity are effectively zero.
            if (moveInput.sqrMagnitude < 0.0001f && 
                _horizontalVelocity.sqrMagnitude < 0.0001f)
            {
                _horizontalVelocity = Vector3.zero;
                _moveDirection = Vector3.zero;
                PlanarVelocity = Vector3.zero;
                NormalizedSpeed = 0f;
            }

            // Expose values for animation.
            UpdatePlanarValuesForAnimation();
        }

        /// <summary>
        /// Handles movement when lock-on is enabled.
        /// In this mode the player strafes around the target instead of turning with the camera.
        /// </summary>
        private void HandleLockOnMovement(Vector2 moveInput)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 desireDir = forward * moveInput.y + right * moveInput.x;

            float inputMag = moveInput.magnitude;
            float speed = (inputMag > 0.6f) ? runSpeed : walkSpeed;

            if (_input.SprintHeld && inputMag > 0.1f)
            {
                speed *= sprintMultiplier;
                IsSprinting = true;
            }
            else
            {
                IsSprinting = false;
            }

            Vector3 desiredVelocity = desireDir * speed;

            float accel = (desiredVelocity.sqrMagnitude > 0.001f)
                ? acceleration
                : deceleration;

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                desiredVelocity,
                accel * Time.deltaTime);

            _moveDirection = _horizontalVelocity;

            UpdatePlanarValuesForAnimation();
        }

        /// <summary>
        /// Updates PlanarVelocity and NormalizedSpeed based on the current horizontal velocity.
        /// These values are typically used by the animation system.
        /// </summary>
        private void UpdatePlanarValuesForAnimation()
        {
            Vector3 planar = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);
            PlanarVelocity = planar;

            float planarSpeed = planar.magnitude;
            float maxSpeed = runSpeed * sprintMultiplier;
            float normalized = (maxSpeed > 0.01f) ? (planarSpeed / maxSpeed) : 0f;

            NormalizedSpeed = Mathf.Clamp01(normalized);
        }

        #endregion

        #region Ground & Gravity & Jump

        /// <summary>
        /// Updates grounding information based on the CharacterController.
        /// </summary>
        private void UpdateGroundedState()
        {
            _isGrounded = _controller.isGrounded;
        }

        /// <summary>
        /// Handles jump input while in grounded state.
        /// Computes the vertical launch velocity based on gravity and jumpHeight.
        /// </summary>
        private void HandleJump()
        {
            
            if (!_isGrounded)
                return;

            if (!_input.JumpTriggered)
                return;

            // v = sqrt(2 * g * h) using custom gravity * multiplier.
            _verticalVelocity = Mathf.Sqrt(-2f * gravity * gravityMultiplier * jumpHeight);
            IsJumping = true;
        }

        /// <summary>
        /// Applies custom gravity to the vertical velocity.
        /// Keeps a small downward force while grounded to maintain stable contact.
        /// </summary>
        private void ApplyGravity()
        {
            if (_isGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    // Slight downward force to keep controller grounded.
                    _verticalVelocity = groundedGravity;
                }
            }
            else
            {
                // Custom gravity (Project Settings gravity can be set to zero).
                _verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
            }
        }

        /// <summary>
        /// Detects landing transitions (airborne -> grounded)
        /// and resets jump-related state accordingly.
        /// </summary>
        private void HandleLandingLogic()
        {
            bool isGroundedNow = _controller.isGrounded;

            // Landing detection: was in the air & now grounded -> landed.
            // IMPORTANT: Only treat it as a real landing when we're falling (verticalVelocity <= 0).
            if (isGroundedNow && !_wasGrounded && _verticalVelocity <= 0f)
            {
                IsJumping = false;

                // Keep controller grounded.
                _verticalVelocity = groundedGravity;
            }

            _wasGrounded = isGroundedNow;
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Smoothly rotates the character to face the current movement direction.
        /// In lock-on mode, rotation is driven by the camera/lock-on logic instead.
        /// </summary>
        private void HandleRotation()
        {
            // External systems (combat aim, lock-on, etc.) can temporarilly own rotation.
            if (!AllowRotation)
                return;

            // Lock-On: rotation handled by camera (player always faces target)
            if (_isLockOn)
                return;

            Vector3 dir = new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z);

            if (dir.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRot, 
                rotationSpeed * Time.deltaTime);
        }

        #endregion

        #region Movement Application

        /// <summary>
        /// Combine horizontal and vertical velocities and moves the CharacterController.
        /// </summary>
        private void MoveCharacter()
        {
            _velocity = _moveDirection;
            _velocity.y = _verticalVelocity;

            _controller.Move(_velocity * Time.deltaTime);
        }

        #endregion

        #region External API

        /// <summary>
        /// Sets the lock-on state from external systems (typically the camera controller).
        /// While locked-on, movement will use strafe logic and rotation is not handled here.
        /// </summary>
        public void SetLockOnState(bool value)
        {
            _isLockOn = value;
        }

        /// <summary>
        /// Allows external systems to temporarilly take ownership of character rotation.
        /// 
        /// Example:
        /// - Combat aim system
        /// - Lock-on system
        /// - Cinematic controller
        /// </summary>
        /// <param name="value"></param>
        public void SetAllowRotation(bool value)
        {
            AllowRotation = value;
        }

        #endregion
    }
}