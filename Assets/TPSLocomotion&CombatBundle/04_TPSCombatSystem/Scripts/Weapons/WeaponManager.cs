using System;
using System.Collections.Generic;
using TPSCombatSystem.Core;
using TPSCombatSystem.Interfaces;
using TPSCombatSystem.UI;
using UnityEngine;

namespace TPSCombatSystem.Weapons
{
    /// <summary>
    /// Manages weapon switching for weapon objects under a weapon socket.
    /// 
    /// Responsibilities:
    /// 1) Cache and track weapon GameObjects under the assigned socket.
    /// 2) Equip next / previous / indexed weapons.
    /// 3) Resolve the active <see cref="WeaponController"/> and <see cref="ShooterCore"/>.
    /// 4) Rebind the active shooter to UI systems such as <see cref="CrosshairUI"/>.
    /// 5) Optionally listen to <see cref="IWeaponSwapInput"/> for runtime weapon switching.
    /// 
    /// Notes:
    /// - This manager assumes each weapon exists as a child under <c>weaponSocket</c>
    /// - It can also act a safety net when active weapons are changed externally.
    /// </summary>
    public sealed class WeaponManager : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private Transform weaponSocket;
        [SerializeField] private CrosshairUI crosshairUI;

        [Header("Weapon Swap Input (Optional)")]
        [Tooltip("Assign a component that implements IWeaponSwapInput(for example, CombatInputReader).")]
        [SerializeField] private MonoBehaviour swapInputBehaviour;

        [Header("Auto Detect (Safety Net)")]
        [SerializeField] private bool autoDetectActiveWeapon = true;
        [SerializeField] private float pollInterval = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        #endregion

        #region Cached References / State

        private IWeaponSwapInput _swapInput;

        private readonly List<GameObject> weapons = new();
        private int currentIndex = -1;

        private ShooterCore boundShooter;
        private float pollTimer;

        #endregion

        #region Properties / Events

        /// <summary>
        /// Raised after the active weapon changes.
        /// Provides the active weapon GameObject and its index.
        /// </summary>
        public event Action<GameObject, int> WeaponChanged;

