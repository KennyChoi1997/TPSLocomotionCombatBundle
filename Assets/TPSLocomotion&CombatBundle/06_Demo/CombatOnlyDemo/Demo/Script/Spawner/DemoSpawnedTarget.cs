using UnityEngine;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Runtime link between a spawned target instance and its owning spawn group.
    /// </summary>
    public sealed class DemoSpawnedTarget : MonoBehaviour
    {
        private DemoTargetSpawnGroup ownerGroup;
        private SimpleHealth health;
        private bool notifiedDeath;

        public void Initialize(DemoTargetSpawnGroup group)
        {
            ownerGroup = group;
            notifiedDeath = false;
        }

        private void Awake()
        {
            health = GetComponent<SimpleHealth>();
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

        private void OnDied()
        {
            if (notifiedDeath)
            {
                return;
            }

            notifiedDeath = true;
            ownerGroup?.NotifyTargetDied(this);
        }
    }
}