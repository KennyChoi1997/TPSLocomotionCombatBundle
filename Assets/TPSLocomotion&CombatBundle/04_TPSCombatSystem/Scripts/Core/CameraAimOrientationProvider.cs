using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSCombatSystem.Core
{
    /// <summary>
    /// Simple aim orientation provider backed by a camera transform.
    /// 
    /// Returns the assigned camera's world rotation as the current aim orientation.
    /// If no camera is assigned, it falls back to <see cref="Camera.main"/>,
    /// then to this trasnsform's rotation.
    /// </summary>
    public sealed class CameraAimOrientationProvider : MonoBehaviour, IAimOrientationProvider
    {
        #region Ispector Fields

        [Tooltip("Camera used as the source of aim world rotation.")]
        [SerializeField] private Camera cam;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        #endregion

        #region IAimOrientationProvider

        /// <summary>
        /// Returns current world-space aim rotation from the configured camera.
        /// </summary>
        public Quaternion GetAimWorldRotation()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam == null)
            {
                return transform.rotation;
            }

            return cam.transform.rotation;
        }

        #endregion
    }
}