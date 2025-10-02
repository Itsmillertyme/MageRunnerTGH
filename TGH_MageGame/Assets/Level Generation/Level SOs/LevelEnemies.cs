using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spawner/Level Enemies")]
public class LevelEnemies : ScriptableObject {
    [System.Serializable]
    public struct WeightedMob {
        public GameObject prefab;
        [Range(1, 100)]
        public int weight; // Higher = more common
    }

    [Header("Mobs with weights")]
    public List<WeightedMob> mobEnemies = new List<WeightedMob>();

    [Header("Boss")]
    public GameObject bossPrefab;

    public GameObject GetRandomMob() {
        if (mobEnemies.Count == 0) return null;

        int totalWeight = 0;
        foreach (var mob in mobEnemies)
            totalWeight += mob.weight;

        int randomValue = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var mob in mobEnemies) {
            cumulative += mob.weight;
            if (randomValue < cumulative)
                return mob.prefab;
        }

        return null; // should never hit
    }
}
