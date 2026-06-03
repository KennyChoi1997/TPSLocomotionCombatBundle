using TPSCombatSystem.Input;
using UnityEngine;

namespace TPSCombatSystem.Bridge
{
    /// <summary>
    /// Optional integration bridge that forwards combat aiming state
    /// to locomotion-driven rotation and movement reference systems.
    /// 
    /// Notes:
    /// - Used when combat and locomotion systems are combined.
    /// - Not required for combat-only demo scenes.
    /// </summary>
    public sealed class CombatAimToLocomotionBridge : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private CombatInputReader inputReader;
        [SerializeField] private MonoBehaviour targetBehaviour;  // IRotationMoveBridgeTarget

        [Header("Move Reference")]
        [SerializeField] private Transform aimYawReference;
        [SerializeField] private Transform freeLookYawReference;

        #endregion

        #region Cached References

        private IRotationMoveBridgeTarget target;

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

            target = targetBehaviour as IRotationMoveBridgeTarget;

            if (inputReader == null)
            {
                Debug.LogError("[CombatAimToLocomotionBridge] CombatInputReader missing.", this);
            }

            if (target == null)
            {
                Debug.LogError("[CombatAimToLocomotionBridge] Target missing/invalid (IRotationMoveBridgeTarget).", this);
            }  
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.AimChanged += OnAimChanged;

                // Initial sync
                OnAimChanged(inputReader.IsAiming);
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.AimChanged -= OnAimChanged;
            }

            // Restore safe defaults
            if (target != null)
            {
                target.SetRotationAllowed(true);

                if (freeLookYawReference != null)
                {
                    target.SetMoveReference(freeLookYawReference);
                }  
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Handles aim state changes and updates locomotion behaviour accordingly.
        /// </summary>
        private void OnAimChanged(bool isAiming)
        {
            Debug.Log(
                $"[Combat->Locomotion] Aim={isAiming}" +
                $"targetNull={(target == null)}" +
                $"aimRef={(aimYawReference ? aimYawReference.name : "NULL")}" + 
                $"freeRef={(freeLookYawReference ? freeLookYawReference.name : "NULL")}", 
                this
                );

            if (target == null)
            {
                return;
            }

            // When aiming, combat controls rotation
            target.SetRotationAllowed(!isAiming);

            // Switch moverment reference (camera-relative movement)
            Transform refYaw = isAiming ? aimYawReference : freeLookYawReference;

            if (refYaw != null)
            {
                target.SetMoveReference(refYaw);
            }
        }

        #endregion
    }
}