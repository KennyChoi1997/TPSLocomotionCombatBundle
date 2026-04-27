using UnityEngine;

namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Provides an aim ray for combat systems such as TPS or Soulslike aiming.
    /// Combat logic should depend on this instead of Camera or Input directly.
    /// </summary>
    public interface IAimProvider
    {
        /// <summary>
        /// Returns the current aiming ray in world space.
        /// </summary>
        Ray GetAimRay();
    }
}