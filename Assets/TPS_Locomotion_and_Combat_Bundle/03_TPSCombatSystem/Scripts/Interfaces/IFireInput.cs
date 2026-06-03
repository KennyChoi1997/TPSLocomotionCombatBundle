namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Provides combat fire/reload input in a polling-friendly form.
    /// Intended for implementations backed by the Unity Input System.
    /// </summary>
    public interface IFireInput
    {
        /// <summary>
        /// Returns true once for a fire press, then clears the stored trigger.
        /// </summary>
        bool ConsumeFirePressed();

        /// <summary>
        /// Returns true once for a reload press, then clears the stored trigger.
        /// </summary>
        bool ConsumeReloadPressed();

        /// <summary>
        /// True while the fire input is currently held.
        /// Useful for full-auto polling.
        /// </summary>
        bool FireHeld {  get; }
    }
}