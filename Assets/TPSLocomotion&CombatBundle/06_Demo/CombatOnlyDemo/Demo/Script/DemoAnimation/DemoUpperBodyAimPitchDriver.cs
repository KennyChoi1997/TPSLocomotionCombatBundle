using TPSCombatSystem.CameraSystem;
using TPSCombatSystem.Input;
using UnityEngine;

namespace TPSCombatSystem.Demo
{
    /// <summary>
    /// Demo-Only upper body pitch driver.
    /// 
    /// Responsibilities:
    /// 1) Read current aim pitch from <see cref="CombatAimCameraController"/>.
    /// 2) Blend pitch influence in and out based on aiming state.
    /// 3) Apply additive local pitch offsets to humanoid upper-body bones.
    /// 
    /// Notes:
    /// - Intended for demo/sample presentation only.
    /// - Requires a humanoid Aniamtor setup.
    /// - Applies pitch after Animator evaluation in <see cref="LateUpdate"/>.
    /// </summary>
    public sealed class DemoUpperBodyAimPitchDriver : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private CombatInputReader combatInputReader;
        [SerializeField] private CombatAimCameraController aimCameraController;

        [Header("Blend")]
        [SerializeField] private float blendInSpeed = 12f;
        [SerializeField] private float blendOutSpeed = 10f;

        [Header("Pitch Mapping")]
        [Tooltip("Scales the camera pitch before applying it to the upper body.")]
        [SerializeField] private float pitchScale = 0.35f;

        [Tooltip("Use -1 if upper body rotates in the opposite direction.")]
        [SerializeField] private float pitchSign = 1f;

        [SerializeField] private float maxUpDegrees = 22f;
        [SerializeField] private float maxDownDegrees = -16f;

        [Header("Bone Weights")]
        [SerializeField] private float spineWeight = 0.20f;
        [SerializeField] private float chestWeight = 0.35f;
        [SerializeField] private float upperChestWeight = 0.30f;
        [SerializeField] private float headWeight = 0.15f;

        #endregion

        #region Cached Bones / State

        private Transform spine;
        private Transform chest;
        private Transform upperChest;
        private Transform head;

        private float blend;

        #endregion

        #region Unity Lifecycle

        private void Reset()
        {
            AssignReferencesIfMissing();
        }

        private void Awake()
        {
            AssignReferencesIfMissing();
            CacheBones();
        }

        private void LateUpdate()
        {
            if (animator == null || !animator.isHuman || aimCameraController == null)
            {
                return;
            }

            bool isAiming = combatInputReader != null && combatInputReader.IsAiming;

            UpdateBlend(isAiming);

            if (blend <= 0.0001f)
            {
                return;
            }
            
            float finalPitch = BuildFinalPitch();

            ApplyAdditivePitch(spine, finalPitch * spineWeight);
            ApplyAdditivePitch(chest, finalPitch * chestWeight);
            ApplyAdditivePitch(upperChest, finalPitch * upperChestWeight);
            ApplyAdditivePitch(head, finalPitch * headWeight);
        }

        #endregion

        #region Setup

        private void AssignReferencesIfMissing()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (combatInputReader == null)
            {
                combatInputReader = GetComponent<CombatInputReader>();
            }

            if (aimCameraController == null)
            {
                aimCameraController = FindFirstObjectByType<CombatAimCameraController>(FindObjectsInactive.Include);
            }
        }

        /// <summary>
        /// Caches humanoid upper-body bone references for the Animator.
        /// </summary>
        private void CacheBones()
        {
            if (animator == null || !animator.isHuman)
            {
                return;
            }

            spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            upperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            head = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        #endregion

        #region Internal Flow

        /// <summary>
        /// Updates the current blend weight based on aiming state.
        /// </summary>
        private void UpdateBlend(bool isAiming)
        {
            float targetBlend = isAiming ? 1f : 0f;
            float blendSpeed = isAiming ? blendInSpeed : blendOutSpeed;

            blend = Mathf.MoveTowards(blend, targetBlend, blendSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Builds the final additive pitch value to distribute across upper-body bones.
        /// </summary>
        private float BuildFinalPitch()
        {
            float rawPitch = aimCameraController.CurrentPitch * pitchScale * pitchSign;
            float clampedPitch = Mathf.Clamp(rawPitch, maxDownDegrees, maxUpDegrees);
            return clampedPitch * blend;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Applies additive local pitch to the given bone.
        /// </summary>
        private static void ApplyAdditivePitch(Transform bone, float pitchDegrees)
        {
            if (bone == null || Mathf.Abs(pitchDegrees) < 0.0001f)
            {
                return;
            }

            bone.localRotation = bone.localRotation * Quaternion.Euler(pitchDegrees, 0f, 0f);
        }

        #endregion
    }
}