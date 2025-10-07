using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerPG2 : MonoBehaviour {
    [Header("Enemy Spawner Settings")]
    [SerializeField] LevelEnemies enemyData; // ScriptableObject with enemies

    // Called by LevelGenerator after generation
    public void SpawnMobEnemies(Dictionary<Vector2Int, RoomInstance> placedRooms, Transform enemyParent, bool debugMode = false) {
        if (enemyData == null || enemyData.mobEnemies.Count == 0) {
            Debug.LogWarning("[EnemySpawner] No enemy Scritable Object or no mob prefabs assigned.");
            return;
        }

        foreach (KeyValuePair<Vector2Int, RoomInstance> entry in placedRooms) {
            RoomInstance roomInstance = entry.Value;
            RoomData roomData = roomInstance.RoomData;

            if (roomData.EnemySpawns == null || roomData.EnemySpawns.Count == 0 || roomData.RoomType == RoomType.Special) continue;

            foreach (Transform spawnPoint in roomData.EnemySpawns) {
                if (spawnPoint == null) continue;

                // Get random enemy from the SO
                GameObject enemyPrefab = enemyData.GetRandomMob();

                if (enemyPrefab == null) {
                    if (debugMode) Debug.LogWarning("[EnemySpawner] No enemy prefab returned from LevelEnemies SO.");
                    continue;

                }

                //set up random facing rotation
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 2) == 1 ? 90 : -90, 0); // 90 degrees right or left

                GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, randomRotation);
                enemyInstance.name = $"{enemyPrefab.name}{enemyInstance.GetInstanceID()}";
                enemyInstance.transform.parent = enemyParent;

                //Initialize AI
                //EnemyPatrol enemyAI = enemyInstance.GetComponent<EnemyPatrol>();
                //enemyAI.Initialize(roomData, debugMode);
                IBehave[] behaviors = enemyInstance.GetComponents<IBehave>();
                foreach (IBehave behavior in behaviors) {
                    behavior.Initialize(roomData, debugMode);
                }

            }
        }

        if (debugMode) Debug.Log("[EnemySpawner] Finished spawning enemies.");
    }

    public void SpawnBossEnemy(RoomInstance bossRoom, Transform enemyParent, bool debugMode = false) {
        if (enemyData == null || enemyData.bossPrefab == null) {
            if (debugMode) Debug.LogWarning("[EnemySpawner] No enemy Scritable Object or no Boss prefabs assigned.");
            return;
        }

        RoomData roomData = bossRoom.RoomData;
        if (roomData.EnemySpawns == null || roomData.EnemySpawns.Count == 0) {
            if (debugMode) Debug.LogWarning("[EnemySpawner] Boss room has no enemy spawn points.");
            return;
        }

        GameObject bossInstance = Instantiate(enemyData.bossPrefab, roomData.EnemySpawns[0].position, Quaternion.identity);
        bossInstance.name = $"**{enemyData.bossPrefab.name}{bossInstance.GetInstanceID()}**";
        bossInstance.transform.parent = enemyParent;

        IBehave[] behaviors = bossInstance.GetComponents<IBehave>();
        foreach (IBehave behavior in behaviors) {
            behavior.Initialize(roomData, debugMode);
        }
    }

}
