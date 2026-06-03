namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Represents an object that can receive damage.
    /// Keep this minimal for broad compatibility in v1.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies incoming damage using a prepared damage info payload.
        /// </summary>
        void TakeDamage(in DamageInfo info);
    }
}