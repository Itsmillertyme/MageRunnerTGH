using System.Collections.Generic;
using UnityEngine;

public class DecorationItemSpawner : MonoBehaviour {

    public void SpawnDecorationItems(List<Vector3> platformSpawnPositionsIn, Transform decorationParentIn) {

        //Helpers
        LevelDecorations levelDecorations = GameObject.Find("GameManager").GetComponent<GameManager>().LevelDecorations;

        foreach (Vector3 spawnPos in platformSpawnPositionsIn) {
            //get random item from SO
            GameObject decoration = levelDecorations.DecorationPrefabs[Random.Range(0, levelDecorations.DecorationPrefabs.Count)];

            float decorZPos = Random.Range(-2.5f, 2.5f) + spawnPos.z;
            Vector3 decorSpawnPos = new Vector3(spawnPos.x + .05f, Random.Range(0, 2) == 0 ? spawnPos.y + 1f : spawnPos.y + 4.75f, decorZPos);

            decoration = Instantiate(decoration, decorSpawnPos, Quaternion.Euler(Random.Range(0, 360), 0, -90), decorationParentIn);
        }


    }
}
