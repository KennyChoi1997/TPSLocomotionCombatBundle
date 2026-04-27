using UnityEngine;

namespace TPSCombatSystem.Utils
{
    /// <summary>
    /// Automatically destroys the GameObject after a specified lifetime.
    /// 
    /// Useful for temporary objects such as VFX, decals, or projectiles.
    /// </summary>
    public sealed class AutoDestroy : MonoBehaviour
    {
        #region Inspector Fields

        [Tooltip("Lifetime in seconds before this object is destroyed.")]
        [SerializeField, Min(0f)] private float lifeTime = 1f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if(lifeTime <= 0f)
            {
                return;
            }

            Destroy(gameObject, lifeTime);
        }

        #endregion
    }
}