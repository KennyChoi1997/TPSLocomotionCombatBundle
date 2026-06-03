using TPSCombatSystem.CameraSystem;
using UnityEngine;

namespace TPSCombatSystem.Input
{
    /// <summary>
    /// Simple bridge that forwards aim state changes from <see cref="CombatInputReader"/>
    /// to a <see cref="CameraModeSwitcher"/>.
    /// 
    /// Notes:
    /// - This is a concrete bridge for CameraModeSwitcher specifically.
    /// - For a more generic setup, prefer a bridge that targets <c>IAimCameraRig</c>.
    /// </summary>
    public sealed class AimEventBridge : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private CombatInputReader input;
        [SerializeField] private CameraModeSwitcher switcher;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (input != null)
            {
                input.AimChanged += OnAimChanged;
            }

            if(switcher != null)
            {
                switcher.SetAiming(input != null && input.IsAiming);
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.AimChanged -= OnAimChanged;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Forwards aim state changes to the assigned camera mode switcher.
        /// </summary>
        private void OnAimChanged(bool isAiming)
        {
            if (switcher != null)
            {
                switcher.SetAiming(isAiming);
            }
        }

        #endregion
    }
}