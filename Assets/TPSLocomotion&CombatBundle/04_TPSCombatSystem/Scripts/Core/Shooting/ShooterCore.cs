using TPSCombatSystem.Input;
using TPSCombatSystem.Interfaces;
using TPSCombatSystem.Utils;
using UnityEngine;

namespace TPSCombatSystem.Core
{
    /// <summary>
    /// Minimal hitscan shooting core.
    /// 
    /// Responsibilities:
    /// 1) Get an aim ray from <see cref="IAimProvider"/>.
    /// 2) Resolve an aim point from the ray (hit point or fallback far point).
    /// 3) Resolve the final fire directionn from the muzzle.
    /// 4) Perform hitscan raycast.
    /// 5) Resolve damage and publish shot feedback result.
    /// 
    /// Notes:
    /// - Weapon is typically runtime-injected via <see cref="SetWeapon"/>.
    /// - AimProvider and DamageResolver are expected to be assigned in the Inspector.
    /// - Demo-specific logic should stay outside this class.
    /// </summary>
    public sealed class ShooterCore : MonoBehaviour
    {
        /// <summary>
        /// Controls how the final fire direction is computed.
        /// </summary>
        public enum FireDirectionMode
        {
            /// <summary>
            /// Fire from the muzzle toward the resolve camera aim point.
            /// Best for crosshair-aligned TPS aiming.
            /// </summary>
            CameraAimToMuzzle=0,

            /// <summary>
            /// Fire directly along the weapon muzzle forward axis.
            /// BEst for hip fire or intentionally "physical" muzzle-forward shots.
            /// </summary>
            TrueMuzzleForward=1,
        }

        /// <summary>
        /// Invoked after a shot is evaluated.
        /// True = any hit was detected, false = no hit.
        /// Usually consumed by UI / crosshair feedback system.
        /// </summary>
        public event System.Action<bool> ShotResult;

        #region Inspector Fields

        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour aimProviderBehaviour;    // IAimProvider
        [SerializeField] private MonoBehaviour damageResolverBehaviour; // IDamageResolver

        [Header("Weapon (Runtime Inject)")]
        [SerializeField] private MonoBehaviour weaponBehaviour;         // IWeapon (optional fallback for inspector testing)

        [Header("Direction Policy")]
        [SerializeField] private FireDirectionMode aimingDirectionMode = FireDirectionMode.CameraAimToMuzzle;
        [SerializeField] private FireDirectionMode hipFireDirectionMode = FireDirectionMode.TrueMuzzleForward;

        [Header("Aim State")]
        [SerializeField] private CombatInputReader combatInputReader;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private bool drawDebugRay = true;

        #endregion

        #region Cached References

        private IAimProvider aimProvider;
        private IDamageResolver damageResolver;
        private IWeapon weapon;

        #endregion

        #region Constants / Internal Data

        private const float MinDirectionSqrMagnitude = 0.0001f;

        /// <summary>
        /// Internal one-shot data bundle used to avoid recalculating
        /// aim, muzzle, and weapon parameters multiple times per shot.
        /// </summary>
        private struct ShotSetup
        {
            public Ray AimRay;
            public Vector3 AimPoint;
            public Vector3 Origin;
            public Vector3 Direction;
            public Transform Muzzle;
            public float MaxDistance;
            public LayerMask HitMask;
            public float Damage;
            public bool IsAiming;
            public FireDirectionMode Mode;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            aimProvider = aimProviderBehaviour as IAimProvider;
            damageResolver = damageResolverBehaviour as IDamageResolver;
            weapon = weaponBehaviour as IWeapon;

            if (aimProvider == null)
            {
                Debug.LogError("[ShooterCore] AimProvider missing or invalid.", this);
            }

            if (damageResolver == null)
            {
                Debug.LogError("[ShooterCore] DamageResolver missing or invalid.", this);
            } 
        }

