using UnityEngine;

namespace TPSCombatSystem.Bridge
{
    /// <summary>
    /// Contract for systems that can be controlled by a rotation/movement bridge.
    /// 
    /// Allows external systems (e.g. combat camera/aim bridges) to:
    /// 1) Enable or disable rotation control.
    /// 2) Provide a reference transform for movement direction.
    /// </summary>
    public interface IRotationMoveBridgeTarget
    {
        /// <summary>
        /// Enables or disables rotation control.
        /// </summary>
        void SetRotationAllowed(bool allowed);

        /// <summary>
        /// Sets the reference transform used for movement direction (e.g. camera forward).
        /// </summary>
        void SetMoveReference(Transform reference);
    }
}