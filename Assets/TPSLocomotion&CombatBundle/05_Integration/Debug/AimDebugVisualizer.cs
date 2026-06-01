using TPSCombatSystem.Core;
using TPSCombatSystem.Interfaces;
using UnityEngine;

namespace TPSLocomotionCombatBundle.Integration.Debugging
{
    public sealed class AimDebugVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour aimProviderBehaviour; // IAimProvider
        [SerializeField] private ShooterCore shooterCore;

        [Header("Aim Blocking")]
        [SerializeField] private LayerMask aimBlockMask = ~0;

        [Header("Debug")]
        [SerializeField] private bool drawDebug = true;
        [SerializeField] private float debugDistance = 50f;
        [SerializeField] private float sphereRadius = 0.008f;

        private IAimProvider aimProvider;

        private void Awake()
        {
            aimProvider = aimProviderBehaviour as IAimProvider;
        }

        private void Update()
        {
            if (!drawDebug || aimProvider == null || shooterCore == null)
            {
                return;
            }

            IWeapon weapon = shooterCore.CurrentWeapon;

            if(weapon==null || weapon.Muzzle == null)
            {
                return;
            }

            Ray aimRay = aimProvider.GetAimRay();

            bool aimHit = Physics.Raycast(
                aimRay,
                out RaycastHit cameraHit,
                weapon.MaxDistance,
                aimBlockMask,
                QueryTriggerInteraction.Collide
                );

            Vector3 aimPoint = aimHit
                ? cameraHit.point
                : aimRay.origin + aimRay.direction * debugDistance;

            Vector3 fireOrigin = weapon.Muzzle.position;
            Vector3 fireDirection = (aimPoint - fireOrigin).normalized;

            bool fireHit = Physics.Raycast(
                fireOrigin,
                fireDirection,
                out RaycastHit actualHit,
                weapon.MaxDistance,
                weapon.HitMask,
                QueryTriggerInteraction.Collide
                );

            // Debug.DrawRay(aimRay.origin, aimRay.direction * debugDistance, Color.blue);
            if (aimHit)
            {
                Debug.DrawRay(aimRay.origin, 
                    cameraHit.point, 
                    Color.blue
                    );
            }
            else
            {
                Debug.DrawRay(aimRay.origin, 
                    aimRay.direction * debugDistance, 
                    Color.blue);
            }

            Debug.DrawRay(fireOrigin, fireDirection * debugDistance, Color.red);

            DrawDebugSphere(aimPoint, sphereRadius, Color.green);

            if (fireHit)
            {
                DrawDebugSphere(actualHit.point, sphereRadius, Color.yellow);
            }
        }

        private static void DrawDebugSphere(Vector3 position, float radius, Color colour)
        {
            Debug.DrawRay(position + Vector3.up * radius, position - Vector3.up * radius, colour);
            Debug.DrawRay(position + Vector3.right * radius, position - Vector3.right * radius, colour);
            Debug.DrawRay(position + Vector3.forward * radius, position - Vector3.forward * radius, colour);
        }
    }
}