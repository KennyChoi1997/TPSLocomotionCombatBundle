using System;

namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Provides weapon swap requests for slot-based and cycle-based weapon switching.
    /// </summary>
    public interface IWeaponSwapInput
    {
        /// <summary>
        /// Raised when a specific weapon slot is requested. Used zero-based slot indexing.
        /// </summary>
        event Action<int> SlotRequested;

        /// <summary>
        /// Raised when the next weapon in sequence is requested.
        /// </summary>
        event Action NextRequested;

        /// <summary>
        /// Raised when the previous weapon in sequence is requested.
        /// </summary>
        event Action PrevRequested;
    }
}