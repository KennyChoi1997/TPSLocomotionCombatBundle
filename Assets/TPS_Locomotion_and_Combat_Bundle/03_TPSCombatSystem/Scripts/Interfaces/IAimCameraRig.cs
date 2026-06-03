namespace TPSCombatSystem.Interfaces
{
    /// <summary>
    /// Minimal interface for enabling or disabling combat-side aim camera behaviour.
    /// Implement this on an aim camera rig or camera controller, not on locomotion cameras.
    /// </summary>
    public interface IAimCameraRig
    {
        /// <summary>
        /// Enables or disables aiming mode for the camera rig.
        /// </summary>
        void SetAiming(bool isAiming);
    }
}