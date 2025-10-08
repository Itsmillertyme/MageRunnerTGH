using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
[RequireComponent(typeof(NavMeshLinkBuilder))]
[RequireComponent(typeof(MaskGeneratorPG2))]
[RequireComponent(typeof(EnemySpawnerPG2))]
public class LevelGenerator : MonoBehaviour {
    #region Variables
    [Header("Level Data")]
    [SerializeField] LevelData levelData;
    [SerializeField] NavMeshSurface navMeshSurface;
    [SerializeField] PlayerController playerController;

    [Header("Level Settings")]
    [SerializeField] StartRoomPosition startRoomPosition = StartRoomPosition.CENTER; //default to center
    [Range(0f, 1f)]
    [SerializeField] float deadEndChance;


    [Header("Debug Settings")]
    [SerializeField] bool levelGenerationDebug;
    [SerializeField] bool navMeshDebug;
    [SerializeField] bool enemySpawningDebug;
    [SerializeField] bool enemyAIDebug;
    [SerializeField] bool omitMask;

    [Header("Miscellaneous References")]
    [SerializeField] Transform levelParent;
    [SerializeField] Transform maskParent;
    [SerializeField] Transform enemyParent;
    [SerializeField] GameObject VirtualCameraContainer;

    //**FIELDS**
    RoomSelector roomSelector;
    Dictionary<Vector2Int, RoomInstance> placedRooms;
    Queue<PortalTask> openPortals;
    RoomInstance startRoomInstance;
    RoomInstance bossRoomInstance;
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
    //
    private void OnDisable() {
        ClearLevel();
    }
    #endregion

    #region Generation
    //**MAIN GENERATION METHOD**
    public void GenerateLevel() {
        if (levelGenerationDebug) Debug.Log("[Level Generation] ========== START GENERATION RUN ==========");
        bool generationValid = false;

        do {
            ClearLevel();
            Initialize();

            PlaceStartRoom();

            //Start building from open portals
            while (openPortals.Count > 0) {
                PortalTask currentPortalTask = openPortals.Dequeue();
                TryExpandFromPortal(currentPortalTask);
            }

            //place boss room
            PlaceBossRoom(out generationValid);
            if (!generationValid) {
                if (levelGenerationDebug) Debug.LogWarning("[Level Generation] Boss room placement failed; restarting generation.");
            }
            else {
                if (levelGenerationDebug) Debug.Log("[Level Generation] Boss room placed successfully.");
            }
        } while (!generationValid);


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
        if (levelGenerationDebug) Debug.Log($"[Level Generation] scanning {placedRooms.Count} placed rooms");
        foreach (KeyValuePair<Vector2Int, RoomInstance> placedRoom in placedRooms) {
            RoomInstance room = placedRoom.Value;

            if (levelGenerationDebug) Debug.Log($"[Level Generation]Closing {room.GetActiveUnconnectedPortals(levelGenerationDebug).Count} unconnected portals");
            foreach (PortalData portal in room.GetActiveUnconnectedPortals(levelGenerationDebug)) {
                portal.ClosePortal();
            }
        }

        //Bake navmesh
        if (navMeshSurface != null) {

            if (navMeshDebug) Debug.Log("[NavMesh Creation] Building NavMesh...");
            navMeshSurface.BuildNavMesh();
        }
        NavMeshLinkBuilder linkBuilder = GetComponent<NavMeshLinkBuilder>();
        linkBuilder.BuildAll();

        //Setup boss room environment
        BossRoomEnvironmentController controller = bossRoomInstance.RoomData.GetComponentInChildren<BossRoomEnvironmentController>();
        controller.HidePlatforms();

        //Create mask
        if (!omitMask) {
            MaskGeneratorPG2 maskGen = GetComponent<MaskGeneratorPG2>();
            List<RoomData> rooms = placedRooms.Values.Select(r => r.RoomData).ToList();
            maskGen.GenerateMaskMesh(rooms, levelData.MaxWidth, levelData.MaxHeight);
        }

        //Spawn Enemies
        EnemySpawnerPG2 enemySpawner = GetComponent<EnemySpawnerPG2>();
        enemySpawner.SpawnMobEnemies(placedRooms, enemyParent, enemySpawningDebug);
        enemySpawner.SpawnBossEnemy(bossRoomInstance, enemyParent, enemySpawningDebug);

    }

    //Clear level
    public void ClearLevel() {
        //Destroy player
        //        if (playerController != null) {
        //#if UNITY_EDITOR
        //            if (!Application.isPlaying)
        //                DestroyImmediate(playerController.gameObject);
        //            else
        //                Destroy(playerController.gameObject);
        //#else
        //            Destroy(playerController.gameObject);
        //#endif
        //        }

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
        navMeshSurface.RemoveData();
    }

