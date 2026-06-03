using UnityEngine;

namespace TPSCombatSystem.Feedback
{
    /// <summary>
    /// Scene-independent coroutine runner used by the haptics system.
    /// 
    /// Responsibilities:
    /// 1) Provide a persistent runtime host for haptics coroutines.
    /// 2) Survive scene loads via DontDestroyOnLoad.
    /// 3) Avoid creating or returning an instance while quitting/exiting play mode.
    /// </summary>
    internal sealed class HapticsRunner : MonoBehaviour
    {
        private static HapticsRunner instance;
        private static bool quitting;

        /// <summary>
        /// Returns the singleton runner instance.
        /// Creates one automatically if needed and safe to do so.
        /// </summary>
        public static HapticsRunner Instance
        {
            get
            {
                if (quitting)
                {
                    return null;
                }

                if (instance != null)
                {
                    return instance;
                }

                GameObject go = new GameObject("HapticsRunner");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<HapticsRunner>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            quitting = true;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInit()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    quitting = true;
                }
                    
                if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
                {
                    quitting = false;
                    instance = null;
                }
            };
        }
#endif
    }
}