using UnityEngine;
using LocomotionSystem.Input;
using LocomotionSystem.UI;

namespace LocomotionSystem.CameraSystem
{
    /// <summary>
    /// Basic third-person camera controller for a Soulslike-style character.
    /// 
    /// Responsibilities:
    /// - Follows the player (CameraRoot).
    /// - Rotates based on Look input (mouse / right stick).
    /// - Supports lock-on to a single target.
    /// - Supports target switching (left/right).
    /// - Handles camera zoom and collision.
    /// </summary>
    public class ThirdPersonCameraController : MonoBehaviour
    {
        #region Serialized Fields - References

        [Header("References")]
        [Tooltip("Player root transform the camera rig should follow (e.g. CameraRoot parent).")]
        [SerializeField] private Transform target;

        [Tooltip("Pivot transform for camera rotation (usually a child of this rig).")]
        [SerializeField] private Transform pivot;

        [Tooltip("Input reader providing look / lock-on / target switch inputs.")]
        [SerializeField] private PlayerInputReader input;

        [Tooltip("PlayerLocomotion used to forward lock-on state.")]
        [SerializeField] private LocomotionSystem.Core.PlayerLocomotion _locomotion;

        #endregion

        #region Serialized Fields - Follow & Rotation

        [Header("Distance")]
        [Tooltip("Default camera distance from pivot in free-look mode.")]
        [SerializeField] private float distance = 3f;

        [Header("Follow Settings")]
        [Tooltip("Follow interpolation speed for the camera rig.")]
        [SerializeField] private float followSpeed = 10f;

        [Header("Rotation Settings")]
        [Tooltip("Horizontal/vertical rotation speed in degrees per second.")]
        [SerializeField] private float rotationSpeed = 120f;

        [Tooltip("Minimum pitch angle (downwards).")]
        [SerializeField] private float minPitch = -40f;

        [Tooltip("Maximum pitch angle (upwards).")]
        [SerializeField] private float maxPitch = 70f;

        [Tooltip("Invert vertical look input.")]
        [SerializeField] private bool invertY = false;

        #endregion

        #region Serialized Fields - Lock-On

        [Header("Lock-On Settings")]
        [Tooltip("Current lock-on target (enemy). Usually assigned at runtime.")]
        [SerializeField] private Transform _lockOnTarget;

        [Tooltip("Rotation speed while locked-on (player and camera).")]
        [SerializeField] private float _lockOnRotateSpeed = 10f;

        [Tooltip("Maximum angle from camera forward to allow lock-on.")]
        [SerializeField] private float _maxLockAngle = 80f;

        [Tooltip("Screen-space crosshair indicator for the lock-on target.")]
        [SerializeField] private LockOnIndicator _lockOnIndicator;

        [Header("Lock-On Camera Settings")]
        [Tooltip("Camera distance while lock-on is active.")]
        [SerializeField] private float _lockOnDistance = 6f;

        [Tooltip("Lerp speed when blending camera distance (free ¡ê lock-on).")]
        [SerializeField] private float _zoomLerpSpeed = 5f;

        [Tooltip("Minimum bias towards the lock-on target when close.")]
        [SerializeField] private float _minCentreBias = 0.25f;

        [Tooltip("Maximum bias towards the lock-on target when far.")]
        [SerializeField] private float _maxCentreBias = 0.5f;

        [Tooltip("Distance at which the follow bias is maxed out.")]
        [SerializeField] private float _maxBiasDistance = 12f;

        [Header("Lock-On Validator")]
        [Tooltip("Maximum distance for a valid lock-on target (auto-break beyond this).")]
        [SerializeField] private float _maxLockDistance = 20f;

        [Tooltip("Layer mask used to check obstacles that block line of sight to the target.")]
        [SerializeField] private LayerMask _obstacleMask;

        #endregion

        #region Serialized Fields - Target Search & Priority

        [Header("Target Search Settings")]
        [Tooltip("Radius around the player used to search for lock-on targets.")]
        [SerializeField] private float lockOnRadius = 15f;

