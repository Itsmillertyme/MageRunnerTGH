using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class LevelEnemies : ScriptableObject {
    public List<GameObject> mobEnemyPrefabs = new List<GameObject>();

    public GameObject bossPrefab;

}