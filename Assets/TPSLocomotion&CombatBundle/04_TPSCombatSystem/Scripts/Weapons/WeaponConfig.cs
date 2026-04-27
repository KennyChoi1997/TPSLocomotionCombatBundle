using UnityEngine;

namespace TPSCombatSystem.Weapons
{
    /// <summary>
    /// Data container for weapon behaviour.
    /// Used by WeaponController and weapon inplementations.
    /// </summary>
    [CreateAssetMenu(menuName = "TPS Combat System/Weapon Config", fileName = "WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        #region Damage

        [Header("Damage")]
        [Min(0f)] public float damage = 25f;

        #endregion

        #region Fire

        [Header("Fire")]
        public FireMode fireMode = FireMode.SemiAuto;

        [Tooltip("Shots per second")]
        [Min(0.01f)] public float fireRate = 10f;

        [Tooltip("Maximum hit distance")]
        [Min(0.1f)] public float maxDistance = 100f;

        [Tooltip("Layer mask used for aim and hit detection.")]
        public LayerMask hitMask = ~0;

        #endregion

        #region Aim Constraint

        [Header("Aim Constraint")]
        [Tooltip("Maximum allowed angle between muzzle forward and desired aim direction.")]
        [Min(0f)] public float maxHorizontalAimDeviationDeg = 40f;
        [Min(0f)] public float maxVerticalAimDeviationDeg = 25f;

        #endregion

        #region Shotgun(Optional)

        [Header("Shotgun")]
        [Tooltip("Pellet count used only when Fire Mode is Shotgun.")]
        [Min(1)] public int pelletCount = 10;

        [Tooltip("Spread cone angle in degrees used only when Fire Mode is Shotgun.")]
        [Min(0f)] public float spreadAngleDeg = 6f;

        #endregion

        #region Ammo

        [Header("Ammo")]
        [Min(1)] public int magazineSize = 12;

        [Min(0)] public int maxReserveAmmo = 60;

        #endregion

        #region Reload

        [Header("Reload")]
        [Min(0f)] public float reloadTime = 1.2f;

        #endregion

        #region Derived

        /// <summary>
        /// Time between shots in seconds.
        /// </summary>
        public float FireInterval => 1f / fireRate;

        #endregion
    }
}