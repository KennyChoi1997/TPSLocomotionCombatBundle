using UnityEngine;
using UnityEngine.InputSystem;

namespace LocomotionSystem.Input
{
    using LocomotionSystem.Input;

    /// <summary>
    /// Centralized input reader for the player.
    /// Wraps the Unity Input System generated input class and exposes
    /// high-level input values such as movement, look, sprint, jump and lock-on.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        #region Fields

        /// <summary> 
        /// Automatically generated input actions class. 
        /// </summary>
        private LocomotionInputAction _actions;

        /// <summary> 
        /// Raw movement input before smoothing & deadzone.
        /// </summary>
        private Vector2 _rawMoveInput;
        
        /// <summary> 
        /// Velocity used by SmoothDamp for movement input. 
        /// </summary>
        private Vector2 _moveInputVelocity;

        [Header("Movement Tuning")]
        [SerializeField] private float _moveDeadzone = 0.35f;
        [SerializeField] private float _moveSmoothing = 0.08f; // 0.05-0.15 recommended

        #endregion

        #region Properties

        /// <summary> 
        /// Current movement input from keyboard or gamepad (smoothed). 
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary> 
        /// Current camera look input from mouse or right stick. 
        /// </summary>
        public Vector2 LookInput { get; private set; }

        /// <summary>
        /// Horizontal axis used for target switching (-1..1).
        /// </summary>
        public float TargetSwitchInput {  get; private set; }

        /// <summary> 
        /// True while the sprint button is held (after press started).
        /// </summary>
        public bool SprintHeld { get; private set; }

        /// <summary> 
        /// True only for the frame when jump is triggered. 
        /// </summary>
        public bool JumpTriggered {  get; private set; }

        /// <summary> 
        /// True only for the frame when lock-on button is pressed. 
        /// </summary>
        public bool LockOnPressed {  get; private set; }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _actions = new LocomotionInputAction();

            // --- Movement (with deadzone) ---
            _actions.Locomotion.Move.performed += ctx =>
            {
                Vector2 input = ctx.ReadValue<Vector2>();

                // Apply deadzone
                if (input.sqrMagnitude < _moveDeadzone * _moveDeadzone)
                    input = Vector2.zero;

                _rawMoveInput = input;
            };

            _actions.Locomotion.Move.canceled += _ =>
            {
                _rawMoveInput = Vector2.zero;
                MoveInput = Vector2.zero;
            };

            // --- Look ---
            _actions.Locomotion.Look.performed += ctx => 
            LookInput = ctx.ReadValue<Vector2>();
            _actions.Locomotion.Look.canceled += _ => 
            LookInput = Vector2.zero;

            // --- Sprint (hold only) ---
            _actions.Locomotion.Sprint.started += _ =>
            SprintHeld = true;
            _actions.Locomotion.Sprint.canceled += _ => 
            SprintHeld = false;

            // --- Jump (1-frame trigger) ---
            _actions.Locomotion.Jump.performed += _ => 
            JumpTriggered = true;

            // --- Lock-On (1-frame trigger) ---
            _actions.Locomotion.LockOn.performed += _ =>
            LockOnPressed = true;

            // --- Target switch (axis) ---
            _actions.Locomotion.TargetSwitch.performed += ctx =>
            TargetSwitchInput = ctx.ReadValue<float>();
            _actions.Locomotion.TargetSwitch.canceled += _ =>
            TargetSwitchInput = 0f;
        }

        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

        private void LateUpdate()
        {
            // Reset 1-frame actions
            JumpTriggered = false;

            // Smooth movement input
            MoveInput = Vector2.SmoothDamp(
                MoveInput,
                _rawMoveInput,
                ref _moveInputVelocity,
                _moveSmoothing);

            // Hard clamp to zero for tiny values
            if (_rawMoveInput.sqrMagnitude < 0.0001f && MoveInput.sqrMagnitude < 0.0001f)
            {
                MoveInput = Vector2.zero;
                _moveInputVelocity = Vector2.zero;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Consumes lock-on press so it only lasts a single frame.
        /// Called by the camera controller after reading the value.
        /// </summary>
        public void ConsumeLockOn()
        {
            LockOnPressed = false;
        }

        #endregion
    }
}