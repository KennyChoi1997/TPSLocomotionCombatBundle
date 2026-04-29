using TPSCombatSystem.Input;
using TPSLocomotionCombatBundle.Integration.CameraSystem;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.Animation
{
    /// <summary>
    /// Bridges bundle-level combat state to the integrated Animator Controller.
    /// 
    /// Responsibilities:
    /// - Sends aim state to Animator.
    /// - Sends fire / reload triggers to Animator.
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
        [SerializeField] private CombatInputReader combatInputReader;

        [Header("Options")]
        [SerializeField] private bool resetFireTriggerBeforeSet = true;
        [SerializeField] private bool resetReloadTriggerBeforeSet = true;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (cameraModeController != null)
            {
                cameraModeController.AimModeChanged += OnAimModeChanged;
                animator?.SetBool(IsAimingHash, cameraModeController.IsAiming);
            }

            if (combatInputReader != null)
            {
                combatInputReader.FirePressed += OnFirePressed;
                combatInputReader.ReloadPressed += OnReloadPressed;
            }
        }

        private void OnDisable()
        {
            if (cameraModeController != null)
            {
                cameraModeController.AimModeChanged -= OnAimModeChanged;
            }

            if (combatInputReader != null)
            {
                combatInputReader.FirePressed -= OnFirePressed;
                combatInputReader.ReloadPressed -= OnReloadPressed;
            }
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

        private void OnFirePressed()
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

        private void OnReloadPressed()
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