using System.Collections.Generic;
using UnityEngine;

public class LevelDecorator : MonoBehaviour {
    //**PROPERTIES**
    [Range(0.0f, 1.0f)]
    [SerializeField] float floorDecorationFrequency;
    [Range(0.0f, 1.0f)]
    public float wallDecorationFrequency;
    //
    [SerializeField] GameObject castleWall5x5WindowPrefab;
    [SerializeField] GameObject castleWall5x5DrainPrefab;
    [SerializeField] GameObject castleWall5x5DoorPrefab;

    //**UTILITY METHODS**
    public void PlacePlayerDoorDecoration(Vector3 playerLocationIn, bool debugIn = false) {
        string debugOutput = "===============================\nPlacing door decoration:\n";
        debugOutput += $"Player Pos: {playerLocationIn}\n";

        Collider[] hits = Physics.OverlapSphere(playerLocationIn, 15f);
        GameObject closestGO = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in hits) {
            GameObject go = collider.gameObject;

            if (go.name == "CastleWall_5x5(Clone)") {
                float distance = Vector3.Distance(playerLocationIn, go.transform.position);
                debugOutput += $"Nearest GameObject: {go.name}\nDistance: {distance}\n---------------------\n";
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestGO = go;
                }
            }
        }

        debugOutput += $"Closest Distance: {closestDistance}\n";
        debugOutput += $"Closest wall: {closestGO.transform.position}\n===============================\n";

        GameObject door = Instantiate(castleWall5x5DoorPrefab, closestGO.transform.parent);
        door.name = "door";
        door.transform.position = closestGO.transform.localPosition;
        door.transform.rotation = Quaternion.Euler(90, 90, 0);
        //closestGO.SetActive(false);
        DestroyImmediate(closestGO);

        if (debugIn) {
            Debug.Log(debugOutput);
        }

    }
    //
    public void DecorateLevelWalls(List<GameObject> fullWallsIn, Transform roomParentIn) {
        foreach (GameObject wall in fullWallsIn) {
            float randomFloat = Random.value;
            if (randomFloat < wallDecorationFrequency) {

                GameObject prefab = Random.Range(0, 2) == 0 ? castleWall5x5DrainPrefab : castleWall5x5WindowPrefab;
                int ogChildIndex = wall.transform.GetSiblingIndex();
                GameObject decorativeWallPrefab = Instantiate(prefab, wall.transform.position, wall.transform.rotation, roomParentIn);
                DestroyImmediate(wall);
                decorativeWallPrefab.transform.SetSiblingIndex(ogChildIndex);
            }
        }
    }
    //
    public List<Vector3> GenerateFloorItemLocations(List<Vector3> platformLocationsIn) {
        List<Vector3> itemLocationsOut = new List<Vector3>();

        foreach (Vector3 platformLocation in platformLocationsIn) {

            float roll = Random.value;
            if (roll < floorDecorationFrequency) {
                itemLocationsOut.Add(platformLocation);
            }
        }
        return itemLocationsOut;
    }
    //
    public void SpawnFloorDecorationItems(List<Vector3> itemLocationsIn, Transform decorationParentIn) {

        //Helpers
        LevelDecorations levelDecorations = GameObject.Find("GameManager").GetComponent<GameManager>().LevelDecorations;

        foreach (Vector3 spawnPos in itemLocationsIn) {
            //get random item from SO
            GameObject decoration = levelDecorations.DecorationPrefabs[Random.Range(0, levelDecorations.DecorationPrefabs.Count)];

            float decorZPos = Random.Range(-2.5f, 2.5f) + spawnPos.z;
            Vector3 decorSpawnPos = new Vector3(spawnPos.x + .05f, Random.Range(0, 2) == 0 ? spawnPos.y + 1f : spawnPos.y + 4.75f, decorZPos);

            decoration = Instantiate(decoration, decorSpawnPos, Quaternion.Euler(Random.Range(0, 360), 0, -90), decorationParentIn);
        }


    }
}