        [Tooltip("Layer mask used to find lock-on candidates (enemies).")]
        [SerializeField] private LayerMask lockOnLayerMask;

        [Header("Target Priority")]
        [Tooltip("Weight for angle in target scoring (lower angle = better).")]
        [SerializeField] private float angleWeight = 1.0f;

        [Tooltip("Weight for distance in target scoring (closer = better).")]
        [SerializeField] private float distanceWeight = 0.02f;

        #endregion

        #region Serialized Fields - Camera Collision

        [Header("Camera Collision")]
        [Tooltip("Layer mask used to detect camera collisions (walls, environment).")]
        [SerializeField] private LayerMask _cameraCollisionMask;

        [Tooltip("Sphere radius used for camera collision checks.")]
        [SerializeField] private float _cameraRadius = 0.2f;

        [Tooltip("Minimum distance from pivot to camera when colliding.")]
        [SerializeField] private float _cameraMinDistance = 0.5f;

        [Tooltip("Offset to pull the camera away from the wall after collision.")]
        [SerializeField] private float _cameraCollisionOffset = 0.1f;

        #endregion

        #region Serialized Fields - Target Switching

        [Header("Target switching")]
        [Tooltip("Cooldown between target switches when using the target switch axis.")]
        [SerializeField] private float _switchCooldown = 0.3f;

        #endregion

        #region Private Runtime Fields

        private float _switchTimer = 0f;

        private float _yaw;
        private float _pitch;

        private Camera _cam;
        private bool _isLockOn;

        /// <summary> Public read-only access for other systems (UI, etc.) </summary>
        public bool IsLockOn => _isLockOn;
        public Transform LockOnTarget => _lockOnTarget;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            // Initialize rig position to player position
            if (target != null)
            {
                transform.position = target.position;
            }

            if (pivot != null)
            {
                pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }
        }

        private void Awake()
        {
            // Try to auto-find pivot if not assigned
            if (pivot == null && transform.childCount > 0)
                pivot = transform.GetChild(0);

            _cam = Camera.main;

            // Initialize camera local position (distance)
            if (_cam != null)
            {
                _cam.transform.localPosition = new Vector3(0f, 0f, -distance);
            }

            // Initialize yaw / pitch from current pivot rotation
            if (pivot != null)
            {
                Vector3 euler = pivot.rotation.eulerAngles;
                _yaw = euler.y;
                _pitch = euler.x;
            }

            // Ensure indicator starts hidden
            if (_lockOnIndicator != null)
                _lockOnIndicator.Hide();
        }

        private void LateUpdate()
        {
            if (target == null || input == null || pivot == null)
                return;

            // 1) Follow rig
            HandleFollow();

            // 2) Lock-on toggle / auto-break
            HandleLockOnToggle();
            HandleLockOnAutoBreak();

            // 3) Notify locomotion about current lock-on state
            _locomotion?.SetLockOnState(_isLockOn);

            // 4) Rotation mode
            if (_isLockOn)
                HandleLockOnRotation();
            else
                HandleFreeLookRotation();

            // 5) Target switching
            HandleTargetSwitch();

            // 6) Camera distance & collision
            if (_cam != null)
            {
                HandleCameraDistanceAndCollision();
            }
        }

        #endregion

        #region Camera Distance & Collision

        /// <summary>
        /// Handles camera distance (normal vs lock-on) and prevents the camera
        /// from clipping through walls by using a sphere cast from the pivot.
        /// </summary>
        private void HandleCameraDistanceAndCollision()
        {
            if (pivot == null || _cam == null)
                return;

            // 1) Desired distanc camera distance (free-look vs lock-on)
            float targetDist = _isLockOn ? _lockOnDistance : distance;

            // Camera is placed behind the pivot along -forward
            Vector3 camDir = -pivot.forward.normalized;
            float desiredDist = targetDist;

            // 2) Sphere cast: check for obstacles between pivot and desired camera position
            if (Physics.SphereCast(
                pivot.position,
                _cameraRadius,
                camDir,
                out RaycastHit hit,
                targetDist,
                _cameraCollisionMask,
                QueryTriggerInteraction.Ignore))
            {
                // Hit something -> stop the camera slightly before the wall
                float hitDist = hit.distance - _cameraCollisionOffset;

                // Clamp to a minimum distance so the camera doesn't go inside the player
                desiredDist = Mathf.Max(_cameraMinDistance, hitDist);
            }

            // 3) Smoothly interpolate current distance to desired distance
            Vector3 localPos = _cam.transform.localPosition;

            // local z is negative because the camera is behind the pivot
            float currentDist = -localPos.z;

            float newDist = Mathf.Lerp(
                currentDist,
                desiredDist,
                _zoomLerpSpeed * Time.deltaTime);

            localPos.z = -newDist;
            _cam.transform.localPosition = localPos;
        }