        /// <summary>
        /// Gets the active weapon controller currently found under the weapon socket.
        /// </summary>
        public WeaponController CurrentWeaponController
        {
            get
            {
                var weaponGo = FindActiveWeaponUnderSocket(weaponSocket);
                if (weaponGo == null)
                {
                    return null;
                }

                return weaponGo.GetComponentInChildren<WeaponController>(true);
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (weaponSocket == null)
            {
                Debug.LogError("[WeaponManager] weaponSocket is NULL", this);
            }

            if (crosshairUI == null)
            {
                Debug.LogError("[WeaponManager] crosshairUI is NULL", this);
            }

            _swapInput = swapInputBehaviour as IWeaponSwapInput;
            if (swapInputBehaviour != null && _swapInput == null)
            {
                Debug.LogError("[WeaponManager] swapInputBehaviour does not implement IWeaponSwapInput.", this);
            }

            RebuildWeaponCache();
        }

        private void OnEnable()
        {
            if (_swapInput != null)
            {
                _swapInput.SlotRequested += EquipByIndex;
                _swapInput.NextRequested += EquipNext;
                _swapInput.PrevRequested += EquipPrev;
            }
        }

        private void OnDisable()
        {
            if (_swapInput != null)
            {
                _swapInput.SlotRequested -= EquipByIndex;
                _swapInput.NextRequested -= EquipNext;
                _swapInput.PrevRequested -= EquipPrev;
            }
        }

        private void Start()
        {
            // Bind to the currently active weapon at startup.
            ForceRebind();

            // If no weapon is active but weapons exist, activate slot 0
            if (currentIndex < 0 && weapons.Count > 0)
            {
                EquipByIndex(0);
            }
        }

        private void Update()
        {
            if (!autoDetectActiveWeapon || weaponSocket == null || crosshairUI == null)
            {
                return;
            }

            pollTimer += Time.deltaTime;
            if (pollTimer < pollInterval)
            {
                return;
            }

            pollTimer = 0f;

            // Safety net: if weapon were toggled weapons externally, keep UI and bindings consistent.
            ForceRebind();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Equips the next weapon in the cached list.
        /// </summary>
        public void EquipNext()
        {
            if (weapons.Count == 0)
            {
                return;
            }

            int next = (currentIndex + 1) % weapons.Count;
            EquipByIndex(next);
        }

        /// <summary>
        /// Equips the previous weapon in the cached list.
        /// </summary>
        public void EquipPrev()
        {
            if (weapons.Count == 0)
            {
                return;
            }

            int prev = (currentIndex - 1 + weapons.Count) % weapons.Count;
            EquipByIndex(prev);
        }

        /// <summary>
        /// Equips the weapon at the specified cached index.
        /// </summary>
        public void EquipByIndex(int index)
        {
            if (swapInputBehaviour is TPSCombatSystem.Input.CombatInputReader inputReader)
            {
                inputReader.ResetFireState();
            } 

            if (weapons.Count == 0)
            {
                RebuildWeaponCache();

                Debug.LogWarning("[WeaponManager] No weapons found under weaponSocket.", this);
                return;
            }

            index = Mathf.Clamp(index, 0, weapons.Count - 1);

            if (index == currentIndex)
            {
                return;
            }

            // Deactivate current weapon.
            if (currentIndex >= 0 && currentIndex < weapons.Count)
            {
                GameObject currentWeapon = weapons[currentIndex];
                if (currentWeapon != null)
                {
                    currentWeapon.SetActive(false);
                }
            }

            // Active next weapon.
            GameObject nextWeapon = weapons[index];
            if (nextWeapon == null)
            {
                Debug.LogWarning("[WeaponManager] No weapons found under weaponSocket.", this);
                return;
            }

            nextWeapon.SetActive(true);
            currentIndex = index;

            // Immediate rebind (no need to wait for polling).
            RebindFromWeaponGO(nextWeapon);

            WeaponChanged?.Invoke(nextWeapon, currentIndex);
        }

        /// <summary>
        /// Rebuilds the internal weapon cache from children under the weapon weapon socket.
        /// </summary>
        [ContextMenu("Rebuild Weapon Cache")]
        public void RebuildWeaponCache()
        {
            weapons.Clear();

            if (weaponSocket == null)
            {
                return;
            }

            for (int i = 0; i < weaponSocket.childCount; i++)
            {
                Transform child = weaponSocket.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                weapons.Add(child.gameObject);
            }
        }

        /// <summary>
        /// Re-resolve the active weapon under the socket and rebinds dependent UI/runtime references.
        /// </summary>
        [ContextMenu("Force Rebind")]
        public void ForceRebind()
        {
            GameObject weaponGo = FindActiveWeaponUnderSocket(weaponSocket);

            if (weaponGo == null)
            {
                if (boundShooter != null)
                {
                    boundShooter = null;

                    if (crosshairUI != null)
                    {
                        crosshairUI.SetShooter(null);
                    }
                }

                currentIndex = FindIndexOfActiveWeapon();
                WeaponChanged?.Invoke(null, currentIndex);
                return;
            }

            currentIndex = FindWeaponIndex(weaponGo);
            RebindFromWeaponGO(weaponGo);
            WeaponChanged?.Invoke(weaponGo, currentIndex);
        }

        #endregion

        #region Internal Binding

        /// <summary>
        /// Resolves the weapon controller and shooter from the given weapon object,
        /// then rebinds the crosshair UI if needed.
        /// </summary>
        private void RebindFromWeaponGO(GameObject weaponGo)
        {
            WeaponController weaponController = 
                weaponGo != null ? weaponGo.GetComponentInChildren<WeaponController>(true) : null;

            ShooterCore shooter = 
                weaponController != null ? weaponController.Shooter : null;

            Log($"[WeaponManager] activeWeapon={(weaponGo ? weaponGo.name : "NULL")}" +
                $"wc={(weaponController ? weaponController.name : "NULL")}"+
                $"shooter={(shooter ? shooter.name : "NULL")}"
                );

            // Only rebind if the shooter actually changed.
            if (shooter == boundShooter)
            {
                return;
            }

            boundShooter = shooter;

            if (crosshairUI != null)
            {
                crosshairUI.SetShooter(boundShooter);
            }

            Log($"[WeaponManager] Rebind Crosshair -> shooter={(boundShooter ? boundShooter.name : "NULL")}");
        }

        #endregion

        #region Lookup Helpers

        /// <summary>
        /// Finds the cached index of the currently active weapon under the socket.
        /// </summary>
        private int FindIndexOfActiveWeapon()
        {
            GameObject active = FindActiveWeaponUnderSocket(weaponSocket);
            return FindWeaponIndex(active);
        }

        /// <summary>
        /// Finds the cached index of the specified weapon object.
        /// </summary>
        private int FindWeaponIndex(GameObject weaponGo)
        {
            if (weaponGo == null)
            {
                return -1;
            }

            if (weapons.Count == 0)
            {
                RebuildWeaponCache();
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] == weaponGo)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the first active weapon GameObject under the given socket.
        /// </summary>
        private static GameObject FindActiveWeaponUnderSocket(Transform socket)
        {
            if (socket == null)
            {
                return null;
            }

            for (int i = 0; i < socket.childCount; i++)
            {
                Transform child = socket.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    return child.gameObject;
                }
            }
            return null;
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