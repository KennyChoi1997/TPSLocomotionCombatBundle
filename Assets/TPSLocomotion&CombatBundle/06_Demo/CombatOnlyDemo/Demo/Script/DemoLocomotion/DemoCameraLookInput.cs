using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Demo-only camera input reader.
    /// 
    /// Responsibilities:
    /// 1) Read look input from the demo camera action map.
    /// 2) Cache current look input value for polling-based camera logic.
    /// 
    /// Notes:
    /// - Intended only for the combat demo scene.
    /// - Character movement input is intentionally excluded.
    /// </summary>
    public sealed class DemoCameraLookInput : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Action References (Demo Camera Map)")]
        [SerializeField] private InputActionReference look;

        [Header("Behaviour")]
        [Tooltip("If true, the Look action remains enabled when this component is disabled.")]
        [SerializeField] private bool keepLookEnabledOnDisable = true;

        #endregion

        #region State / Properties

        /// <summary>
        /// Current look input value.
        /// </summary>
        public Vector2 Look { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            EnableAssignedActions();
            SubscribeCallbacks();
        }

        private void OnDisable()
        {
            UnsubscribeCallbacks();
            DisableAssignedActions();
            ResetCachedState();
        }

        #endregion

        #region Action Setup

        private void EnableAssignedActions()
        {
            Enable(look);
        }

        private void DisableAssignedActions()
        {

            if (!keepLookEnabledOnDisable)
            {
                Disable(look);
            }
        }

        private void SubscribeCallbacks()
        {
            if (look?.action != null)
            {
                look.action.performed += OnLook;
                look.action.canceled += OnLook;
            }
        }

        private void UnsubscribeCallbacks()
        {
            if (look?.action != null)
            {
                look.action.performed -= OnLook;
                look.action.canceled -= OnLook;
            }
        }

        private void ResetCachedState()
        {
            Look = Vector2.zero;
        }

        #endregion

        #region Input Callbacks

        private void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        #endregion

        #region Helpers

        private static void Enable(InputActionReference reference)
        {
            if (reference?.action == null)
            {
                return;
            }

            if (!reference.action.enabled)
            {
                reference.action.Enable();
            }
        }

        private static void Disable(InputActionReference reference)
        {
            if (reference?.action == null)
            {
                return;
            }

            if (reference.action.enabled)
            {
                reference.action.Disable();
            }
        }

        #endregion
    }
}