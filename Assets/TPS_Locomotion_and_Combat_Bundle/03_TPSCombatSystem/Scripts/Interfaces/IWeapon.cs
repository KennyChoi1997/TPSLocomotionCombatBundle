using UnityEngine;

namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Defines the minimal runtime weapon data required by combat firing systems.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        /// Transform used as the typical shot origin.
        /// </summary>
        Transform Muzzle { get; }

        /// <summary>
        /// Which local axis should be treated as weapon forward.
        /// </summary>
        WeaponForwardAxis ForwardAxis { get; }

        /// <summary>
        /// Based damage applied per hit.
        /// </summary>
        float Damage { get; }

        /// <summary>
        /// Maximum effective hitscan travel distance.
        /// </summary>
        float MaxDistance { get; }

        /// <summary>
        /// Layer mask used for hit detection.
        /// </summary>
        LayerMask HitMask { get; }

        /// <summary>
        /// Optional helper interval derived from weapon fire rate.
        /// </summary>
        float FireInterval { get; }

        /// <summary>
        /// Maximum allowed horizontal angle between muzzle forward and desired aim direction.
        /// </summary>
        float MaxHorizontalAimDeviationDeg {  get; }

        /// <summary>
        /// Maximum allowed vertical angle between muzzle forward and desired aim direction.
        /// </summary>
        float MaxVerticalAimDeviationDeg { get; }

        /// <summary>
        /// Called when the weapon is fired.
        /// Typically used for recoil, haptics, audio, or other local feedback.
        /// </summary>
        void OnFired();
    }

    /// <summary>
    /// Defines which local axis of a weapon transform should be treated as forward.
    /// Useful when imported models do not use the same default orientation.
    /// </summary>
    public enum WeaponForwardAxis
    {
        PlusZ,
        MinusZ,
        PlusX,
        MinusX,
        PlusY,
        MinusY
    }
}