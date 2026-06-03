using System;
using TPSCombatSystem.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TPSCombatSystem.Input
{
    /// <summary>
    /// Reads combat-related input actions using <see cref="InputActionReferences"/>.
    /// 
    /// Responsibilities:
    /// 1) Track continuous combat input states such as aiming and fire hold.
    /// 2) Convert performed button actions into one-frame consumable flags.
    /// 3) Raise events for aim changes, fire/reload requests, and weapon swap requests.
    /// 
    /// Note:
    /// - This class is intentionally seperated from locomotion input.
    /// - It enables/disables only the assigned combat actions referenced here.
    /// </summary>
    public sealed class CombatInputReader : MonoBehaviour, IFireInput, IWeaponSwapInput
    {
        #region Inspector Fields

        [Header("Input Action References (Combat Map)")]
        [Tooltip("Bind to: Combat/Aim (Button)")]
        [SerializeField] private InputActionReference aim;

        [Tooltip("Bind to: Combat/Fire (Button)")]
        [SerializeField] private InputActionReference fire;

        [Tooltip("Bind to: Combat/Reload (Button)")]
        [SerializeField] private InputActionReference reload;

        [Header("Weapon Swap (Optional, CombatMap)")]
        [Tooltip("Bind to: Combat/WeaponSlot1 (Button)")]
        [SerializeField] private InputActionReference weaponSlot1;

        [Tooltip("Bind to: Combat/WeaponSlot2 (Button)")]
        [SerializeField] private InputActionReference weaponSlot2;

        [Tooltip("Bind to: Combat/WeaponSlot3 (Button)")]
        [SerializeField] private InputActionReference weaponSlot3;

        [Tooltip("Bind to: Combat/NextWeapon (Button)")]
        [SerializeField] private InputActionReference nextWeapon;

        [Tooltip("Bind to: Combat/PrevWeapon (Button)")]
        [SerializeField] private InputActionReference prevWeapon;

        [Header("Events (UnityEvent)")]
        [Tooltip("Optional UnityEvent mirror for aim state changes.")]
        public UnityEvent<bool> OnAimChangedUnity;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        #endregion

        #region State / Events

        /// <summary>
        /// True while the aim input is currently held.
        /// </summary>
        public bool IsAiming { get; private set; }

        /// <summary>
        /// True while the fire input is currently held.
        /// Useful for full-auto style polling.
        /// </summary>
        public bool FireHeld {  get; private set; }

        // One-frame triggers consumed by polling systems.
        private bool firePressed;
        private bool reloadPressed;

        /// <summary>
        /// Raised when aim state changes.
        /// </summary>
        public event Action<bool> AimChanged;

        /// <summary>
        /// Raised when a fire performed input is recieved.
        /// </summary>
        public event Action FirePressed;

        /// <summary>
        /// Raised when a reload performed input is recieved.
        /// </summary>
        public event Action ReloadPressed;

        /// <summary>
        /// Raised when a special weapon slot is requested.
        /// </summary>
        public event Action<int> SlotRequested;

        /// <summary>
        /// Raised when next-weapon input is requested.
        /// </summary>
        public event Action NextRequested;

        /// <summary>
        /// Raised when pervious-weapon input is requested.
        /// </summary>
        public event Action PrevRequested;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EnableAssignedActions();
            SubscribeCallbacks();
        }

        private void OnDisable()
        {
            UnsubscribeCallbacks();
            DisableAssignedActions();
            ResetCachedStates();
        }

        #endregion

        #region Input Callback Subscription

        /// <summary>
        /// Enables all assigned combat input actions.
        /// Safe to call even if some actions are alreadt enabled.
        /// </summary>
        private void EnableAssignedActions()
        {
            EnableAction(aim);
            EnableAction(fire);
            EnableAction(reload);

            EnableAction(weaponSlot1);
            EnableAction(weaponSlot2);
            EnableAction(weaponSlot3);
            EnableAction(nextWeapon);
            EnableAction(prevWeapon);
        }

        /// <summary>
        /// Disables all assigned combat input actions.
        /// </summary>
        private void DisableAssignedActions()
        {
            DisableAction(aim);
            DisableAction(fire);
            DisableAction(reload);

            DisableAction(weaponSlot1);
            DisableAction(weaponSlot2);
            DisableAction(weaponSlot3);
            DisableAction(nextWeapon);
            DisableAction(prevWeapon);
        }

        /// <summary>
        /// Subscribes input callbacks to all assigned actions.
        /// </summary>
        private void SubscribeCallbacks()
        {
            if (aim?.action != null)
            {
                aim.action.started += OnAimStarted;
                aim.action.canceled += OnAimCanceled;
            }

            if (fire?.action != null)
            {
                fire.action.started += OnFireStarted;
                fire.action.canceled += OnFireCanceled;
                fire.action.performed += OnFirePerformed;
            }

            if (reload?.action != null)
                reload.action.performed += OnReloadPerformed;

            if (weaponSlot1?.action != null) weaponSlot1.action.performed += OnWeaponSlot1;
            if (weaponSlot2?.action != null) weaponSlot2.action.performed += OnWeaponSlot2;
            if (weaponSlot3?.action != null) weaponSlot3.action.performed += OnWeaponSlot3;

            if (nextWeapon?.action != null) nextWeapon.action.performed += OnNextWeapon;
            if (prevWeapon?.action != null) prevWeapon.action.performed += OnPrevWeapon;
        }

        /// <summary>
        /// Unsubscribes previously registered input callbacks.
        /// </summary>
        private void UnsubscribeCallbacks()
        {
            if (aim?.action != null)
            {
                aim.action.started -= OnAimStarted;
                aim.action.canceled -= OnAimCanceled;
            }

            if (fire?.action != null)
            {
                fire.action.started -= OnFireStarted;
                fire.action.canceled -= OnFireCanceled;
                fire.action.performed -= OnFirePerformed;
            }

            if (reload?.action != null)
                reload.action.performed -= OnReloadPerformed;

            if (weaponSlot1?.action != null) weaponSlot1.action.performed -= OnWeaponSlot1;
            if (weaponSlot2?.action != null) weaponSlot2.action.performed -= OnWeaponSlot2;
            if (weaponSlot3?.action != null) weaponSlot3.action.performed -= OnWeaponSlot3;

            if (nextWeapon?.action != null) nextWeapon.action.performed -= OnNextWeapon;
            if (prevWeapon?.action != null) prevWeapon.action.performed -= OnPrevWeapon;
        }

        /// <summary>
        /// Resets cached runtime input states when the component is disabled.
        /// </summary>
        private void ResetCachedStates()
        {
            IsAiming = false;
            FireHeld = false;
            firePressed = false;
            reloadPressed = false;
        }

        #endregion

        #region Input Callbacks

        private void OnAimStarted(InputAction.CallbackContext context)
        {
            Log("[CombatInputReader] Aim START");

            IsAiming = true;
            AimChanged?.Invoke(true);
            OnAimChangedUnity?.Invoke(true);
        }

        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            Log("[CombatInputReader] Aim END");

            IsAiming = false;
            AimChanged?.Invoke(false);
            OnAimChangedUnity?.Invoke(false);
        }

        private void OnFireStarted(InputAction.CallbackContext context)
        {
            FireHeld = true;
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            FireHeld = false;
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            Log("[CombatInputReader] Fire performed");

            firePressed = true;
            FirePressed?.Invoke();
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            reloadPressed = true;
            ReloadPressed?.Invoke();
        }

        private void OnWeaponSlot1(InputAction.CallbackContext context) => SlotRequested?.Invoke(0);
        private void OnWeaponSlot2(InputAction.CallbackContext context) => SlotRequested?.Invoke(1);
        private void OnWeaponSlot3(InputAction.CallbackContext context) => SlotRequested?.Invoke(2);

        private void OnNextWeapon(InputAction.CallbackContext context) => NextRequested?.Invoke();
        private void OnPrevWeapon(InputAction.CallbackContext context) => PrevRequested?.Invoke();

        #endregion

        #region Public API

        /// <summary>
        /// Returns true once per fire press, then clears the stored flag.
        /// Intended for polling-based semi-auto / shoygun logic.
        /// </summary>
        public bool ConsumeFirePressed()
        {
            bool pressed = firePressed;
            firePressed = false;
            return pressed;
        }

        /// <summary>
        /// Returns true once per reload press, then clears the stored flag.
        /// </summary>
        public bool ConsumeReloadPressed()
        {
            bool pressed = reloadPressed;
            reloadPressed = false;
            return pressed;
        }

        /// <summary>
        /// Clears cached fire input state.
        /// Useful when resetting weapon/input flow after swaps or interruptions.
        /// </summary>
        public void ResetFireState()
        {
            FireHeld = false;
            firePressed = false;
        }

        #endregion

        #region Helpers

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message, this);
            }
        }

        private static void EnableAction(InputActionReference reference)
        {
            if (reference == null || reference.action == null)
            {
                return;
            }

            if (!reference.action.enabled)
            {
                reference.action.Enable();
            }
        }

        private static void DisableAction(InputActionReference reference)
        {
            if (reference == null || reference.action == null)
            {
                return;
            }

            if (reference.action.enabled)
            {
                reference.action.Disable();
            }
        }

        #endregion
    }
}