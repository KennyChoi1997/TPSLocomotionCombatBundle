using UnityEngine;
using TPSCombatSystem.Interfaces;
using TPSCombatSystem.Demo;

namespace TPSCombatSystem.Hit
{
    /// <summary>
    /// Represents a body-part category used by <see cref="HitBox"/>.
    /// </summary>
    public enum HitBoxPart
    {
        Head,
        Torso,
        Arm,
        Leg
    }

    /// <summary>
    /// Child hitbox component that forwards damage to an <see cref="IDamageable"/>.
    /// 
    /// Responsibilities:
    /// 1) Identify which body part was hit.
    /// 2) Apply a body-part-specific damage multiplier.
    /// 3) Forward the adjusted damage to a target damage receiver.
    /// 
    /// Notes:
    /// - Typically placed on child colliders, often configured as triggers.
    /// - If no explicit receiver is alligned, the component searches parent objects.
    /// </summary>
    public sealed class HitBox : MonoBehaviour
    {
        #region Inspecter Fields

        [Header("HitBox")]
        [SerializeField] private HitBoxPart part = HitBoxPart.Torso;

        [Tooltip("Damage multiplier applied for this body part (for example, Head = 2.0).")]
        [SerializeField] private float damageMultiplier = 1f;

        [Header("Routing")]
        [Tooltip("Optional override. If null, an IDamageable is searched in parent objects.")]
        [SerializeField] private MonoBehaviour damageReceiverBehaviour; // Must implement IDamageable

        #endregion

        #region Cached References / Properties

        private IDamageable _receiver;

        /// <summary>
        /// Gets which body part this hitbox represents.
        /// </summary>
        public HitBoxPart Part => part;

        /// <summary>
        /// Gets the damage multiplier applied by this hitbox.
        /// </summary>
        public float Multiplier => damageMultiplier;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (damageReceiverBehaviour != null)
            {
                _receiver = damageReceiverBehaviour as IDamageable;
            }

            if (_receiver == null)
            {
                _receiver = GetComponentInParent<IDamageable>();
            }

            if (_receiver == null)
            {
                Debug.LogError("[HitBox] No IDamageable found in parents.", this);
            } 
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempts to apply damage through this hitbox.
        /// A new <see cref="DamageInfo"/> is created with the adjusted damage amount.
        /// </summary>
        public bool TryApplyDamage(in DamageInfo baseInfo)
        {
            if (_receiver == null)
            {
                return false;
            }

            var info = new DamageInfo(
                baseInfo.Amount * damageMultiplier,
                baseInfo.Point,
                baseInfo.Normal,
                baseInfo.Direction,
                baseInfo.Instigator
                );

            _receiver.TakeDamage(in info);

            DemoTargetAnimator targetAnimator = GetComponentInParent<DemoTargetAnimator>();
            if(targetAnimator != null)
            {
                targetAnimator.ReactToHit(part);
            }

            return true;
        }

        #endregion
    }
}