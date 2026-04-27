using UnityEngine;

namespace TPSCombatSystem.Feedback
{
    /// <summary>
    /// Legacy haptics host placeholder.
    /// Prefer <see cref="HapticsRunner"/> for runtime coroutine hosting.
    /// </summary>
    public sealed class HapticsHost : MonoBehaviour
    {
        public static HapticsHost Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}