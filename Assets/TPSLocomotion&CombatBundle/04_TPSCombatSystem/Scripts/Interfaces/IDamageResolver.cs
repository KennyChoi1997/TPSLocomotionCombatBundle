using UnityEngine;

namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Resolves a hit result into actual damage application.
    /// This keeps combat hit detection decoupled from target health implementations.
    /// </summary>
    public interface IDamageResolver
    {
        /// <summary>
        /// Attempts to apply damage based on raycast hit data.
        /// Returns true if damage was successfully applied to a valid target.
        /// </summary>
        bool TryResolve(in RaycastHit hit, in DamageInfo baseInfo);
    }
}