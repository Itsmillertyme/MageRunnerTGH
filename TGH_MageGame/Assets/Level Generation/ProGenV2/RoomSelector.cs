using System.Collections.Generic;
using UnityEngine;

public class RoomSelector {

    //**FIELDS**
    LevelData levelData;
    float deadEndChance;

    //**CONTRUCTORS**
    public RoomSelector(LevelData levelDataIn, float deadInChangeIn = 0.10f) {
        levelData = levelDataIn;
        deadEndChance = Mathf.Clamp01(deadInChangeIn);
    }

    //**UTILITY METHODS**
    public GameObject GetValidRoomPrefab(PortalData parentPortalIn) {

        //get needed porttal direction for new room
        PortalDirection needEntranceDirection = parentPortalIn.GetOppositeDirection();

        //Build candidate lists
        List<GameObject> candidateRooms = new List<GameObject>();
        candidateRooms.AddRange(levelData.Straights);
        candidateRooms.AddRange(levelData.Corners);
        candidateRooms.AddRange(levelData.Junctions);

        //Filter candidates
        for (int i = candidateRooms.Count - 1; i >= 0; i--) {
            GameObject prefab = candidateRooms[i];
            RoomData roomData = candidateRooms[i].GetComponent<RoomData>();

            bool hasPortalInDirection = roomData.HasPortalInDirection(needEntranceDirection);

            if (roomData == null || !hasPortalInDirection) {
                candidateRooms.RemoveAt(i);
            }
        }

        //ROOM SELECTION
        //Dead end - % chance
        if (levelData.DeadEnds.Count > 0 && Random.value < deadEndChance) {
            //spawn dead end room
            return levelData.DeadEnds[Random.Range(0, levelData.DeadEnds.Count)];
        }
        else if (candidateRooms.Count > 0) {
            return candidateRooms[Random.Range(0, candidateRooms.Count)];
        }
        else {
            //No rooms available at all
            Debug.LogWarning("No valid room prefabs available in LevelData!");
            return null;
        }
    }

}