        #endregion

        #region Lock-On Validation & Search

        /// <summary>
        /// Checks if the target is valid for lock-on:
        /// - Not null
        /// - Within max angle and distance
        /// - Not occluded by obstacles
        /// </summary>
        private bool IsTargetValid(Transform targetTransform)
        {
            if (targetTransform == null || pivot == null)
                return false;

            Vector3 pivotPos = pivot.position;
            Vector3 targetPos = targetTransform.position;

            // Distance check
            float distSqr = (targetPos - pivotPos).sqrMagnitude;
            if (distSqr > _maxLockDistance * _maxLockDistance)
                return false;

            // Angle chceck (XZ plane)
            Vector3 camForward = pivot.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 toTarget = targetPos - pivotPos;
            toTarget.y = 0f;
            toTarget.Normalize();

            if (toTarget.sqrMagnitude < 0.0001f)
                return false;

            float angle = Vector3.Angle(camForward, toTarget);
            if (angle > _maxLockAngle)
                return false;

            // Occlusion check (raycast)
            Vector3 origin = pivotPos;
            Vector3 dir = (targetPos + Vector3.up * 1.0f) - origin;
            float dist = dir.magnitude;

            if (dist > 0.01f)
            {
                dir /= dist;

                if (Physics.Raycast(
                    origin,
                    dir,
                    out RaycastHit hit,
                    dist,
                    _obstacleMask,
                    QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform != targetTransform && hit.transform.root != targetTransform)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles lock-on input and toggles lock-on state.
        /// Also shows/hides the lock-on indicator.
        /// </summary>
        private void HandleLockOnToggle()
        {
            if (!input.LockOnPressed)
                return;

            if (_isLockOn)
            {
                // Turn off lock-on
                _isLockOn = false;
                _lockOnIndicator?.Hide();
            }
            else
            {
                // Try to acquire a new target
                _lockOnTarget = FindBestLockOnTarget();

                if (_lockOnTarget != null && IsTargetValid(_lockOnTarget))
                {
                    _isLockOn = true;

                    if (_lockOnIndicator != null)
                    {
                        _lockOnIndicator.SetTarget(_lockOnTarget);
                        _lockOnIndicator.Show();
                    }
                }
                else
                {
                    _isLockOn = false;
                    _lockOnIndicator?.Hide();
                }
            }

            // Consume input (1-frame trigger)
            input.ConsumeLockOn();
        }

        /// <summary>
        /// Automatically breaks lock-on if the target is invalid or out of range.
        /// </summary>
        private void HandleLockOnAutoBreak()
        {
            if (!_isLockOn)
                return;

            if (!IsTargetValid(_lockOnTarget))
            {
                _isLockOn = false;
                _lockOnIndicator?.Hide();
            }
        }

        /// <summary>
        /// Searches around the player for the best lock-on candidate:
        /// - Inside lockOnRadius
        /// - On lockOnLayerMask
        /// - Within max LockAngle from camera forward
        /// Priority:
        /// - Smaller angle (more in front of the camera)
        /// - Closer distance
        /// </summary>
        private Transform FindBestLockOnTarget()
        {
            Vector3 origin = target.position;

            Collider[] hits = Physics.OverlapSphere(origin, lockOnRadius, lockOnLayerMask);
            if (hits.Length == 0)
                return null;

            Transform best = null;
            float bestScore = float.PositiveInfinity;

            Vector3 pivotPos = pivot.position;
            Vector3 camForward = pivot.forward;
            camForward.y = 0f;
            camForward.Normalize();

            foreach (var hit in hits)
            {
                Transform candidate = hit.transform;

                if (!IsValidCandidate(candidate))
                    continue;

                Vector3 toCandidate = candidate.position - pivotPos;
                float distanceSqr = toCandidate.sqrMagnitude;
                if (distanceSqr < 0.0001f)
                    continue;

                Vector3 flatToCandidate = toCandidate;
                flatToCandidate.y = 0f;
                if (flatToCandidate.sqrMagnitude < 0.0001f)
                    continue;

                float angle = Vector3.Angle(camForward, flatToCandidate.normalized);
                if (angle > _maxLockAngle)
                    continue;

                float distance = Mathf.Sqrt(distanceSqr);

                // Priority score: lower is better
                float score = angle * angleWeight + distance * distanceWeight;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        /// <summary>
        /// Checks if a candidate enemy is a valid lock-on target:
        /// - Has an enabled collider
        /// - Within lock-on angle
        /// - Not behind walls (line of sight)
        /// </summary>
        private bool IsValidCandidate(Transform candidate)
        {
            if (candidate == null)
                return false;

            Collider col = candidate.GetComponent<Collider>();
            if (col == null || !col.enabled)
                return false;

            if (!IsTargetWithinLockAngle(candidate))
                return false;

            // Simple line-of-sight check
            Vector3 eye = pivot.position;
            Vector3 targetPos = candidate.position + Vector3.up * 1.5f;

            if (Physics.Linecast(eye, targetPos, out RaycastHit hit))
            {
                if (hit.transform != candidate)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the target is within acceptable lock-on angle
        /// relative to the camera pivotv (XZ plane).
        /// </summary>
        private bool IsTargetWithinLockAngle(Transform targetTransform)
        {
            if (targetTransform == null || pivot == null)
                return false;

            Vector3 camForward = pivot.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 toTarget = targetTransform.position - pivot.position;
            toTarget.y = 0f;
            toTarget.Normalize();

            float angle = Vector3.Angle(camForward, toTarget);
            return angle <= _maxLockAngle;
        }

        #endregion

        #region Target Switching

        /// <summary>
        /// Handles switching between targets while lock-on is active.
        /// Uses an axis input (e.g. gamepad right stick x).
        /// </summary>
        private void HandleTargetSwitch()
        {
            // Cooldown
            if (_switchTimer > 0f)
                _switchTimer -= Time.deltaTime;

            if (!_isLockOn)
                return;

            if (_switchTimer > 0f)
                return;

            float x = input.TargetSwitchInput;

            // Deadzone
            if (Mathf.Abs(x) < 0.7f)
                return;

            Transform next = FindSwitchTarget(x);

            if (next != null && IsTargetValid(next))
            {
                _lockOnTarget = next;

                // Update indicator
                if (_lockOnIndicator != null)
                {
                    _lockOnIndicator.SetTarget(next);
                    _lockOnIndicator.Show();
                }
            }

            _switchTimer = _switchCooldown;
        }

        /// <summary>
        /// Finds the best target to switch to, based on input direction:
        /// - direction < 0: left side
        /// - direction > 0: right side
        /// Chooses the closest (smallest angle) candidate on that side.
        /// </summary>
        private Transform FindSwitchTarget(float direction)
        {
            if (pivot == null || _lockOnTarget == null)
                return null;

            Collider[] hits = Physics.OverlapSphere(target.position, lockOnRadius, lockOnLayerMask);
            if (hits.Length == 0)
                return null;

            Transform best = null;
            float bestAngle = float.MaxValue;

            Vector3 camForward = pivot.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = pivot.right;
            camRight.y = 0f;
            camRight.Normalize();

            foreach (var hit in hits)
            {
                Transform enemy = hit.transform;

                if (enemy == _lockOnTarget)
                    continue;

                if (!IsValidCandidate(enemy))
                    continue;

                Vector3 toEnemy = enemy.position - pivot.position;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude < 0.0001f)
                    continue;

                Vector3 dirNorm = toEnemy.normalized;

                // dot with camera right -> side detection
                float dot = Vector3.Dot(camRight, dirNorm);

                // Left side: dot < 0, right side: dot > 0
                if (direction < 0f && dot >= 0f)
                    continue;
                if (direction > 0f && dot <= 0f)
                    continue;

                float angle = Vector3.Angle(camForward, dirNorm);

                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    best = enemy;
                }
            }

            return best;
        }

        #endregion

        #region Follow & Rotation

        /// <summary>
        /// Smoothly moves camera rig to follow the target.
        /// While locked on, the follow position is biased between the player and the lock-on target.
        /// </summary>
        private void HandleFollow()
        {
            Vector3 followPos = target.position;

            if (_isLockOn && _lockOnTarget != null)
            {
                float dist = Vector3.Distance(target.position, _lockOnTarget.position);

                float t = Mathf.InverseLerp(0f, _maxBiasDistance, dist);
                float dynamicBias = Mathf.Lerp(_maxCentreBias, _minCentreBias, t);

                followPos = Vector3.Lerp(
                    target.position,
                    _lockOnTarget.position,
                    dynamicBias);
            }

            transform.position = Vector3.Lerp(
                transform.position,
                followPos,
                followSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Free-look rotation using Look input (mouse / right stick).
        /// </summary>
        private void HandleFreeLookRotation()
        {
            Vector2 look = input.LookInput;

            if (look.sqrMagnitude > 0.0001f)
            {
                float deltaX = look.x;
                float deltaY = look.y * (invertY ? 1f : -1f);

                _yaw += deltaX * rotationSpeed * Time.deltaTime;
                _pitch += deltaY * rotationSpeed * Time.deltaTime;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        /// <summary>
        /// Lock-on rotation:
        /// - Smoothly rotates the player to face the target (XZ plane)
        /// - Rotates the pivot so the camera actually looks at the target
        /// </summary>
        private void HandleLockOnRotation()
        {
            if (_lockOnTarget == null)
                return;

            // 1) Rotate the player on the XZ plane towards the target
            Vector3 flatToTarget = _lockOnTarget.position - target.position;
            flatToTarget.y = 0f;

            if (flatToTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion playerRot = Quaternion.LookRotation(flatToTarget.normalized, Vector3.up);
                target.rotation = Quaternion.Slerp(
                    target.rotation,
                    playerRot,
                    _lockOnRotateSpeed * Time.deltaTime);
            }

            // 2) Rotate camera pivot so it actually looks at the target
            Vector3 camToTarget = _lockOnTarget.position - pivot.position;
            if (camToTarget.sqrMagnitude < 0.0001f)
                return;

            Quaternion desiredCamRot = Quaternion.LookRotation(camToTarget.normalized, Vector3.up);
            Vector3 desiredEuler = desiredCamRot.eulerAngles;

            float desiredYaw = desiredEuler.y;
            float desiredPitch = desiredEuler.x;

            // Converts pitch from 0..360 to -180..180
            if (desiredPitch > 180f)
                desiredPitch -= 360f;

            _yaw = Mathf.LerpAngle(_yaw, desiredYaw, _lockOnRotateSpeed * Time.deltaTime);
            _pitch = Mathf.LerpAngle(_pitch, desiredPitch, _lockOnRotateSpeed * Time.deltaTime);
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Allows setting lock-on target from other systems (e.g. AI, targeting manager).
        /// If lock-on is already active, this will update the indicator.
        /// </summary>
        public void SetLockOnTarget(Transform newTarget)
        {
            _lockOnTarget = newTarget;

            if (!_isLockOn)
                return;

            if (_lockOnTarget != null && IsTargetWithinLockAngle(_lockOnTarget))
            {
                if (_lockOnIndicator != null)
                {
                    _lockOnIndicator.SetTarget(_lockOnTarget);
                    _lockOnIndicator.Show();
                }
            }
            else
            {
                _isLockOn = false;
                _lockOnIndicator?.Hide();
            }
        }

        #endregion
    }
}