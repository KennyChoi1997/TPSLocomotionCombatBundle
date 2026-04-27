using System.Collections;
using UnityEngine;

namespace TPSCombatSystem.Demo
{
    public enum DemoTargetRangeBand
    {
        Close,
        Mid,
        Long
    }

    /// <summary>
    /// Manages one independent target spawn group for a single weapon branch in the demo scene.
    /// 
    /// Responsibilities:
    /// 1) Keep exactly one active target in this branch.
    /// 1) Spawn one target on start.
    /// 3) Respawn only when this branch's current target dies.
    /// </summary>
    public sealed class DemoTargetSpawnGroup : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Spawn Point")]
        [SerializeField] private DemoTargetSpawnPoint[] closeSpawnPoints;
        [SerializeField] private DemoTargetSpawnPoint[] midSpawnPoints;
        [SerializeField] private DemoTargetSpawnPoint[] longSpawnPoints;

        [Header("Target")]
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private float respawnDelay = 0.75f;

        [Header("Range Selection Weight")]
        [SerializeField, Min(0)] private int closeWeight = 60;
        [SerializeField, Min(0)] private int midWeight = 30;
        [SerializeField, Min(0)] private int longWeight = 10;

        [Header("Options")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool avoidImmediateRepeatSpawn = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        #endregion

        #region State

        private DemoSpawnedTarget currentTarget;
        private DemoTargetRangeBand lastRangeBand = DemoTargetRangeBand.Mid;
        private int lastSpawnIndex = -1;
        private Coroutine respawnRoutine;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnNextTarget();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Called by a spawned target when it dies.
        /// </summary>
        public void NotifyTargetDied(DemoSpawnedTarget target)
        {
            // Ignore notifications from anything other than this branch's current target.
            if (target != currentTarget)
            {
                return;
            }

            Log($"[SpawnGroup] Current target died in branch: {name}");

            currentTarget = null;

            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
            }

            respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        [ContextMenu("Spawn Next Target")]
        public void SpawnNextTarget()
        {
            // Safety: never spawn a second target while one is still alive in this branch.
            if (currentTarget != null)
            {
                return;
            }

            if (targetPrefab == null)
            {
                Debug.LogWarning("[SpawnGroup] Target prefab is missing.", this);
                return;
            }

            DemoTargetRangeBand rangeBand = ChooseRangeBand();
            DemoTargetSpawnPoint[] candidatePoints = GetSpawnPoints(rangeBand);

            if (candidatePoints == null || candidatePoints.Length == 0)
            {
                Debug.LogWarning("[SpawnGroup] No spawn points Found for range band: {rangeBand}.", this);
                return;
            }

            int spawnIndex = GetNextSpawnIndex(candidatePoints);
            if (spawnIndex < 0)
            {
                Debug.LogWarning("[SpawnGroup] Failed to resolve a valid spawn index.", this);
                return;
            }

            Transform spawnTransform = candidatePoints[spawnIndex].transform;

            GameObject instance = Instantiate(
                targetPrefab,
                spawnTransform.position,
                spawnTransform.rotation
                );

            DemoSpawnedTarget spawnedTarget = instance.GetComponent<DemoSpawnedTarget>();
            if (spawnedTarget == null)
            {
                spawnedTarget = instance.AddComponent<DemoSpawnedTarget>();
            }

            spawnedTarget.Initialize(this);

            currentTarget = spawnedTarget;
            lastRangeBand = rangeBand;
            lastSpawnIndex = spawnIndex;

            Log($"[SpawnGroup] Spawned target in {name} / {rangeBand} / index {name}");
        }

        #endregion

        #region Internal Flow

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            respawnRoutine = null;
            SpawnNextTarget();
        }

        private DemoTargetRangeBand ChooseRangeBand()
        {
            int totalWeight = closeWeight + midWeight + longWeight;

            if (totalWeight <= 0)
            {
                return DemoTargetRangeBand.Mid;
            }

            int roll = Random.Range(0, totalWeight);

            if (roll < closeWeight)
            {
                return DemoTargetRangeBand.Close;
            }

            roll -= closeWeight;

            if (roll < midWeight)
            {
                return DemoTargetRangeBand.Mid;
            }

            return DemoTargetRangeBand.Long;
        }

        private DemoTargetSpawnPoint[] GetSpawnPoints(DemoTargetRangeBand rangeBand)
        {
            return rangeBand switch
            {
                DemoTargetRangeBand.Close => closeSpawnPoints,
                DemoTargetRangeBand.Mid => midSpawnPoints,
                DemoTargetRangeBand.Long => longSpawnPoints,
                _ => midSpawnPoints
            };
        }

        private int GetNextSpawnIndex(DemoTargetSpawnPoint[] points)
        {
            if (points == null || points.Length == 0)
            {
                return -1;
            }

            if (points.Length == 1)
            {
                return 0;
            }

            int index;
            int safety = 0;

            do
            {
                index = Random.Range(0, points.Length);
                safety++;
            }
            while (avoidImmediateRepeatSpawn &&
            points == GetSpawnPoints(lastRangeBand) &&
            index == lastSpawnIndex &&
            safety < 16);

            return index;
        }

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message, this);
            }
        }

        #endregion
    }
}