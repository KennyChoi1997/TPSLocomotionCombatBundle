using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSCombatSystem.CameraSystem
{
    /// <summary>
    /// Switches between locomotion and aim camera rigs.
    /// 
    /// Responsibilities:
    /// 1) Enable the correct rig based on current aim state.
    /// 2) Forward aim state to an optional internal aim camera controller.
    /// 
    /// Notes:
    /// - Implements <see cref="IAimCameraRig"/> so combat systems can control it indirectly.
    /// - Useful when locomotion and combat camera setups are separated into different rig objects.
    /// </summary>
    public sealed class CameraModeSwitcher : MonoBehaviour, IAimCameraRig
    {
        #region Inspector Fields

        [Header("Rigs")]
        [SerializeField] private GameObject locomotionRig;
        [SerializeField] private GameObject aimRig;

        [Header("Start State")]
        [SerializeField] private bool startInAim = false;

        [SerializeField] private MonoBehaviour aimControllerBehaviour; // Must implement IAimCameraRig

        #endregion

        #region Cached References / State

        private bool _isAiming;
        private IAimCameraRig _aimController;

        /// <summary>
        /// Gets whether the switcher is currently in aiming mode.
        /// </summary>
        public bool IsAiming => _isAiming;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _aimController = aimControllerBehaviour as IAimCameraRig;
            SetAiming(startInAim);
        }

        #endregion

        #region IAimCameraRig

        /// <summary>
        /// Enables the appropriate camera rig for the requested aim state.
        /// Also keeps the optional aim controller synchronized.
        /// </summary>
        public void SetAiming(bool isAiming)
        {
            _isAiming = isAiming;

            if (locomotionRig != null)
            {
                locomotionRig.SetActive(!isAiming);
            }

            if (aimRig != null)
            {
                aimRig.SetActive(isAiming);
            }

            // keeps internal aim camera logic synchronized with rig state.
            _aimController?.SetAiming(isAiming); 
        }

        #endregion
    }
}