    //Place start room at designated position
    void PlaceStartRoom() {
        GameObject startRoomPrefab = Instantiate(levelData.StartRoom, levelParent);
        RoomData startRoomData = startRoomPrefab.GetComponent<RoomData>();
        startRoomData.InitializePortals();

        if (levelGenerationDebug) {
            if (startRoomData == null) {
                Debug.LogError("[Level Generation] Start room prefab does not have RoomData on its root!");
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
        this.startRoomInstance = startRoomInstance;

        //Enqueue portals
        if (levelGenerationDebug
            ) {
            var portals = startRoomInstance.GetActiveUnconnectedPortals(levelGenerationDebug).ToList();
            Debug.Log($"[Level Generation] Starter room portals found: {portals.Count}");
        }
        foreach (PortalData portal in startRoomInstance.GetActiveUnconnectedPortals(levelGenerationDebug)) {
            if (levelGenerationDebug) {
                Debug.Log($"[Level Generation] Enqueueing portal {portal.PortalDirection} (Active={portal.IsActive}, Connected={portal.IsConnected})");
            }
            openPortals.Enqueue(new PortalTask(startRoomInstance, portal));
        }

        //Place Player
        playerController.transform.position = startRoomData.PlayerSpawn.position;
        if (levelGenerationDebug) Debug.Log($"[Level Generation] Player placed in scene at {startRoomData.PlayerSpawn.position}");

    }

    // Place Boss room
    void PlaceBossRoom(out bool successfulPlacement) {
        // Validate inputs
        if (levelData == null || levelData.BossRoom == null) {
            if (levelGenerationDebug) Debug.LogWarning("[LevelGen] No BossRoom assigned in LevelData.");
            successfulPlacement = false;
            return;
        }

        RoomData bossRoomDataPrefab = levelData.BossRoom.GetComponent<RoomData>();
        if (bossRoomDataPrefab == null) {
            if (levelGenerationDebug) Debug.LogError("[LevelGen] BossRoom prefab is missing RoomData on the root.");
            successfulPlacement = false;
            return;
        }

        if (placedRooms == null || placedRooms.Count == 0) {
            if (levelGenerationDebug) Debug.LogWarning("[LevelGen] No placed rooms — cannot place boss room.");
            successfulPlacement = false;
            return;
        }



        Vector2Int startGrid = startRoomInstance.GridPosition;

        //Scan all rooms for candidate endpoints (both active-unconnected and inactive/closed)
        PortalData bestPortal = null;
        RoomInstance bestRoom = null;
        Vector2Int bestBossGridPos = Vector2Int.zero;
        Vector3 bestBossWorldPos = Vector3.zero;
        float bestDistance = -1f;

        foreach (KeyValuePair<Vector2Int, RoomInstance> kvp in placedRooms) {
            RoomInstance room = kvp.Value;

            // Gather endpoints
            List<PortalData> endpoints = new List<PortalData>();
            // Active + unconnected
            List<PortalData> actives = room.GetActiveUnconnectedPortals(levelGenerationDebug);
            if (actives != null && actives.Count > 0) endpoints.AddRange(actives);
            // Inactive + not connected (dead ends)
            List<PortalData> inactives = room.GetInactivePortals(levelGenerationDebug);
            if (inactives != null && inactives.Count > 0) {
                for (int i = 0; i < inactives.Count; i++) {
                    PortalData p = inactives[i];
                    if (p != null && !p.IsConnected) {
                        endpoints.Add(p);
                    }
                }
            }

            // Evaluate each endpoint as a potential boss entrance target
            for (int i = 0; i < endpoints.Count; i++) {
                PortalData portal = endpoints[i];
                if (portal == null || portal.IsConnected) continue;

                // Boss must have an entrance opposite to this portal
                PortalData bossEntrancePrefab = bossRoomDataPrefab.GetPortalInDirection(portal.GetOppositeDirection());
                if (bossEntrancePrefab == null) continue;

                // Compute the boss room world & grid position if we snap entrance to this endpoint
                Vector3 endpointWorld = portal.GetWorldPosition();
                Vector3 bossEntranceLocal = bossEntrancePrefab.transform.localPosition;
                Vector3 bossWorldPos = endpointWorld - bossEntranceLocal;

                Vector2Int bossGridPos = new Vector2Int(
                    Mathf.RoundToInt(bossWorldPos.x),
                    Mathf.RoundToInt(bossWorldPos.y)
                );

                // Fit checks
                if (!IsWithinLevelBounds(bossGridPos, bossRoomDataPrefab)) continue;
                if (!IsRoomSpaceFree(bossGridPos, bossRoomDataPrefab)) continue;

                // Score = distance from start room in grid space (farther is better)
                float dist = Vector2Int.Distance(startGrid, bossGridPos);
                if (dist > bestDistance) {
                    bestDistance = dist;
                    bestPortal = portal;
                    bestRoom = room;
                    bestBossGridPos = bossGridPos;
                    bestBossWorldPos = bossWorldPos;
                }
            }
        }

        if (bestPortal == null || bestRoom == null) {
            if (levelGenerationDebug) Debug.LogWarning("[LevelGen] No valid endpoint found for Boss Room placement.");
            successfulPlacement = false;
            return;
        }

        // Instantiate and register the boss room
        GameObject bossObj = Instantiate(levelData.BossRoom, bestBossWorldPos, Quaternion.identity, levelParent);
        RoomData bossRoomData = bossObj.GetComponent<RoomData>();
        bossRoomData.InitializePortals();

        RoomInstance bossInstance = new RoomInstance(bossRoomData, bestBossGridPos);
        bossRoomInstance = bossInstance;
        placedRooms.Add(bestBossGridPos, bossInstance);

        // Connect the endpoint to the boss entrance on the **instance**
        PortalData bossEntranceOnInstance = bossRoomData.GetPortalInDirection(bestPortal.GetOppositeDirection());
        if (bossEntranceOnInstance == null) {
            if (levelGenerationDebug) Debug.LogWarning("[LevelGen] Boss room instance missing required entrance; destroying boss room.");
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(bossObj); else Destroy(bossObj);
#else
        Destroy(bossObj);
#endif
            placedRooms.Remove(bestBossGridPos);
            successfulPlacement = false;
            return;
        }

        bestPortal.OpenPortal();
        bestRoom.ConnectPortals(bestPortal, bossEntranceOnInstance);


        if (levelGenerationDebug) {
            Debug.Log($"[LevelGen] Boss Room placed at {bestBossGridPos} from {bestPortal.PortalDirection} of {bestRoom.RoomData.name}; distance = {bestDistance:F1}");
        }
        successfulPlacement = true;
    }

    // Expand level from a source portal task
    private void TryExpandFromPortal(PortalTask portalTaskIn) {
        // --- Source references ---
        RoomInstance sourceRoom = portalTaskIn.SourceRoom;
        PortalData sourcePortal = portalTaskIn.SourcePortal;

        if (levelGenerationDebug) {
            Debug.Log($"[Level Generation] Trying to expand from {sourcePortal.PortalDirection} portal of room {sourceRoom.RoomData.name} at {sourceRoom.GridPosition}");
        }

        // --- Choose connector list based on direction ---
        List<GameObject> connectorList =
            (sourcePortal.PortalDirection == PortalDirection.LEFT || sourcePortal.PortalDirection == PortalDirection.RIGHT)
                ? levelData.HorizontalConnectors
                : levelData.VerticalConnectors;

        if (connectorList == null || connectorList.Count == 0) {
            if (levelGenerationDebug) Debug.LogWarning("[Level Generation] No connector prefabs available for this direction. Closing source portal.");
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
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] Connector instance {connectorInstanceObj.name} missing entrance/exit for {sourcePortal.PortalDirection}. Closing source portal.");
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
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] Connector {connectorInstanceObj.name} out of bounds at {connectorGridPos}. Closing source portal.");
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
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] Space occupied at {connectorGridPos} for connector {connectorInstanceObj.name}. Closing source portal.");
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
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] No valid room prefab for exit {connectorExit.PortalDirection}. Closing connector exit.");
            connectorExit.ClosePortal();
            return;
        }

        // Instantiate next room first so its portals are initialized/active on the instance
        GameObject nextRoomInstanceObj = Instantiate(nextRoomPrefab, Vector3.zero, Quaternion.identity, levelParent);
        RoomData nextRoomData = nextRoomInstanceObj.GetComponent<RoomData>();
        nextRoomData.InitializePortals();

        PortalData nextRoomEntrance = nextRoomData.GetPortalInDirection(connectorExit.GetOppositeDirection());
        if (nextRoomEntrance == null) {
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] Next room instance {nextRoomInstanceObj.name} missing entrance for {connectorExit.PortalDirection}. Closing connector exit.");
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
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] Next room {nextRoomInstanceObj.name} out of bounds at {nextRoomGridPos}. Closing connector exit.");
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
            if (levelGenerationDebug) Debug.LogWarning($"[Level Generation] Space occupied at {nextRoomGridPos} for next room {nextRoomInstanceObj.name}. Closing connector exit.");
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
        List<PortalData> unconnected = placedRoomInstance.GetActiveUnconnectedPortals(levelGenerationDebug);
        for (int i = 0; i < unconnected.Count; i++) {
            PortalData portal = unconnected[i];
            if (portal != nextRoomEntrance) // safeguard; usually already excluded by "unconnected"
            {
                if (levelGenerationDebug) Debug.Log($"[Level Generation] Enqueueing new portal {portal.PortalDirection} from room {nextRoomData.name}");
                openPortals.Enqueue(new PortalTask(placedRoomInstance, portal));
            }
        }
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