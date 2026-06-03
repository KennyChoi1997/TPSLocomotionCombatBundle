using TPSCombatSystem.Input;
using TPSCombatSystem.Weapons;
using TPSLocomotionCombatBundle.Integration.CameraSystem;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.Animation
{
    /// <summary>
    /// Bridges verified weapon/combat state to the integrated Animator Controller.
    /// 
    /// Responsibilities:
    /// - Sends aim state to Animator.
    /// - Sends fire animation only when the active weapon actually fires.
    /// - Sends reload animation only when the active weapon actually starts reloading.
    /// 
    /// Locomotion animation remains handled by PlayerLocomotionAnimator.
    /// </summary>
    public sealed class IntegratedAnimatorBridge : MonoBehaviour
    {
        #region Animator Hashes

        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        private static readonly int FireHash = Animator.StringToHash("Fire");
        private static readonly int ReloadHash = Animator.StringToHash("Reload");

        #endregion

        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private CameraModeController cameraModeController;
        [SerializeField] private WeaponManager weaponManager;

        [Header("Options")]
        [SerializeField] private bool resetFireTriggerBeforeSet = true;
        [SerializeField] private bool resetReloadTriggerBeforeSet = true;

        #endregion

        #region Runtime State

        private WeaponController currentWeapon;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (cameraModeController != null)
            {
                cameraModeController.AimModeChanged += OnAimModeChanged;
                animator?.SetBool(IsAimingHash, cameraModeController.IsAiming);
            }

            if (weaponManager != null)
            {
                weaponManager.WeaponChanged += OnWeaponChanged;
                BindWeapon(weaponManager.CurrentWeaponController);
            }
        }

        private void OnDisable()
        {
            if (cameraModeController != null)
            {
                cameraModeController.AimModeChanged -= OnAimModeChanged;
            }

            if (weaponManager != null)
            {
                weaponManager.WeaponChanged -= OnWeaponChanged;
            }

            UnbindWeapon();
        }

        #endregion

        #region Event Handler

        private void OnAimModeChanged(bool isAiming)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsAimingHash, isAiming);
        }

        private void OnWeaponChanged(GameObject _, int __)
        {
            BindWeapon(weaponManager != null ? weaponManager.CurrentWeaponController : null);
        }

        private void OnWeaponFired()
        {
            TriggerFireAnimation();
        }

        private void OnWeaponReloadStarted()
        {
            TriggerReloadAnimation();
        }

        #endregion

        #region Weapon Binding

        private void BindWeapon(WeaponController weapon)
        {
            if (currentWeapon == weapon)
            {
                return;
            }

            UnbindWeapon();

            currentWeapon = weapon;

            if (currentWeapon == null)
            {
                return;
            }

            currentWeapon.Fired += OnWeaponFired;
            currentWeapon.ReloadStarted += OnWeaponReloadStarted;
        }

        private void UnbindWeapon()
        {
            if (currentWeapon == null)
            {
                return;
            }

            currentWeapon.Fired -= OnWeaponFired;
            currentWeapon.ReloadStarted -= OnWeaponReloadStarted;
            currentWeapon = null;
        }

        #endregion

        #region Animation Trigger

        private void TriggerFireAnimation()
        {
            if (animator == null)
            {
                return;
            }

            if (resetFireTriggerBeforeSet)
            {
                animator.ResetTrigger(FireHash);
            }

            animator.SetTrigger(FireHash);
        }

        private void TriggerReloadAnimation()
        {
            if (animator == null)
            {
                return;
            }

            if (resetReloadTriggerBeforeSet)
            {
                animator.ResetTrigger(ReloadHash);
            }

            animator.SetTrigger(ReloadHash);
        }

        #endregion
    }
}