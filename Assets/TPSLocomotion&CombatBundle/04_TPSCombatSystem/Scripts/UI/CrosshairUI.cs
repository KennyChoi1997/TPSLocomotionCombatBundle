using TPSCombatSystem.Core;
using TPSCombatSystem.Input;
using UnityEngine;
using UnityEngine.UI;

namespace TPSCombatSystem.UI
{
    /// <summary>
    /// Runtime crosshair UI controller.
    /// 
    /// Responsibilities:
    /// 1) Show or hide the crosshair based on current aiming state.
    /// 2) Listen to <see cref="ShooterCore.ShotResult"/> for hit feedback.
    /// 3) Apply visual hit response such as colour and scale changes.
    /// 4) Support runtime shooter reassignment.
    /// </summary>
    public sealed class CrosshairUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private CombatInputReader input;
        [SerializeField] private GameObject crosshairRoot;

        [Header("Shooter (runtime)")]
        [SerializeField] private ShooterCore shooter;

        [Header("Visibility")]
        [Tooltip("If true, crosshair stays visible even when not aiming.")]
        [SerializeField] private bool showWhenNotAiming = true;

        [Header("Visual")]
        [SerializeField] private Graphic graphic;
        [SerializeField] private Color normalColour = Color.white;
        [SerializeField] private Color hitColour = Color.red;

        [SerializeField] private RectTransform rect;
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float hitScale = 1.15f;

        [Header("Hit feedback")]
        [SerializeField] private float hitHoldTime = 0.08f;

        [Header("Debug")]
        [SerializeField] private bool enableDebudLog = false;

        #endregion

        #region State

        private float hitTimer;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (crosshairRoot == null)
            {
                crosshairRoot = gameObject;
            }

            if (rect == null)
            {
                rect = crosshairRoot.GetComponent<RectTransform>();
            }

            if (graphic == null)
            {
                graphic = crosshairRoot.GetComponentInChildren<Graphic>(true);
            }

            TryAutoAssignInput();

            ApplyVisibility();
            SetHit(false);
        }

        private void OnEnable()
        {
            TryAutoAssignInput();
            TryAutoAssignShooter();
            BindShooterEvents(true);
        }

        private void OnDisable()
        {
            BindShooterEvents(false);
        }

        private void Update()
        {
            if (crosshairRoot == null)
            {
                return;
            }

            ApplyVisibility();
            UpdateHitFeedbackTimer();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Assigns a new shooter at runtime and rebinds hit feedback events.
        /// </summary>
        public void SetShooter(ShooterCore newShooter)
        {
            if (shooter == newShooter)
            {
                return;
            }

            BindShooterEvents(false);
            shooter = newShooter;
            BindShooterEvents(true);

            hitTimer = 0f;
            SetHit(false);

            Log($"[CrosshairUI] SetShooter -> {(shooter ? shooter.name : "NULL")}");
        }

        /// <summary>
        /// Applies hit or normal visual state immediately.
        /// </summary>
        public void SetHit(bool hit)
        {
            if (graphic != null)
            {
                graphic.color = hit ? hitColour : normalColour;
            }

            if (rect != null)
            {
                rect.localScale = Vector3.one * (hit ? hitScale : normalScale);
            }
        }

        #endregion

        #region Binding / Auto Assignment

        /// <summary>
        /// Attempts to auto-assign the combat input reader if none is set
        /// </summary>
        private void TryAutoAssignInput()
        {
            if (input == null)
            {
                input = FindFirstObjectByType<CombatInputReader>(FindObjectsInactive.Include);
            }
        }

        /// <summary>
        /// Attempts to auto-assign the shooter if none is set.
        /// </summary>
        private void TryAutoAssignShooter()
        {
            if (shooter == null)
            {
                shooter = FindFirstObjectByType<ShooterCore>(FindObjectsInactive.Include);
            }
        }

        /// <summary>
        /// Binds or unbinds to the current shooter shot-result event.
        /// </summary>
        private void BindShooterEvents(bool bind)
        {
            if (shooter == null)
            {
                Log($"[CrosshairUI] BindShooterEvents({bind}) skipped - shooter NULL");
                return;
            }

            if (bind)
            {
                shooter.ShotResult += OnShotResult;
            }
            else
            {
                shooter.ShotResult -= OnShotResult;
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Responds to shot result feedback from the current shooter.
        /// </summary>
        private void OnShotResult(bool hit)
        {
            Log(
                $"[CrosshairUI] OnShotResult hit={hit}" +
                $"boundShooter={(shooter ? shooter.name : "NULL")}" +
                $"id={(shooter ? shooter.GetInstanceID() : -1)}"
                );

            if (hit)
            {
                SetHit(true);
                hitTimer = hitHoldTime;
            }
            else
            {
                SetHit(false);
                hitTimer = 0f;
            }
        }

        #endregion

        #region Internal Flow

        /// <summary>
        /// Applies current visibility rules based on aiming state.
        /// </summary>
        private void ApplyVisibility()
        {
            bool aimingNow = (input != null) && input.IsAiming;
            bool shouldShow = showWhenNotAiming || aimingNow;

            if (crosshairRoot.activeSelf != shouldShow)
            {
                crosshairRoot.SetActive(shouldShow);
            }  
        }

        /// <summary>
        /// Updates temporary hit feedback timing.
        /// </summary>
        private void UpdateHitFeedbackTimer()
        {
            if (hitTimer <= 0f)
            {
                return;
            }

            hitTimer -= Time.deltaTime;

            if (hitTimer <= 0f)
            {
                SetHit(false);
            }
        }

        #endregion

        #region Helpers

        private void Log(string message)
        {
            if (enableDebudLog)
            {
                Debug.Log(message, this);
            }
        }

        #endregion
    }
}