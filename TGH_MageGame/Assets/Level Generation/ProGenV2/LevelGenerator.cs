using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshLinkBuilder))]
[RequireComponent(typeof(MaskGeneratorPG2))]
[RequireComponent(typeof(EnemySpawnerPG2))]
public class LevelGenerator : MonoBehaviour {
    #region Variables
    [Header("Level Data")]
    [SerializeField] LevelData levelData;
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] GameObject playerPrefab;

    [Header("Level Settings")]
    [SerializeField] StartRoomPosition startRoomPosition = StartRoomPosition.CENTER; //default to center
    [Range(0f, 1f)]
    [SerializeField] float deadEndChance;
    [SerializeField] bool debugMode;
    [SerializeField] bool omitMask;

    [Header("Parent References")]
    [SerializeField] Transform levelParent;
    [SerializeField] Transform maskParent;
    [SerializeField] Transform enemyParent;

    //**FIELDS**
    RoomSelector roomSelector;
    Dictionary<Vector2Int, RoomInstance> placedRooms;
    Queue<PortalTask> openPortals;
    PlayerController player;
    #endregion

    #region Unity Methods
    //**UNITY METHODS**
    private void Awake() {
        Initialize();
    }
    //
    private void Start() {
        GenerateLevel();
    }
    #endregion

    #region Generation
    //**MAIN GENERATION METHOD**
    public void GenerateLevel() {
        Debug.Log("========== [LevelGen] START GENERATION RUN ==========");
        ClearLevel();
        Initialize();

        PlaceStartRoom();

        //Start building from open portals
        while (openPortals.Count > 0) {
            PortalTask currentPortalTask = openPortals.Dequeue();
            TryExpandFromPortal(currentPortalTask);
        }

        FinalizeLevel();
    }

    //**GENERATION METHODS**
    void Initialize() {
        roomSelector = new RoomSelector(levelData, deadEndChance);
        placedRooms = new Dictionary<Vector2Int, RoomInstance>();
        openPortals = new Queue<PortalTask>();
    }

    //finish level gen
    void FinalizeLevel() {
        //Close any remaining portals
        Debug.Log($"scanning {placedRooms.Count} placed rooms");
        foreach (KeyValuePair<Vector2Int, RoomInstance> placedRoom in placedRooms) {
            RoomInstance room = placedRoom.Value;

            Debug.Log($"Closing {room.GetActiveUnconnectedPortals(debugMode).Count} unconnected portals");
            foreach (PortalData portal in room.GetActiveUnconnectedPortals(debugMode)) {
                portal.ClosePortal();
            }
        }

        //Bake navmesh
        if (navMeshSurface != null) {

            if (debugMode) {
                Debug.Log("Building NavMesh...");
            }
            navMeshSurface.BuildNavMesh();
        }

        NavMeshLinkBuilder linkBuilder = GetComponent<NavMeshLinkBuilder>();
        linkBuilder.BuildAll();

        //Create mask
        if (!omitMask) {
            MaskGeneratorPG2 maskGen = GetComponent<MaskGeneratorPG2>();
            List<RoomData> rooms = placedRooms.Values.Select(r => r.RoomData).ToList();
            maskGen.GenerateMaskMesh(rooms, levelData.MaxWidth, levelData.MaxHeight);
        }

        //Spawn Enemies
        EnemySpawnerPG2 enemySpawner = GetComponent<EnemySpawnerPG2>();
        enemySpawner.SpawnEnemies(placedRooms, enemyParent);

    }

    //Place start room at designated position
    void PlaceStartRoom() {
        GameObject startRoomPrefab = Instantiate(levelData.StartRoom, levelParent);
        RoomData startRoomData = startRoomPrefab.GetComponent<RoomData>();
        startRoomData.InitializePortals();

        if (debugMode) {
            if (startRoomData == null) {
                Debug.LogError("Start room prefab does not have RoomData on its root!");
            }
        }


        Vector2Int gridPosition = Vector2Int.zero;
        switch (startRoomPosition) {
            case StartRoomPosition.TOP:
                gridPosition = new Vector2Int((levelData.MaxWidth / 2) + (startRoomData.Width / 2), levelData.MaxHeight - startRoomData.Height);
                break;

            case StartRoomPosition.TOP_RIGHT:
                gridPosition = new Vector2Int(levelData.MaxWidth, levelData.MaxHeight - startRoomData.Height);
                break;

            case StartRoomPosition.RIGHT:
                gridPosition = new Vector2Int(levelData.MaxWidth, (levelData.MaxHeight / 2) - (startRoomData.Height / 2));
                break;

            case StartRoomPosition.BOTTOM_RIGHT:
                gridPosition = new Vector2Int(levelData.MaxWidth, 0);
                break;

            case StartRoomPosition.BOTTOM:
                gridPosition = new Vector2Int((levelData.MaxWidth / 2) + (startRoomData.Width / 2), 0);
                break;

            case StartRoomPosition.BOTTOM_LEFT:
                gridPosition = new Vector2Int(startRoomData.Width, 0);
                break;

            case StartRoomPosition.LEFT:
                gridPosition = new Vector2Int(startRoomData.Width, (levelData.MaxHeight / 2) - (startRoomData.Height / 2));
                break;

            case StartRoomPosition.TOP_LEFT:
                gridPosition = new Vector2Int(startRoomData.Width, levelData.MaxHeight - startRoomData.Height);
                break;

            default: // CENTER
                gridPosition = new Vector2Int((levelData.MaxWidth / 2) + (startRoomData.Width / 2), (levelData.MaxHeight / 2) - (startRoomData.Height / 2));
                break;
        }
        //Place room
        startRoomPrefab.transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);

        //Track placed room
        RoomInstance startRoomInstance = new RoomInstance(startRoomData, gridPosition);
        placedRooms.Add(gridPosition, startRoomInstance);

        //Enqueue portals
        if (debugMode) {
            var portals = startRoomInstance.GetActiveUnconnectedPortals(debugMode).ToList();
            Debug.Log($"Starter room portals found: {portals.Count}");
        }
        foreach (PortalData portal in startRoomInstance.GetActiveUnconnectedPortals(debugMode)) {
            if (debugMode) {
                Debug.Log($"Enqueueing portal {portal.PortalDirection} (Active={portal.IsActive}, Connected={portal.IsConnected})");
            }
            openPortals.Enqueue(new PortalTask(startRoomInstance, portal));
        }

        //Place Player
        GameObject playerInstance = Instantiate(playerPrefab, startRoomData.PlayerSpawn.position, Quaternion.identity, levelParent.parent);
        player = playerInstance.GetComponent<PlayerController>();
        if (debugMode) Debug.Log($"Player instantiated in scene at {startRoomData.PlayerSpawn.position}");

    }

    // Expand level from a source portal task
    private void TryExpandFromPortal(PortalTask portalTaskIn) {
        // --- Source references ---
        RoomInstance sourceRoom = portalTaskIn.SourceRoom;
        PortalData sourcePortal = portalTaskIn.SourcePortal;

        if (debugMode) {
            Debug.Log($"[Expand] Trying to expand from {sourcePortal.PortalDirection} portal of room {sourceRoom.RoomData.name} at {sourceRoom.GridPosition}");
        }

        // --- Choose connector list based on direction ---
        List<GameObject> connectorList =
            (sourcePortal.PortalDirection == PortalDirection.LEFT || sourcePortal.PortalDirection == PortalDirection.RIGHT)
                ? levelData.HorizontalConnectors
                : levelData.VerticalConnectors;

        if (connectorList == null || connectorList.Count == 0) {
            if (debugMode) Debug.LogWarning("[Expand] No connector prefabs available for this direction. Closing source portal.");
            sourcePortal.ClosePortal();
            return;
        }

        GameObject connectorPrefab = connectorList[Random.Range(0, connectorList.Count)];

        // --- Instantiate connector first so portals are initialized/active on the instance ---
        GameObject connectorInstanceObj = Instantiate(connectorPrefab, Vector3.zero, Quaternion.identity, levelParent);
        RoomData connectorRoomData = connectorInstanceObj.GetComponent<RoomData>();
        connectorRoomData.InitializePortals();

        // Find entrance/exit on the INSTANCE (not the prefab), so IsActive is respected correctly
        PortalData connectorEntrance = connectorRoomData.GetPortalInDirection(sourcePortal.GetOppositeDirection());
        PortalData connectorExit = connectorRoomData.GetPortalInDirection(sourcePortal.PortalDirection);

        if (connectorEntrance == null || connectorExit == null) {
            if (debugMode) {
                Debug.LogWarning($"[Expand] Connector instance {connectorInstanceObj.name} missing entrance/exit for {sourcePortal.PortalDirection}. Closing source portal.");
            }
            sourcePortal.ClosePortal();
            Destroy(connectorInstanceObj);
            return;
        }

        // Compute connector placement from source-portal anchor -> connector entrance local offset
        Vector3 sourcePortalWorldPos = sourcePortal.GetWorldPosition();
        Vector3 connectorEntranceLocalPos = connectorEntrance.transform.localPosition;
        Vector3 connectorWorldPos = sourcePortalWorldPos - connectorEntranceLocalPos;

        Vector2Int connectorGridPos = new Vector2Int(
            Mathf.RoundToInt(connectorWorldPos.x),
            Mathf.RoundToInt(connectorWorldPos.y)
        );

        // Bounds and occupancy checks BEFORE committing the connector
        if (!IsWithinLevelBounds(connectorGridPos, connectorRoomData)) {
            if (debugMode) Debug.LogWarning($"[Expand] Connector {connectorInstanceObj.name} out of bounds at {connectorGridPos}. Closing source portal.");
            sourcePortal.ClosePortal();
            if (Application.isPlaying) {
                Destroy(connectorInstanceObj);
            }
            else {
                DestroyImmediate(connectorInstanceObj);
            }
            return;
        }

        if (!IsRoomSpaceFree(connectorGridPos, connectorRoomData)) {
            if (debugMode) Debug.LogWarning($"[Expand] Space occupied at {connectorGridPos} for connector {connectorInstanceObj.name}. Closing source portal.");
            sourcePortal.ClosePortal();
            if (Application.isPlaying) {
                Destroy(connectorInstanceObj);
            }
            else {
                DestroyImmediate(connectorInstanceObj);
            }
            return;
        }

        // Commit connector placement + register
        connectorInstanceObj.transform.position = connectorWorldPos;
        RoomInstance connectorInstance = new RoomInstance(connectorRoomData, connectorGridPos);
        placedRooms.Add(connectorGridPos, connectorInstance);

        // Wire source <-> connector
        sourceRoom.ConnectPortals(sourcePortal, connectorEntrance);

        // --- Select next room using the connector's exit on the INSTANCE ---
        GameObject nextRoomPrefab = roomSelector.GetValidRoomPrefab(connectorExit);
        if (nextRoomPrefab == null) {
            if (debugMode) Debug.LogWarning($"[Expand] No valid room prefab for exit {connectorExit.PortalDirection}. Closing connector exit.");
            connectorExit.ClosePortal();
            return;
        }

        // Instantiate next room first so its portals are initialized/active on the instance
        GameObject nextRoomInstanceObj = Instantiate(nextRoomPrefab, Vector3.zero, Quaternion.identity, levelParent);
        RoomData nextRoomData = nextRoomInstanceObj.GetComponent<RoomData>();
        nextRoomData.InitializePortals();

        PortalData nextRoomEntrance = nextRoomData.GetPortalInDirection(connectorExit.GetOppositeDirection());
        if (nextRoomEntrance == null) {
            if (debugMode) {
                Debug.LogWarning($"[Expand] Next room instance {nextRoomInstanceObj.name} missing entrance for {connectorExit.PortalDirection}. Closing connector exit.");
            }
            connectorExit.ClosePortal();
            if (Application.isPlaying) {
                Destroy(nextRoomInstanceObj);
            }
            else {
                DestroyImmediate(nextRoomInstanceObj);
            }
            return;
        }

        // Compute next room placement from connector-exit anchor -> next-room entrance local offset
        Vector3 connectorExitWorldPos = connectorExit.GetWorldPosition();
        Vector3 nextRoomEntranceLocalPos = nextRoomEntrance.transform.localPosition;
        Vector3 nextRoomWorldPos = connectorExitWorldPos - nextRoomEntranceLocalPos;

        Vector2Int nextRoomGridPos = new Vector2Int(
            Mathf.RoundToInt(nextRoomWorldPos.x),
            Mathf.RoundToInt(nextRoomWorldPos.y)
        );

        // Bounds and occupancy checks BEFORE committing the next room
        if (!IsWithinLevelBounds(nextRoomGridPos, nextRoomData)) {
            if (debugMode) Debug.LogWarning($"[Expand] Next room {nextRoomInstanceObj.name} out of bounds at {nextRoomGridPos}. Closing connector exit.");
            connectorExit.ClosePortal();
            if (Application.isPlaying) {
                Destroy(nextRoomInstanceObj);
            }
            else {
                DestroyImmediate(nextRoomInstanceObj);
            }

            return;
        }

        if (!IsRoomSpaceFree(nextRoomGridPos, nextRoomData)) {
            if (debugMode) Debug.LogWarning($"[Expand] Space occupied at {nextRoomGridPos} for next room {nextRoomInstanceObj.name}. Closing connector exit.");
            connectorExit.ClosePortal();
            if (Application.isPlaying) {
                Destroy(nextRoomInstanceObj);
            }
            else {
                DestroyImmediate(nextRoomInstanceObj);
            }
            return;
        }

        // Commit next room placement + register
        nextRoomInstanceObj.transform.position = nextRoomWorldPos;
        RoomInstance placedRoomInstance = new RoomInstance(nextRoomData, nextRoomGridPos);
        placedRooms.Add(nextRoomGridPos, placedRoomInstance);

        // Wire connector <-> next room
        connectorInstance.ConnectPortals(connectorExit, nextRoomEntrance);

        // --- Enqueue new open portals from the newly placed room ---
        List<PortalData> unconnected = placedRoomInstance.GetActiveUnconnectedPortals(debugMode);
        for (int i = 0; i < unconnected.Count; i++) {
            PortalData portal = unconnected[i];
            if (portal != nextRoomEntrance) // safeguard; usually already excluded by "unconnected"
            {
                if (debugMode) Debug.Log($"[Expand] Enqueueing new portal {portal.PortalDirection} from room {nextRoomData.name}");
                openPortals.Enqueue(new PortalTask(placedRoomInstance, portal));
            }
        }
    }

    //Clear level
    public void ClearLevel() {
        //Destroy player
        if (player != null) {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(player.gameObject);
            else
                Destroy(player.gameObject);
#else
            Destroy(player.gameObject);
#endif
        }

        // Destroy all level objects
        if (levelParent != null) {
            var children = new List<GameObject>();
            foreach (Transform child in levelParent) {
                children.Add(child.gameObject);
            }

            foreach (GameObject child in children) {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
                    Destroy(child);
#else
            Destroy(child);
#endif
            }
        }

        //Remove mask
        if (maskParent != null) {
            var children = new List<GameObject>();
            foreach (Transform child in maskParent) {
                children.Add(child.gameObject);
            }

            foreach (GameObject child in children) {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
                    Destroy(child);
#else
            Destroy(child);
#endif
            }
        }

        //Remove enemies
        if (enemyParent != null) {
            var children = new List<GameObject>();
            foreach (Transform child in enemyParent) {
                children.Add(child.gameObject);
            }

            foreach (GameObject child in children) {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
                    Destroy(child);
#else
            Destroy(child);
#endif
            }
        }

        //Remove NavMesh
        NavMesh.RemoveAllNavMeshData();
    }

    #endregion

    #region Utilities
    //**UTILITY METHODS**
    bool IsWithinLevelBounds(Vector2Int positionIn, RoomData roomDataIn) {
        bool leftCheck = positionIn.x - roomDataIn.Width >= 0;
        bool rightCheck = positionIn.x <= levelData.MaxWidth;
        bool bottomCheck = positionIn.y >= 0;
        bool topCheck = positionIn.y + roomDataIn.Height <= levelData.MaxHeight;

        return leftCheck && rightCheck && bottomCheck && topCheck;
    }

    //
    bool IsRoomSpaceFree(Vector2Int newPositionIn, RoomData newRoomDataIn) {
        //calc new footprint
        int newRoomLeftBound = newPositionIn.x - newRoomDataIn.Width;
        int newRoomRightBound = newPositionIn.x;
        int newRoomBottomBound = newPositionIn.y;
        int newRoomTopBound = newPositionIn.y + newRoomDataIn.Height;

        foreach (KeyValuePair<Vector2Int, RoomInstance> placedRoom in placedRooms) {
            //Helpers
            Vector2Int placedRoomPosition = placedRoom.Key;
            RoomData placedRoomData = placedRoom.Value.RoomData;
            int placedRoomLeftBound = placedRoomPosition.x - placedRoomData.Width;
            int placedRoomRightBound = placedRoomPosition.x;
            int placedRoomBottomBound = placedRoomPosition.y;
            int placedRoomTopBound = placedRoomPosition.y + placedRoomData.Height;

            //Check for no overlap
            bool noOverlap = newRoomLeftBound >= placedRoomRightBound || newRoomRightBound <= placedRoomLeftBound || newRoomBottomBound >= placedRoomTopBound || newRoomTopBound <= placedRoomBottomBound;
            if (!noOverlap) {
                return false; //space is occupied
            }
        }
        return true; //space is free
    }
    #endregion
}
public enum StartRoomPosition {
    CENTER = 0,
    TOP = 1,
    TOP_RIGHT = 2,
    RIGHT = 3,
    BOTTOM_RIGHT = 4,
    BOTTOM = 5,
    BOTTOM_LEFT = 6,
    LEFT = 7,
    TOP_LEFT = 8

}