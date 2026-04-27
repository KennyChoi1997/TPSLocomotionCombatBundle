using TPSCombatSystem.Hit;
using UnityEngine;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Demo-only target animator driver.
    /// 
    /// Rersponsibilities:
    /// 1) Play hit reaction animations based on hit body part.
    /// 2) Play death animation when health reaches zero.
    /// 3) Despawn the target after death so the spawn group can replace it.
    /// </summary>
    public sealed class DemoTargetAnimator : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private SimpleHealth health;

        [Header("Death Spawn")]
        [SerializeField] private float despawnDelay = 1.5f;
        [SerializeField] private bool destroyOnDeath = true;

        #endregion

        #region Animator Hashes

        private static readonly int HitHeadHash = Animator.StringToHash("HitHead");
        private static readonly int HitChestHash = Animator.StringToHash("HitChest");
        private static readonly int DieHash = Animator.StringToHash("Die");

        #endregion

        #region State

        private bool isDead;
        private float despawnTimer;

        #endregion

        #region Unity Lifecycle
        private void Reset()
        {
            if(animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (health == null)
            {
                health = GetComponent<SimpleHealth>();
            }
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (health == null)
            {
                health = GetComponent<SimpleHealth>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        void Update()
        {
            if (!isDead)
            {
                return;
            }

            despawnTimer -= Time.deltaTime;
            if (despawnTimer <= 0f)
            {
                DespawnTarget();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Plays a hit reaction based on the given hitbox part.
        /// </summary>
        public void ReactToHit(HitBoxPart part)
        {
            if (animator == null || isDead)
            {
                return;
            }

            switch (part)
            {
                case HitBoxPart.Head:
                    animator.SetTrigger(HitHeadHash);
                    break;

                case HitBoxPart.Torso:
                case HitBoxPart.Arm:
                case HitBoxPart.Leg:
                default:
                    animator.SetTrigger(HitChestHash);
                    break;
            }
        }

        #endregion

        #region Death / Reset

        private void OnDied()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            despawnTimer = despawnDelay;

            if (animator != null)
            {
                animator.SetTrigger(DieHash);
            }
        }

        private void DespawnTarget()
        {
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #endregion
    }
}