using UnityEngine;

namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Immutable data payload describing a damage event.
    /// 
    /// Pass from combat systems (e.g. ShooterCore) to damaga receivers.
    /// Can be extended in the future (critical hits, falloff, armour, etc.).
    /// </summary>
    public readonly struct DamageInfo
    {
        /// <summary>
        /// Final damage amount to apply.
        /// </summary>
        public readonly float Amount;

        /// <summary>
        /// World-space hit point.
        /// </summary>
        public readonly Vector3 Point;

        /// <summary>
        /// Surface normal at the hit point.
        /// </summary>
        public readonly Vector3 Normal;

        /// <summary>
        /// Direction from the attacker toward the target (or ray direction).
        /// </summary>
        public readonly Vector3 Direction;

        /// <summary>
        /// The source GameObject that caused the damage (player, enemy, etc.).
        /// Can be null for environment-based damage.
        /// </summary>
        public readonly GameObject Instigator;

        /// <summary>
        /// Creates a new damage info payload.
        /// </summary>
        public DamageInfo(
            float amount, 
            Vector3 point, 
            Vector3 normal, 
            Vector3 direction, 
            GameObject instigator)
        {
            Amount = amount;
            Point = point;
            Normal = normal;
            Direction = direction;
            Instigator = instigator;
        }
    }
}