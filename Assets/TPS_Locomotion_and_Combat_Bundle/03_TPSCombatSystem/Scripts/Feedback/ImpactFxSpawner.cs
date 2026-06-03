using UnityEngine;

namespace TPSCombatSystem.Feedback
{
    /// <summary>
    /// Spawns impact visual effects at a hit location.
    /// 
    /// Responsibilities:
    /// 1) Instantiate an impact prefab at the hit point.
    /// 2) Optionally allign the effect to the surface normal.
    /// 3) Apply a small offset to avoid z-fighting.
    /// 
    /// Notes:
    /// - Typically used by damage resolvers after a successful hit.
    /// - Prefab should face forward (Z+) for correct normal allignment.
    /// </summary>
    public sealed class ImpactFxSpawner : MonoBehaviour
    {
        #region Inspector Fields

        [Header("FX")]
        [SerializeField] private GameObject impactPrefab;

        [Tooltip("If true, aligns the effect forward direction to the hit normal.")]
        [SerializeField] private bool alignToNormal = true;

        [Tooltip("Small offset along normal to prevent z-fighting.")]
        [SerializeField] private float offset = 0.01f;

        [Header("Optional")]
        [Tooltip("Optional parent transform for spawned FX.")]
        [SerializeField] private Transform parent;

        [Tooltip("Auto-destroy spawned FX after this time (seconds). Set 0 to disable.")]
        [SerializeField] private float autoDestroyTime = 2f;

        #endregion

        #region Public API

        /// <summary>
        /// Spawns an impact effect at the given point and normal.
        /// </summary>
        public void Spawn(Vector3 point, Vector3 normal)
        {
            if (impactPrefab == null)
            {
                return;
            }

            Quaternion rotation = alignToNormal
                ? Quaternion.LookRotation(normal)
                : Quaternion.identity;

            Vector3 position = point + normal * offset;

            GameObject fx = Instantiate(impactPrefab, position, rotation, parent);

            if (autoDestroyTime > 0f)
            {
                Destroy(fx, autoDestroyTime);
            }
        }

        #endregion
    }
}