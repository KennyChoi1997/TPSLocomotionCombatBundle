using UnityEngine;
using UnityEngine.UI;

namespace LocomotionSystem.UI
{
    /// <summary>
    /// Screen-space lock-on indicator.
    /// 
    /// Responsibilities:
    /// - Livess on a screen-space canvas (HUD)
    /// - Follows a world-space target by projecting its position to screen space.
    /// - Always rendered on top of 3D (never occluded by world geometry).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class LockOnIndicator : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [Tooltip("Camera used to convert world position to screen position (usually Main Camera).")]
        [SerializeField] private Camera _cam;

        [Tooltip("Current world-space target this indicator should follow.")]
        [SerializeField] private Transform _target;

        [Header("World Offset")]
        [Tooltip("World-space offset from the target position (e.g. above the head).")]
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 1.8f, 0f);

        [Header("Image")]
        [Tooltip("UI Image used as the lock-on crosshair.")]
        [SerializeField] private Image _image;

        #endregion

        #region Private Fields

        private RectTransform _rect;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            // Auto-assign camera if not set
            if (_cam == null)
                _cam = Camera.main;

            // Auto-assign Image on this object if not set
            if(_image == null)
                _image = GetComponent<Image>();

            // Ensure the indicator starts hidden
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (_cam == null)
                return;

            // No target -> hide and early out
            if (_target == null)
            {
                SetVisible(false);
                return;
            }

            // World position above target
            Vector3 worldPos = _target.position + _worldOffset;

            // Project world position to screen position
            Vector3 screenPos = _cam.WorldToScreenPoint(worldPos);

            // If behind camera, hide
            if (screenPos.z < 0f)
            {
                SetVisible(false);
                return;
            }

            // Place the UI element at the screen position
            _rect.position = screenPos;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Assigns a new world-space target for this indicator.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }

        /// <summary>
        /// Makes the indicator visible (if an Image is assigned).
        /// </summary>
        public void Show()
        {
            SetVisible(true);
        }

        /// <summary>
        /// Hides the indicator.
        /// </summary>
        public void Hide()
        {
            SetVisible(false);
        }

        /// <summary>
        /// Enables or disables the visual representation of this indicator.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_image != null)
                _image.enabled = visible;
            else
                gameObject.SetActive(visible);
        }

        #endregion
    }
}