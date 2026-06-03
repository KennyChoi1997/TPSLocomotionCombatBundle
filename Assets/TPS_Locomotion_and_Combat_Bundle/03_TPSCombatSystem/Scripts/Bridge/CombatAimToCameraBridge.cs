using TPSCombatSystem.Input;
using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSCombatSystem.Bridge
{
    /// <summary>
    /// Bridges combat aim state to a camera rig that implement <see cref="IAimCameraRig"/>.
    /// 
    /// Responsibilities:
    /// 1) Listen to aim state changes from <see cref="CombatInputReader"/>.
    /// 2) Forward that state to the assigned combat camera rig.
    /// 
    /// Notes:
    /// - Keeps Combat independent from camera implementation details.
    /// - Performs an initial sync on enable to avoid incorrect startup camera state.
    /// </summary>
    public sealed class CombatAimToCameraBridge : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private CombatInputReader inputReader;
        [SerializeField] private MonoBehaviour cameraRigBehaviour; // Must implement IAimCameraRig

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        #endregion

        #region Cached Reference

        private IAimCameraRig cameraRig;

        #endregion

        #region Unity Lifecycle

        private void Reset()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<CombatInputReader>();
            }
        }

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<CombatInputReader>();
            } 

            cameraRig = cameraRigBehaviour as IAimCameraRig;

            if (cameraRig == null)
            {
                Debug.LogError(
                   "[CombatAimToCameraBridge] Camera rig is missing or does not implement IAimCameraRig.\n" +
                   $"Assigned: {(cameraRigBehaviour ? cameraRigBehaviour.GetType().Name : "NULL")}",
                   this
               );
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.AimChanged += OnAimChanged;
            }  

            // Sync once on enable (prevents wrong initial camera state)
            cameraRig?.SetAiming(inputReader != null && inputReader.IsAiming);
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.AimChanged -= OnAimChanged;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Forwards combat aim state changes to the assigned camera rig.
        /// </summary>
        private void OnAimChanged(bool isAiming)
        {
            Log($"[AimToCameraBridge] Aim={isAiming} cameraRigNull={(cameraRig == null)}");
            cameraRig?.SetAiming(isAiming);
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

        #endregion
    }
}