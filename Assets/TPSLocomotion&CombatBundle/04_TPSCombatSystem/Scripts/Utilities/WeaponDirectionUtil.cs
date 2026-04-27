using UnityEngine;
using TPSCombatSystem.Interfaces;

namespace TPSCombatSystem.Utils
{
    /// <summary>
    /// Utility for resolving a direction vector from a transform
    /// based on a configured <see cref="WeaponForwardAxis"/>.
    /// 
    /// Used by ShooterCore to determine actual muzzle forward direction
    /// independent of model orientation (e.g. +Z, -X, etc.).
    /// </summary>
    public static class WeaponDirectionUtil
    {
        /// <summary>
        /// Returns a world-space direction vector from the given transform
        /// according to the specified axis mapping.
        /// </summary>
        /// <param name="t">Source transform (usually weapon muzzle).</param>
        /// <param name="axis">Axis configuration of the weapon forward direction.</param>
        /// <returns>Normalized direction vector.</returns>
        public static Vector3 GetAxisDir(Transform t, WeaponForwardAxis axis)
        {
            if (t == null)
            {
                Debug.LogWarning("[WeaponDirectionUtil] Transform is null. Returning forward.", null);
                return Vector3.forward;
            }

            // Transform directions (forward/right/up) are already normalized.
            return axis switch
            {
                WeaponForwardAxis.PlusZ => t.forward,
                WeaponForwardAxis.MinusZ => -t.forward,
                WeaponForwardAxis.PlusX => t.right,
                WeaponForwardAxis.MinusX => -t.right,
                WeaponForwardAxis.PlusY => t.up,
                WeaponForwardAxis.MinusY => -t.up,
                _ => t.forward,
            };
        }
    }
}