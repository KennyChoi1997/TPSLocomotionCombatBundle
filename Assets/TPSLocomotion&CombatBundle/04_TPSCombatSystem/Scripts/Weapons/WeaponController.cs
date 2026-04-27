using System;
using System.Collections;
using TPSCombatSystem.Core;
using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSCombatSystem.Weapons
{
    /// <summary>
    /// Runtime weapon firing controller.
    /// 
    /// Responsibilities:
    /// 1) Read fire / reload input through <see cref="IFireInput"/>.
    /// 2) Manage ammo, reserve ammo, cooldown, and reload state.
    /// 3) Trigger single-shot or shotgun-style fire through <see cref="ShooterCore"/>.
    /// 4) Publish ammo / fire / reload events for UI and feedback systems.
    /// 
    /// Notes:
    /// - This class does not perform hit detection directly.
    /// - Actual hitscan logic is delegated to <see cref="ShooterCore"/>.
    /// - Weapon behaviour data is read from <see cref="WeaponConfig"/>.
    /// - Runtime weapon feedback/data source is still read through <see cref="IWeapon"/>.
    /// </summary>
    public sealed class WeaponController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private ShooterCore shooter;
        [SerializeField] private MonoBehaviour inputBehaviour;  // IFireInput
        [SerializeField] private MonoBehaviour weaponBehaviour; // IWeapon (HItscanWeapon etc.)
        [SerializeField] private WeaponConfig config;

        [Header("Start Ammo")]
        [Tooltip("If true, reserve ammo starts full (config.maxReserveAmmo).")]
        [SerializeField] private bool startWithFullReserveAmmo = true;

        [Tooltip("Used only when Start With Full Reserve Ammo is false.")]
        [SerializeField, Min(0)] private int startReserveAmmo = 24;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        [Header("State (ReadOnly)")]
        [SerializeField] private int currentAmmo;
        [SerializeField] private int currentReserveAmmo;
        [SerializeField] private bool isReloading;
        [SerializeField] private float fireCooldown;

        #endregion

        #region Cached References

        private IFireInput fireInput;
        private IWeapon weapon;
        private Coroutine reloadRoutine;

        #endregion

        #region Properties / Events
   
        public int CurrentAmmo => currentAmmo;
        public int CurrentReserveAmmo => currentReserveAmmo;
        public int MagazineSize => config != null ? config.magazineSize : 0;
        public int MaxReserveAmmo => config != null ? config.maxReserveAmmo : 0;
        public bool IsReloading => isReloading;
        public ShooterCore Shooter => shooter;
        public WeaponConfig Config => config;

        /// <summary>
        /// Invoked whenever current ammo or reserve ammo changes.
        /// Typically used by ammo UI.
        /// </summary>
        public event Action AmmoChanged;

        /// <summary>
        /// Invoked after a shot is consumed and processed.
        /// </summary>
        public event Action Fired;

        /// <summary>
        /// Invoked when reload starts.
        /// </summary>
        public event Action ReloadStarted;

        /// <summary>
        /// Invoked when reload finishes successfully.
        /// </summary>
        public event Action ReloadFinished;

        /// <summary>
        /// Invoked when an in-progress reload is canceled.
        /// </summary>
        public event Action ReloadCanceled;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheRefs();
            InitializeAmmo();
            InjectWeaponToShooter();
            NotifyAmmoChanged();
        }

        private void OnEnable()
        {
            fireCooldown = 0f;
            InjectWeaponToShooter();
        }

        private void OnDisable()
        {
            // If weapon is disabled mid-reload, the coroutine may stops before
            // resetting internal states. Clean it up here to avoid getting stuck.
            CancelReload(resetCooldown: true);
        }

        private void Update()
        {
            TickCooldown(Time.deltaTime);

            if (fireInput == null || shooter == null || config == null)
            {
                return;
            }

            if (fireInput.ConsumeReloadPressed())
            {
                StartReload();
            }

            if (ShouldFireThisFrame())
            {
                TryFire();
            }
        }

        #endregion

        #region Setup / Initialization

        /// <summary>
        /// Caches and validates required interface references.
        /// </summary>
        private void CacheRefs()
        {
            fireInput = inputBehaviour as IFireInput;
            weapon = weaponBehaviour as IWeapon;

            if (shooter == null)
            {
                Debug.LogError("[WeaponController] ShooterCore missing.", this);
            }   

            if (fireInput == null)
            {
                Debug.LogError("[WeaponController] FireInput missing or invalid (IFireInput).", this);
            }  

            if (weapon == null)
            {
                Debug.LogError("[WeaponController] Weapon missing or invalid (IWeapon).", this);
            }

            if (config == null)
            {
                Debug.LogError("[WeaponController] WeaponConfig missing.", this);
            }
        }

        /// <summary>
        /// Initializes magazine and reserve ammo at startup.
        /// </summary>
        private void InitializeAmmo()
        {
            if (config == null)
            {
                currentAmmo = 0;
                currentReserveAmmo = 0;
                return;
            }

            currentAmmo = config.magazineSize;

            if (startWithFullReserveAmmo)
            {
                currentReserveAmmo = config.maxReserveAmmo;
            }
            else
            {
                currentReserveAmmo = Mathf.Clamp(startReserveAmmo, 0, config.maxReserveAmmo);
            }
        }

        /// <summary>
        /// Injects the currently cached weapon into <see cref="ShooterCore"/>.
        /// This helps keep fire logic consistent after startup or re-enable.
        /// </summary>
        private void InjectWeaponToShooter()
        {
            if (shooter == null || weapon == null)
            {
                return;
            }

            shooter.SetWeapon(weapon);
        }

        #endregion

        #region Update Helpers

        /// <summary>
        /// Advances the fire cooldown timer.
        /// </summary>
        public void TickCooldown(float deltaTime)
        {
            if (fireCooldown > 0f)
            {
                fireCooldown -= deltaTime;
            } 
        }

        /// <summary>
        /// Returns whether a fire request should be processed this frame.
        /// </summary>
        private bool ShouldFireThisFrame()
        {
            if (fireInput == null || config == null)
            {
                return false;
            }

            return config.fireMode switch
            {
                FireMode.SemiAuto => fireInput.ConsumeFirePressed(),
                FireMode.Shotgun => fireInput.ConsumeFirePressed(),
                _ => fireInput.FireHeld
            };
        }

        #endregion

        #region Fire

        /// <summary>
        /// Returns whether the weapon can fire now.
        /// </summary>
        public bool CanFire()
        {
            if (config == null) return false;
            if (shooter == null) return false;
            if (isReloading) return false;
            if (currentAmmo <= 0) return false;
            if (fireCooldown > 0f) return false;
            return true;
        }

        /// <summary>
        /// Attemps to fire using the currently selected fire mode.
        /// Automatically starts reload if the weapon is empty and reserve ammo exists.
        /// </summary>
        public void TryFire()
        {
            if (config == null)
            {
                return;
            }

            if (!CanFire())
            {
                if (!isReloading && currentAmmo <= 0 && currentReserveAmmo > 0)
                {
                    StartReload();
                }

                return;
            }

            Log($"[WeaponController] Fire weapon={name} mode={config.fireMode}");

            currentAmmo--;
            fireCooldown = config.FireInterval;

            weapon?.OnFired();

            bool hit = config.fireMode == FireMode.Shotgun
                ? FireShotgunPellets()
                : shooter.TryFire();

            shooter.PublishShotResult(hit);

            NotifyAmmoChanged();
            Fired?.Invoke();
        }

        /// <summary>
        /// Fires multiple pellets using a shared base direction with per-pellet spread.
        /// A final combined hit result is returned for feedback/UI use.
        /// </summary>
        private bool FireShotgunPellets()
        {
            if (config == null)
            {
                return false;
            }

            bool anyHit = false;
            Vector3 baseDirection = shooter.GetBaseFireDirection();

            for (int i = 0; i < config.pelletCount; i++)
            {
                Vector3 spreadDirection = ApplyConeSpread(baseDirection, config.spreadAngleDeg);
                bool hit = shooter.TryFireWithDirection(spreadDirection);
                anyHit |= hit;
            }

            return anyHit;
        }

        #endregion

        #region Reload

        /// <summary>
        /// Starts reload if current state allows it.
        /// </summary>
        public void StartReload()
        {
            if (!CanReload())
            {
                return;
            }

            if (reloadRoutine != null)
            {
                StopCoroutine(reloadRoutine);
            }

            reloadRoutine = StartCoroutine(ReloadRoutine());
            ReloadStarted?.Invoke();
        }

        /// <summary>
        /// Performs delayed magazine refill from reserve ammo.
        /// </summary>
        private IEnumerator ReloadRoutine()
        {
            if (config == null)
            {
                yield break;
            }

            isReloading = true;

            yield return new WaitForSeconds(config.reloadTime);

            int missingAmmo = config.magazineSize - currentAmmo;
            int ammoToLoad = Mathf.Min(missingAmmo, currentReserveAmmo);

            currentAmmo += ammoToLoad;
            currentReserveAmmo -= ammoToLoad;

            isReloading = false;
            reloadRoutine = null;

            NotifyAmmoChanged();
            ReloadFinished?.Invoke();
        }

        /// <summary>
        /// Cancels the active reload, if any.
        /// Optionally resets fire cooldown as part of state cleanup.
        /// </summary>
        public void CancelReload(bool resetCooldown = false)
        {
            if (reloadRoutine != null)
            {
                StopCoroutine(reloadRoutine);
                reloadRoutine = null;
            }

            bool wasReloading = isReloading;
            isReloading = false;

            if (wasReloading)
            {
                ReloadCanceled?.Invoke();
            }

            if (resetCooldown)
            {
                fireCooldown = 0f;
            }
        }

        /// <summary>
        /// Returns whether reload can start right now.
        /// </summary>
        public bool CanReload()
        {
            if (config == null) return false;
            if (isReloading) return false;
            if (currentAmmo >= config.magazineSize) return false;
            if (currentReserveAmmo <= 0) return false;
            return true;
        }

        #endregion

        #region Ammo

        /// <summary>
        /// Adds reserve ammo without exceeding <see cref="MaxReserveAmmo"/>.
        /// </summary>
        public void AddReserveAmmo(int amount)
        {
            if (config == null || amount <= 0)
            {
                return;
            }

            currentReserveAmmo = Mathf.Clamp(currentReserveAmmo + amount, 0, config.maxReserveAmmo);
            NotifyAmmoChanged();
        }

        /// <summary>
        /// Sets reserve ammo directly, clamped to valid bounds.
        /// </summary>
        public void SetReserveAmmo(int amount)
        {
            if (config == null)
            {
                return;
            }

            currentReserveAmmo = Mathf.Clamp(amount, 0, config.maxReserveAmmo);
            NotifyAmmoChanged();
        }

        /// <summary>
        /// Returns a simple current/reserve ammo text representation.
        /// </summary>
        public string GetAmmoText()
        {
            return $"{currentAmmo}/{currentReserveAmmo}";
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Raises ammo-changed notification.
        /// </summary>
        private void NotifyAmmoChanged()
        {
            AmmoChanged?.Invoke();
        }

        /// <summary>
        /// Applies simple random cone spread around a base direction.
        /// </summary>
        private static Vector3 ApplyConeSpread(Vector3 dir,float angleDeg)
        {
            if (angleDeg <= 0f)
            {
                return dir.normalized;
            }

            float yaw = UnityEngine.Random.Range(-angleDeg, angleDeg);
            float pitch = UnityEngine.Random.Range(-angleDeg, angleDeg);

            Quaternion spreadRot = Quaternion.Euler(pitch, yaw, 0f);
            return (spreadRot * dir).normalized;
        }

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message, this);
            }
        }

        #endregion
    }
}