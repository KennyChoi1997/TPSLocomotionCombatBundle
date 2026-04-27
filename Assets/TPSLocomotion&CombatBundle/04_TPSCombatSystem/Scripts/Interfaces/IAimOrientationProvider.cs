using UnityEngine;

namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Provides current aim orientation in world space.
    /// Typically implemented by an aim camera rig or aim pivot controller.
    /// </summary>
    public interface IAimOrientationProvider
    {
        /// <summary>
        /// Returns the current aim world rotation.
        /// </summary>
        Quaternion GetAimWorldRotation();
    }
}