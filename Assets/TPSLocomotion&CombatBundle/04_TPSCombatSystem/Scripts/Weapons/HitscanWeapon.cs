using UnityEngine;
using TPSCombatSystem.Interfaces;

namespace TPSCombatSystem.Weapons
{
    /// <summary>
    /// Basic hitscan weapon data provider.
    /// 
    /// Responsibilities:
    /// 1) Expose weapon firing data through <see cref="IWeapon"/>.
    /// 2) Define muzzle transform and forward axis for shot direction.
    /// 3) Trigger lightweight feedback when fored.
    /// 
    /// Notes:
    /// - This class does not perform hit detection directly.
    /// - Actually shooting logic is handled by <see cref="TPSCombatSystem.Core.ShooterCore"/>.
    /// </summary>
    public sealed class HitscanWeapon : MonoBehaviour, IWeapon
    {
        #region Inspector Fields

        [Header("Weapon Setup")]
        [Tooltip("Transform used as the typical shot origin.")]
        [SerializeField] private Transform muzzle;

        [Tooltip("Weapon behaviour data source")]
        [SerializeField] private WeaponConfig config;

        [Header("Orientation")]
        [Tooltip("Local axis treated as the weapon's forward firing direction.")]
        [SerializeField] private WeaponForwardAxis forwardAxis = WeaponForwardAxis.MinusX;

        [Header("Haptics")]
        [SerializeField, Range(0f, 1f)] private float lowFreq = 0.4f;
        [SerializeField, Range(0f, 1f)] private float highFreq = 1.0f;
        [SerializeField] private float duration = 0.08f;

        #endregion

        #region IWeapon Properties

        /// <summary>
        /// Gets the muzzle transform used as the shot origin.
        /// </summary>
        public Transform Muzzle => muzzle;

        /// <summary>
        /// Gets which local axis should be treated as the weapon's forward.
        /// </summary>
        public WeaponForwardAxis ForwardAxis => forwardAxis;

        /// <summary>
        /// Gets base damage per hit.
        /// </summary>
        public float Damage => config != null ? config.damage : 0f;

        /// <summary>
        /// Gets maximum hitscan travel distance.
        /// </summary>
        public float MaxDistance => config != null ? config.maxDistance : 0f;

        /// <summary>
        /// Gets the layer mask used for hit queries.
        /// </summary>
        public LayerMask HitMask => config != null ? config.hitMask : ~0;

        /// <summary>
        /// Optional helper interval derived from fire rate.
        /// Not required by <see cref="IWeapon"/>, but may be useful for extentions.
        /// </summary>
        public float FireInterval => config != null ? config.FireInterval : 0f;

        public float MaxHorizontalAimDeviationDeg => 
            config != null ? config.maxHorizontalAimDeviationDeg : 45f;
        
        public float MaxVerticalAimDeviationDeg => 
            config != null ? config.maxVerticalAimDeviationDeg : 30f;

        #endregion

        #region Public API

        /// <summary>
        /// Called when the weapon is fired.
        /// Triggers lightweight feedback such as haptics.
        /// </summary>
        public void OnFired()
        {
            TPSCombatSystem.Feedback.Haptics.Pulse(lowFreq, highFreq, duration);
        }

        #endregion

        #region Unity Lifecycle

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (muzzle == null)
            {
                Transform found = transform.Find("Muzzle");
                if (found != null)
                {
                    muzzle = found;
                }
            }
        }
#endif

        #endregion
    }
}