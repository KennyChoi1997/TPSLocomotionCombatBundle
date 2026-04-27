using UnityEngine;
using TPSCombatSystem.Input;
using TPSCombatSystem.Weapons;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Demo-only upper-body combat animation driver.
    /// 
    /// Responsibilities:
    /// 1) Drive IsAiming and IsReloading animator parameters.
    /// 2) Trigger Fire and Reload animations from actual weapon events.
    /// 3) Rebinds automatically when <see cref="WeaponManager"/> changes the active weapon.
    /// 
    /// Notes:
    /// - Intended for combat demo presentation only.
    /// - Not required by the core combat runtime.
    /// - Does not handle locomotion animation.
    /// </summary>
    public sealed class DemoCombatAnimatorDriver : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private CombatInputReader combatInputReader;
        [SerializeField] private WeaponManager weaponManager;

        [Header("Layer")]
        [SerializeField] private string upperBodyLayerName = "UpperBody_Combat";
        [SerializeField] private bool forceUpperBodyLayerWeightToOne = true;

        #endregion

        #region Animator Hashes

        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        private static readonly int IsReloadingHash = Animator.StringToHash("IsReloading");
        private static readonly int FireTriggerHash = Animator.StringToHash("Fire");
        private static readonly int ReloadTriggerHash = Animator.StringToHash("Reload");

        #endregion

        #region State

        private WeaponController currentWeapon;

        #endregion

        #region Unity Lifecycle

        private void Reset()
        {
            AssignReferencesIfMissing();
        }

        private void Awake()
        {
            AssignReferencesIfMissing();
        }

        private void OnEnable()
        {
            if (combatInputReader != null)
            {
                combatInputReader.AimChanged += OnAimChanged;
            }

            if (weaponManager != null)
            {
                weaponManager.WeaponChanged += OnWeaponChanged;
            }
        }

        private void OnDisable()
        {
            if (combatInputReader != null)
            {
                combatInputReader.AimChanged -= OnAimChanged;
            }

            if (weaponManager != null)
            {
                weaponManager.WeaponChanged -= OnWeaponChanged;
            }

            UnbindWeapon();
        }

        private void Start()
        {
            if (animator == null)
            {
                return;
            }

            InitializeUpperBodyLayer();
            SyncInitialState();
        }

        #endregion

        #region Setup

        private void AssignReferencesIfMissing()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (combatInputReader == null)
            {
                combatInputReader = GetComponent<CombatInputReader>();
            } 

            if (weaponManager == null)
            {
                weaponManager = GetComponent<WeaponManager>();
            } 
        }

        private void InitializeUpperBodyLayer()
        {
            if (!forceUpperBodyLayerWeightToOne || animator == null)
            {
                return;
            }

            int layerIndex = animator.GetLayerIndex(upperBodyLayerName);
            if (layerIndex >= 0)
            {
                animator.SetLayerWeight(layerIndex, 1f);
            }
        }

        private void SyncInitialState()
        {
            animator.SetBool(
                IsAimingHash,
                combatInputReader != null && combatInputReader.IsAiming
                );

            if (weaponManager != null)
            {
                BindWeapon(weaponManager.CurrentWeaponController);
            }
            else
            {
                animator.SetBool(IsReloadingHash, false);
            }
        }

        #endregion

        #region Event Handling

        private void OnAimChanged(bool isAiming)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsAimingHash, isAiming);
        }

        private void OnWeaponChanged(GameObject weaponObject, int index)
        {
            WeaponController weaponController = weaponObject != null
                ? weaponObject.GetComponentInChildren<WeaponController>(true)
                : null;

            BindWeapon(weaponController);
        }

        private void OnWeaponFired()
        {
            if (animator == null)
            {
                return;
            }

            animator.ResetTrigger(ReloadTriggerHash);
            animator.SetTrigger(FireTriggerHash);
        }

        private void OnReloadStarted()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsReloadingHash, true);
            animator.ResetTrigger(FireTriggerHash);
            animator.SetTrigger(ReloadTriggerHash);
        }

        private void OnReloadFinished()
        {
            SetReloading(false);
        }

        private void OnReloadCanceled()
        {
            SetReloading(false);
        }

        #endregion

        #region Weapon Binding

        private void BindWeapon(WeaponController weapon)
        {
            if (currentWeapon == weapon)
            {
                SyncReloadState();
                return;
            }

            UnbindWeapon();
            currentWeapon = weapon;

            if (currentWeapon != null)
            {
                currentWeapon.Fired += OnWeaponFired;
                currentWeapon.ReloadStarted += OnReloadStarted;
                currentWeapon.ReloadFinished += OnReloadFinished;
                currentWeapon.ReloadCanceled += OnReloadCanceled;
            }

            SyncReloadState();
        }

        private void UnbindWeapon()
        {
            if (currentWeapon != null)
            {
                currentWeapon.Fired -= OnWeaponFired;
                currentWeapon.ReloadStarted -= OnReloadStarted;
                currentWeapon.ReloadFinished -= OnReloadFinished;
                currentWeapon.ReloadCanceled -= OnReloadCanceled;
            }

            currentWeapon = null;
        }

        private void SyncReloadState()
        {
            bool isReloading = currentWeapon != null && currentWeapon.IsReloading;
            SetReloading(isReloading);
        }

        #endregion

        #region Helpers

        private void SetReloading(bool isReloading)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(IsReloadingHash, isReloading);
        }

        #endregion
    }
}