        private void OnDisable()
        {
            // Stop any lingering haptics when this shooter is disabled.
            TPSCombatSystem.Feedback.Haptics.Stop();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Replaces the currently active mweapon reference at runtime.
        /// Usually called by weapon swap / weapon manager systems.
        /// </summary>
        public void SetWeapon(IWeapon newWeapon)
        {
            weapon = newWeapon;

            if (weapon == null)
            {
                Debug.LogError("[ShooterCore] SetWeapon failed. Null weapon.", this);
            }
        }

        /// <summary>
        /// Gets the currently assigned runtime weapon.
        /// </summary>
        public IWeapon CurrentWeapon => weapon;

        /// <summary>
        /// Publishes an external shot result to feedback listeners.
        /// Useful when another system aggregates multiple hits first
        /// (for example shotgun pellets) and then sends a final result.
        /// </summary>
        public void PublishShotResult(bool hit)
        {
            ShotResult?.Invoke(hit);
        }

        /// <summary> 
        /// Perform a single hitscan shot using the current weapon configuration. 
        /// </summary>
        public bool TryFire()
        {
            if (enableDebugLog)
            {
                Log($"[ShooterCore] TryFire on {name}");
            }
            
            if(!TryBuildShotSetup(out ShotSetup setup))
            {
                return false;
            }

            return FireRay(setup);
        }

        /// <summary> 
        /// Performs a single hitscan shot using a caller-provided direction.
        /// Intended for shotgun pellets or custom spread logic.
        /// This method does not publish <see cref="ShotResult"/> automatically.
        /// </summary>
        public bool TryFireWithDirection(Vector3 overrideDirection)
        {
            if(!TryBuildBaseSetup(out ShotSetup setup))
            {
                return false;
            }

            setup.Direction = overrideDirection.sqrMagnitude > MinDirectionSqrMagnitude
                ? overrideDirection.normalized
                : setup.AimRay.direction;

            return FireRay(setup);
        }

        /// <summary> 
        /// Gets the current base fire direction before spread is applied.
        /// Useful for shotgun or multi-projectile logic.
        /// </summary>
        public Vector3 GetBaseFireDirection()
        {
            if(!TryBuildShotSetup(out ShotSetup setup))
            {
                return Vector3.forward;
            }

            return setup.Direction;
        }

        #endregion

        #region Shot Setup

        /// <summary>
        /// Builds the full shot setup including:
        /// - aim state
        /// - direction mode
        /// - resolved final fire direction
        /// </summary>
        private bool TryBuildShotSetup(out ShotSetup setup)
        {
            if(!TryBuildBaseSetup(out setup))
            {
                return false;
            }

            setup.IsAiming = combatInputReader != null && combatInputReader.IsAiming;
            setup.Mode = setup.IsAiming ? aimingDirectionMode : hipFireDirectionMode;

            setup.Direction = ResolveFireDirection(
                setup.AimRay,
                setup.AimPoint,
                setup.Origin,
                setup.Muzzle,
                weapon.ForwardAxis,
                setup.Mode
            );

            return true;
        }

        /// <summary>
        /// Builds the common base setup shared by normal fire and custom-direction fire.
        /// </summary>
        private bool TryBuildBaseSetup(out ShotSetup setup)
        {
            setup = default;

            if (aimProvider == null || weapon == null)
            {
                return false;
            }

            Transform muzzle = weapon.Muzzle;
            if (muzzle == null)
            {
                return false;
            }

            setup.AimRay = aimProvider.GetAimRay();
            setup.MaxDistance = weapon.MaxDistance;
            setup.HitMask = weapon.HitMask;
            setup.Damage = weapon.Damage;
            setup.Muzzle = muzzle;
            setup.Origin = muzzle.position;
            setup.AimPoint = ResolveAimPoint(setup.AimRay, setup.MaxDistance, setup.HitMask);

            return true;
        }

        #endregion

        #region Aim / Direction Resolve

        /// <summary>
        /// Resolves the camera aim point.
        /// If the aim ray hits something valid, use that point.
        /// Otherwise, use a fallback point at max distance.
        /// </summary>
        private Vector3 ResolveAimPoint(Ray aimRay, float maxDistance, LayerMask hitMask)
        {
            if(Physics.Raycast(
                aimRay,
                out RaycastHit aimHit,
                maxDistance,
                hitMask,
                QueryTriggerInteraction.Ignore))
            {
                return aimHit.point;
            }

            return aimRay.origin + aimRay.direction * maxDistance;
        }

        /// <summary>
        /// Resolves the final fire direction based on the selected direction policy.
        /// </summary>
        private Vector3 ResolveFireDirection(
            Ray aimRay,
            Vector3 aimPoint,
            Vector3 origin,
            Transform muzzle,
            WeaponForwardAxis forwardAxis,
            FireDirectionMode mode)
        {
            Vector3 muzzleForward = WeaponDirectionUtil.GetAxisDir(muzzle, forwardAxis).normalized;

            switch (mode)
            {
                case FireDirectionMode.TrueMuzzleForward:
                    return muzzleForward;

                case FireDirectionMode.CameraAimToMuzzle:
                default:
                    {
                        Vector3 direction = aimPoint - origin;

                        if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                        {
                            direction = aimRay.direction;
                        }

                        return aimRay.direction;

                        /*
                        // Clamp desired camera-based direction so the weapon cannot
                        // fire outside the physically plausible muzzle cone.
                        return ClampDirectionByYawPitch(
                            muzzle,
                            forwardAxis,
                            direction.normalized, 
                            weapon.MaxHorizontalAimDeviationDeg,
                            weapon.MaxVerticalAimDeviationDeg
                            );
                        */
                    }
            }
        }

        #endregion

        #region Fire Execution

        /// <summary>
        /// Executes the final hitscan raycast and forwards damage resolution if a hit is found.
        /// </summary>
        private bool FireRay(in ShotSetup setup)
        {
            Ray fireRay = new Ray(setup.Origin, setup.Direction);

            if (drawDebugRay)
            {
                Debug.DrawRay(setup.Origin, setup.Direction * setup.MaxDistance, Color.red, 0.25f);
            }

            if (!Physics.Raycast(
                fireRay, 
                out RaycastHit fireHit, 
                setup.MaxDistance, 
                setup.HitMask, 
                QueryTriggerInteraction.Collide))
            {
                return false;
            }

            var baseInfo = new DamageInfo(
                setup.Damage,
                fireHit.point,
                fireHit.normal,
                setup.Direction,
                gameObject
                );

            damageResolver?.TryResolve(in fireHit, in baseInfo);
            return true;
        }

        #endregion

        #region Helper

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message, this);
            }
        }

        private Vector3 ClampDirectionByYawPitch(
            Transform muzzle,
            WeaponForwardAxis forwardAxis,
            Vector3 desiredWorldDirection,
            float maxYawDeg,
            float maxPitchDeg)
        {
            // If both limits are zero or less, treat clamp as disabled.
            if (maxYawDeg <= 0f && maxPitchDeg <= 0f)
            {
                return desiredWorldDirection.normalized;
            }

            Vector3 forward = WeaponDirectionUtil.GetAxisDir(muzzle, forwardAxis).normalized;
            Quaternion toForward = Quaternion.FromToRotation(Vector3.forward, forward);

            Quaternion toLocal = Quaternion.Inverse(toForward);
            Vector3 localDir = toLocal * desiredWorldDirection.normalized;

            float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
            float pitch = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            if (maxYawDeg > 0f)
            {
                yaw = Mathf.Clamp(yaw, -maxYawDeg, maxYawDeg);
            }
            
            if(maxPitchDeg > 0f)
            {
                pitch = Mathf.Clamp(pitch, -maxPitchDeg, maxPitchDeg);
            }

            Quaternion clampedLocalRot = Quaternion.Euler(-pitch, yaw, 0f);
            Vector3 clapmedLocalDir = clampedLocalRot * Vector3.forward;

            Vector3 finalDir = toForward * clapmedLocalDir;
            return finalDir.normalized;
        }

        #endregion
    }
}