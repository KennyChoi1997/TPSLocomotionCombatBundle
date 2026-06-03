using TPSCombatSystem.Feedback;
using TPSCombatSystem.Hit;
using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSCombatSystem.Damage
{
    /// <summary>
    /// Default runtime damage resolver.
    /// 
    /// Responsibilities:
    /// 1) Spawn impact feedback at the hit point.
    /// 2) Prefer Hitbox-based damage routing when configured.
    /// 3) Fallback to parent <see cref="IDamageable"/> lookup when no Hitbox is used.
    /// 
    /// Notes:
    /// - This keeps <see cref="TPSCombatSystem.Core.ShooterCore"/> independent.
    ///   from specific health or hitbox implementations.
    /// - Hitbox priority is useful for body-part specific damage handling.
    /// </summary>
    public sealed class DefaultDamageResolver : MonoBehaviour, IDamageResolver
    {
        #region Inspector Fields

        [Header("HitBox")]
        [Tooltip("If true, HitBox takes priority when present on the hit collider.")]
        [SerializeField] private bool preferHitBox = true;

        [Header("Impact FX")]
        [Tooltip("Optional impact FX spawner for visual hit feedback.")]
        [SerializeField] private ImpactFxSpawner impactFx;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        #endregion

        #region IDamageResolver

        /// <summary>
        /// Attempts to resolve a raycast hit into actual damage application.
        /// </summary>
        public bool TryResolve(in RaycastHit hit, in DamageInfo baseInfo)
        {
            Log("[DamageResolver] TryResolve CALLED");

            // Spawn impact feedback first so non-damageable surface can still
            // produce visual hit response.
            impactFx?.Spawn(hit.point, hit.normal);

            if (preferHitBox &&
                hit.collider != null &&
                hit.collider.TryGetComponent<HitBox>(out var hitBox))
            {
                Log("[DamageResolver] Resolved through Hitbox.");
                return hitBox.TryApplyDamage(in baseInfo);
            }

            if (hit.collider == null)
            {
                Log("[DamageResolver] No collider on hit.");
                return false;
            }

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                Log("[DamageResolver] No IDamageable found in parent chain.");
                return false;
            }

            damageable.TakeDamage(in baseInfo);
            Log("[DamageResolver] Resolved through IDamageable");
            return true;
        }

        #endregion

        #region Helpers

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message);
            }
        }

        #endregion
    }
}