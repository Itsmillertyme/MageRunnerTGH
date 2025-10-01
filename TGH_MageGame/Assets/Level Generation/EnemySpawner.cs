using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] Transform waypoint;

    public List<GameObject> SpawnMobEnemies(PathNode roomIn, Transform enemiesParentIn, bool debugMode = false, bool startRoom = false) {
        //Helpers
        LevelEnemies levelEnemies = GameObject.Find("GameManager").GetComponent<GameManager>().LevelEnemies;
        List<Vector3> spawnLocations = new List<Vector3>();
        List<GameObject> enemiesOut = new List<GameObject>();
        int roomArea = roomIn.RoomDimensions.x * roomIn.RoomDimensions.y;
        int numEnemySpawns = 0;

        if (roomArea < 250) {
            numEnemySpawns = 1;
        }
        else if (roomArea < 500) {
            numEnemySpawns = 2;
        }
        else if (roomArea < 750) {
            numEnemySpawns = 3;
        }
        else if (roomArea < 1000) {
            numEnemySpawns = 4;
        }
        else if (roomArea < 1250) {
            numEnemySpawns = 5;
        }
        else if (roomArea < 1500) {
            numEnemySpawns = 6;
        }
        else {
            numEnemySpawns = 7;
        }

        //get random enemy spawn points
        List<int> xLevels = new List<int>();
        for (int i = 0; i < numEnemySpawns; i++) {
            //find x values(Start room ignores bottom floor)
            for (int j = startRoom ? roomIn.RoomTopLeftCorner.x + 4 : roomIn.RoomTopLeftCorner.x; j < roomIn.RoomTopLeftCorner.x + roomIn.RoomDimensions.x - 3; j += 5) {
                xLevels.Add(j);
            }

            bool locationFound = false;
            float randX = 0;
            float randZ = 0;
            int counter = 0; //To stop runaway loops

            while (!locationFound && counter < 100) {
                counter++;

                randX = xLevels[Random.Range(0, xLevels.Count)] + 0.15f;
                randZ = Random.Range(roomIn.RoomTopLeftCorner.y - roomIn.RoomDimensions.y + 2, roomIn.RoomTopLeftCorner.y - 2);

                //test if on platform
                Vector3 newSpawnPos = new Vector3(randX, 2.5f, randZ);
                Ray platformCheckRay = new Ray(newSpawnPos, Vector3.left);

                if (debugMode) {
                    Debug.DrawLine(newSpawnPos, newSpawnPos + Vector3.left * 2f, Color.red, 60);
                }

                if (Physics.Raycast(platformCheckRay, 2f)) {
                    locationFound = true;
                    if (debugMode) {
                        Debug.DrawLine(newSpawnPos, newSpawnPos + Vector3.left * 2f, Color.yellow, 60);
                    }
                }

                //*Ensure spacing*
                //Check against existing spawn locations
                foreach (Vector3 spawnLocation in spawnLocations) {

                    if (Vector3.Distance(newSpawnPos, spawnLocation) < 15) {
                        locationFound = false;
                    }
                }
                //Check for player
                if (Vector3.Distance(newSpawnPos, GameObject.FindWithTag("Player").transform.position) < 20) { //20 for testing
                    locationFound = false;
                }




                if (debugMode) {
                    Debug.Log($"Mob spawn location found: {locationFound}");
                }

            }

            if (locationFound) {
                spawnLocations.Add(new Vector3(randX, 2.5f, randZ));
            }
        }

        foreach (Vector3 spawnPos in spawnLocations) {
            //get random enemy
            GameObject enemy = levelEnemies.GetRandomMob();

            enemy = Instantiate(enemy, spawnPos, Quaternion.Euler(0, 0, -90), enemiesParentIn);

            enemiesOut.Add(enemy);
        }

        return enemiesOut;
    }

    public GameObject SpawnBoss(PathNode bossRoomPathNode, bool isOnLeft, Transform enemiesParentIn, bool debugMode = false) {
        Vector3 spawnPos = Vector3.zero;
        float xRot = 0;
        GameObject bossPrefab = GameObject.Find("GameManager").GetComponent<GameManager>().LevelEnemies.bossPrefab;

        if (isOnLeft) {
            //Spawn on "left" side of room
            spawnPos = new Vector3(bossRoomPathNode.RoomTopLeftCorner.x + .1f, 2.5f, bossRoomPathNode.RoomTopLeftCorner.y - 2);
            xRot = 180;
        }
        else {
            //spawn on "right" side of room
            spawnPos = new Vector3(bossRoomPathNode.RoomTopLeftCorner.x + .1f, 2.5f, bossRoomPathNode.RoomTopLeftCorner.y - bossRoomPathNode.RoomDimensions.y + 2);
        }

        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.Euler(xRot, 0, -90), enemiesParentIn);

        boss.transform.SetSiblingIndex(0);

        return boss;

        //place boss
        //bossPrefab.transform.position = spawnPos;
        ////rotates boss around origin to account for level rotation
        ////bossPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
        ////bossPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
        ////if (!dungeonFlatMode) {
        ////    //rotates boss around origin to account for level rotation
        ////    bossPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
        ////    bossPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
        ////}

        //bossPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
        //nudge
        //bossPrefab.transform.position = new Vector3(bossPrefab.transform.position.x, spawnPos.x + 3f, bossPrefab.transform.position.z + 1);
    }



}
