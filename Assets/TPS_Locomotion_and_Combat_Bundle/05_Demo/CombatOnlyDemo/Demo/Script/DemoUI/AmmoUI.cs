using TMPro;
using TPSCombatSystem.Weapons;
using UnityEngine;

namespace TPSCombatSystem.UI
{
    /// <summary>
    /// Simple ammo UI displaying the currently active weapon ammo state.
    /// 
    /// Responsibilities:
    /// 1) Listen to <see cref="WeaponManager"/> weapon changes.
    /// 2) Bind to the active <see cref="WeaponController"/>.
    /// 3) Refresh displayed ammo when the active weapon changes or ammo changes.
    /// 
    /// Notes:
    /// - This UI is optional and not required by the core combat runtime.
    /// - Intended primarily for demo/sample scene presentation.
    /// </summary>
    public sealed class AmmoUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private TMP_Text ammoText;

        #endregion

        #region State

        private WeaponController currentWeapon;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (weaponManager == null)
            {
                Debug.LogError("[AmmoUI] WeaponManager is NULL.", this);
            }

            if (ammoText == null)
            {
                Debug.LogError("[AmmoUI] ammoText is NULL.", this);
            }
        }

        private void OnEnable()
        {
            if (weaponManager != null)
            {
                weaponManager.WeaponChanged += OnWeaponChanged;
            }
        }

        private void OnDisable()
        {
            if (weaponManager != null)
            {
                weaponManager.WeaponChanged -= OnWeaponChanged;
            }
        }

        private void Start()
        {
            if (weaponManager != null)
            {
                BindWeapon(weaponManager.CurrentWeaponController);
            }
            else
            {
                RefreshText(null);
            }
        }

        #endregion
        
        #region Event Handling

        /// <summary>
        /// Rebinds ammo UI to the newly active weapon.
        /// </summary>
        private void OnWeaponChanged(GameObject weaponObject, int index)
        {
            WeaponController wc = weaponObject != null
                ? weaponObject.GetComponentInChildren<WeaponController>(true) 
                : null;

            BindWeapon(wc);
        }

        /// <summary>
        /// Refreshes UI when the currently bound weapon ammo changes.
        /// </summary>
        private void OnAmmoChanged()
        {
            RefreshText(currentWeapon);
        }

        #endregion

        #region Binding

        /// <summary>
        /// Binds this UI to the specified weapon controller.
        /// </summary>
        private void BindWeapon(WeaponController weapon)
        {
            if (currentWeapon == weapon)
            {
                RefreshText(currentWeapon);
                return;
            }

            UnbindWeapon();

            currentWeapon = weapon;

            if (currentWeapon != null)
            {
                currentWeapon.AmmoChanged += OnAmmoChanged;
            }

            RefreshText(currentWeapon);
        }

        /// <summary>
        /// Unbinds from the currently tracked weapon controller.
        /// </summary>
        private void UnbindWeapon()
        {
            if (currentWeapon != null)
            {
                currentWeapon.AmmoChanged -= OnAmmoChanged;
            }

            currentWeapon = null;
        }

        #endregion

        #region UI Update

        /// <summary>
        /// Updates the displayed ammo text.
        /// </summary>
        private void RefreshText(WeaponController weapon)
        {
            if (ammoText == null)
            {
                return;
            }

            if (weapon == null)
            {
                ammoText.text = "--/--";
                return;
            }

            ammoText.text = $"{weapon.CurrentAmmo}/{weapon.CurrentReserveAmmo}";
        }

        #endregion
    }
}