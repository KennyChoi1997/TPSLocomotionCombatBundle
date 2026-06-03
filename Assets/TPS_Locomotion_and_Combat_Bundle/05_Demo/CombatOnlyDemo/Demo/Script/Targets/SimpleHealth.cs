using UnityEngine;
using TPSCombatSystem.Interfaces;
using System;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Minimal damage reciever for testing.
    /// Put this on the root object of a dummy or enemy target.
    /// </summary>
    public sealed class SimpleHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;

        private float current;
        private bool isDead;

        public event Action Damaged;
        public event Action Died;

        public float CurrentHealth => current;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            current = maxHealth;
        }

        public void TakeDamage(in DamageInfo info)
        {
            if (isDead)
            {
                return;
            }

            current -= info.Amount;

            Debug.Log($"[SimpleHealth] {name} took {info.Amount}. HP={current}/{maxHealth}", this);
            Damaged?.Invoke();
            
            if (current <= 0f)
            {
                isDead = true;
                current = 0f;

                Debug.Log($"[SimpleHealth] {gameObject.name} died (demo).", this);
                Died?.Invoke();
            }
        }
    }
}