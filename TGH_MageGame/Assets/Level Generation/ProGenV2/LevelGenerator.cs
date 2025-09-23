using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {
    #region Variables
    [Header("Level Setup")]
    [SerializeField] LevelData levelData;
    [SerializeField] StartRoomPosition startRoomPosition = StartRoomPosition.CENTER; //default to center
    [SerializeField] Transform levelParent;

    [Header("Level Settings")]
    [Range(0f, 1f)]
    [SerializeField] float deadEndChance;
    [SerializeField] bool debugMode;

    //**FIELDS**
    RoomSelector roomSelector;
    Dictionary<Vector2Int, RoomInstance> placedRooms;
    Queue<PortalTask> openPortals;
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
        foreach (KeyValuePair<Vector2Int, RoomInstance> placedRoom in placedRooms) {
            RoomInstance room = placedRoom.Value;
            foreach (PortalData portal in room.GetActiveUnconnectedPortals()) {
                portal.ClosePortal();
            }
        }
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
            var portals = startRoomInstance.GetActiveUnconnectedPortals().ToList();
            Debug.Log($"Starter room portals found: {portals.Count}");
        }
        foreach (PortalData portal in startRoomInstance.GetActiveUnconnectedPortals()) {
            if (debugMode) {
                Debug.Log($"Enqueueing portal {portal.PortalDirection} (Active={portal.IsActive}, Connected={portal.IsConnected})");
            }
            openPortals.Enqueue(new PortalTask(startRoomInstance, portal));
        }
    }
    //Expand level from a source portal task
    //void TryExpandFromPortal(PortalTask portalTaskIn) {
    //    // Helpers
    //    RoomInstance sourceRoom = portalTaskIn.SourceRoom;
    //    PortalData sourcePortal = portalTaskIn.SourcePortal;
    //    //
    //    List<GameObject> connectorList =
    //        (sourcePortal.PortalDirection == PortalDirection.LEFT || sourcePortal.PortalDirection == PortalDirection.RIGHT) ? levelData.HorizontalConnectors : levelData.VerticalConnectors;
    //    GameObject connectorPrefab = connectorList[Random.Range(0, connectorList.Count)];
    //    RoomData connectorRoomDataPrefab = connectorPrefab.GetComponent<RoomData>();
    //    PortalData connectorEntrancePrefab = connectorRoomDataPrefab.GetPortalInDirection(sourcePortal.GetOppositeDirection());
    //    PortalData connectorExitPrefab = connectorRoomDataPrefab.GetPortalInDirection(sourcePortal.PortalDirection);

    //    if (connectorEntrancePrefab == null || connectorExitPrefab == null) {
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    //Place connector
    //    Vector3 sourcePortalWorldPos = sourcePortal.GetWorldPosition();
    //    Vector3 connectorEntranceLocalPos = connectorEntrancePrefab.transform.localPosition;
    //    Vector3 connectorWorldPos = sourcePortalWorldPos - connectorEntranceLocalPos;

    //    Vector2Int connectorGridPos = new Vector2Int(
    //        Mathf.RoundToInt(connectorWorldPos.x),
    //        Mathf.RoundToInt(connectorWorldPos.y)
    //    );

    //    if (!IsWithinLevelBounds(connectorGridPos, connectorRoomDataPrefab) || !IsRoomSpaceFree(connectorGridPos, connectorRoomDataPrefab)) {
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    GameObject connectorInstanceObj = Instantiate(connectorPrefab, connectorWorldPos, Quaternion.identity, levelParent);
    //    RoomData connectorRoomData = connectorInstanceObj.GetComponent<RoomData>();
    //    connectorRoomData.InitializePortals();

    //    RoomInstance connectorInstance = new RoomInstance(connectorRoomData, connectorGridPos);
    //    placedRooms.Add(connectorGridPos, connectorInstance);

    //    PortalData connectorEntrance = connectorRoomData.GetPortalInDirection(sourcePortal.GetOppositeDirection());
    //    PortalData connectorExit = connectorRoomData.GetPortalInDirection(sourcePortal.PortalDirection);

    //    sourceRoom.ConnectPortals(sourcePortal, connectorEntrance);

    //    //Place new room
    //    GameObject nextRoomPrefab = roomSelector.GetValidRoomPrefab(connectorExit);
    //    if (nextRoomPrefab == null) {
    //        if (debugMode) {
    //            Debug.LogWarning($"No valid prefab for {sourcePortal.name} in {sourceRoom.RoomData.name}, closing portal.");
    //        }
    //        connectorExit.ClosePortal(); // no next room, close connector exit
    //        return;
    //    }

    //    RoomData nextRoomDataPrefab = nextRoomPrefab.GetComponent<RoomData>();
    //    PortalData nextRoomEntrancePrefab = nextRoomDataPrefab.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    if (nextRoomEntrancePrefab == null) {
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    Vector3 connectorExitWorldPos = connectorExit.GetWorldPosition();
    //    Vector3 nextRoomEntranceLocalPos = nextRoomEntrancePrefab.transform.localPosition;
    //    Vector3 nextRoomWorldPos = connectorExitWorldPos - nextRoomEntranceLocalPos;

    //    Vector2Int nextRoomGridPos = new Vector2Int(
    //        Mathf.RoundToInt(nextRoomWorldPos.x),
    //        Mathf.RoundToInt(nextRoomWorldPos.y)
    //    );

    //    if (!IsWithinLevelBounds(nextRoomGridPos, nextRoomDataPrefab) || !IsRoomSpaceFree(nextRoomGridPos, nextRoomDataPrefab)) {
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    GameObject nextRoomPrefabInstance = Instantiate(nextRoomPrefab, nextRoomWorldPos, Quaternion.identity, levelParent);
    //    RoomData placedRoomData = nextRoomPrefabInstance.GetComponent<RoomData>();
    //    placedRoomData.InitializePortals();

    //    RoomInstance placedRoomInstance = new RoomInstance(placedRoomData, nextRoomGridPos);
    //    placedRooms.Add(nextRoomGridPos, placedRoomInstance);

    //    PortalData nextRoomEntrance = placedRoomData.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    connectorInstance.ConnectPortals(connectorExit, nextRoomEntrance);

    //    //Enqueue portals
    //    foreach (PortalData portal in placedRoomInstance.GetActiveUnconnectedPortals()) {
    //        if (portal != nextRoomEntrance) {
    //            openPortals.Enqueue(new PortalTask(placedRoomInstance, portal));
    //        }
    //    }
    //}
    //void TryExpandFromPortal(PortalTask portalTaskIn) {
    //    // Helpers
    //    RoomInstance sourceRoom = portalTaskIn.SourceRoom;
    //    PortalData sourcePortal = portalTaskIn.SourcePortal;

    //    if (debugMode) {
    //        Debug.Log($"[Expand] Trying to expand from {sourcePortal.PortalDirection} portal of room {sourceRoom.RoomData.name} at {sourceRoom.GridPosition}");
    //    }

    //    // Connector selection
    //    List<GameObject> connectorList =
    //        (sourcePortal.PortalDirection == PortalDirection.LEFT || sourcePortal.PortalDirection == PortalDirection.RIGHT)
    //            ? levelData.HorizontalConnectors : levelData.VerticalConnectors;
    //    GameObject connectorPrefab = connectorList[Random.Range(0, connectorList.Count)];
    //    RoomData connectorRoomDataPrefab = connectorPrefab.GetComponent<RoomData>();
    //    PortalData connectorEntrancePrefab = connectorRoomDataPrefab.GetPortalInDirection(sourcePortal.GetOppositeDirection());
    //    PortalData connectorExitPrefab = connectorRoomDataPrefab.GetPortalInDirection(sourcePortal.PortalDirection);

    //    if (connectorEntrancePrefab == null || connectorExitPrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector prefab {connectorPrefab.name} missing entrance/exit for {sourcePortal.PortalDirection}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    // Place connector
    //    Vector3 sourcePortalWorldPos = sourcePortal.GetWorldPosition();
    //    Vector3 connectorEntranceLocalPos = connectorEntrancePrefab.transform.localPosition;
    //    Vector3 connectorWorldPos = sourcePortalWorldPos - connectorEntranceLocalPos;

    //    Vector2Int connectorGridPos = new Vector2Int(
    //        Mathf.RoundToInt(connectorWorldPos.x),
    //        Mathf.RoundToInt(connectorWorldPos.y)
    //    );

    //    if (!IsWithinLevelBounds(connectorGridPos, connectorRoomDataPrefab)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector {connectorPrefab.name} out of bounds at {connectorGridPos}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    if (!IsRoomSpaceFree(connectorGridPos, connectorRoomDataPrefab)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Space occupied at {connectorGridPos} for connector {connectorPrefab.name}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    GameObject connectorInstanceObj = Instantiate(connectorPrefab, connectorWorldPos, Quaternion.identity, levelParent);
    //    RoomData connectorRoomData = connectorInstanceObj.GetComponent<RoomData>();
    //    connectorRoomData.InitializePortals();

    //    RoomInstance connectorInstance = new RoomInstance(connectorRoomData, connectorGridPos);
    //    placedRooms.Add(connectorGridPos, connectorInstance);

    //    PortalData connectorEntrance = connectorRoomData.GetPortalInDirection(sourcePortal.GetOppositeDirection());
    //    PortalData connectorExit = connectorRoomData.GetPortalInDirection(sourcePortal.PortalDirection);

    //    sourceRoom.ConnectPortals(sourcePortal, connectorEntrance);

    //    // Place next room
    //    GameObject nextRoomPrefab = roomSelector.GetValidRoomPrefab(connectorExit);
    //    if (nextRoomPrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] No valid prefab found for exit {connectorExit.PortalDirection} from {sourceRoom.RoomData.name}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    RoomData nextRoomDataPrefab = nextRoomPrefab.GetComponent<RoomData>();
    //    PortalData nextRoomEntrancePrefab = nextRoomDataPrefab.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    if (nextRoomEntrancePrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Next room prefab {nextRoomPrefab.name} missing entrance for {connectorExit.PortalDirection}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    Vector3 connectorExitWorldPos = connectorExit.GetWorldPosition();
    //    Vector3 nextRoomEntranceLocalPos = nextRoomEntrancePrefab.transform.localPosition;
    //    Vector3 nextRoomWorldPos = connectorExitWorldPos - nextRoomEntranceLocalPos;

    //    Vector2Int nextRoomGridPos = new Vector2Int(
    //        Mathf.RoundToInt(nextRoomWorldPos.x),
    //        Mathf.RoundToInt(nextRoomWorldPos.y)
    //    );

    //    if (!IsWithinLevelBounds(nextRoomGridPos, nextRoomDataPrefab)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Next room {nextRoomPrefab.name} out of bounds at {nextRoomGridPos}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    if (!IsRoomSpaceFree(nextRoomGridPos, nextRoomDataPrefab)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Space occupied at {nextRoomGridPos} for next room {nextRoomPrefab.name}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    GameObject nextRoomPrefabInstance = Instantiate(nextRoomPrefab, nextRoomWorldPos, Quaternion.identity, levelParent);
    //    RoomData placedRoomData = nextRoomPrefabInstance.GetComponent<RoomData>();
    //    placedRoomData.InitializePortals();

    //    RoomInstance placedRoomInstance = new RoomInstance(placedRoomData, nextRoomGridPos);
    //    placedRooms.Add(nextRoomGridPos, placedRoomInstance);

    //    PortalData nextRoomEntrance = placedRoomData.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    connectorInstance.ConnectPortals(connectorExit, nextRoomEntrance);

    //    // Enqueue portals
    //    foreach (PortalData portal in placedRoomInstance.GetActiveUnconnectedPortals()) {
    //        if (portal != nextRoomEntrance) {
    //            if (debugMode) Debug.Log($"[Expand] Enqueueing new portal {portal.PortalDirection} from room {placedRoomData.name}");
    //            openPortals.Enqueue(new PortalTask(placedRoomInstance, portal));
    //        }
    //    }
    //}
    //void TryExpandFromPortal(PortalTask portalTaskIn) {
    //    // Helpers
    //    RoomInstance sourceRoom = portalTaskIn.SourceRoom;
    //    PortalData sourcePortal = portalTaskIn.SourcePortal;

    //    if (debugMode) {
    //        Debug.Log($"[Expand] Trying to expand from {sourcePortal.PortalDirection} portal of room {sourceRoom.RoomData.name} at {sourceRoom.GridPosition}");
    //    }

    //    // Choose connector prefab based on portal direction
    //    List<GameObject> connectorList =
    //        (sourcePortal.PortalDirection == PortalDirection.LEFT || sourcePortal.PortalDirection == PortalDirection.RIGHT)
    //            ? levelData.HorizontalConnectors : levelData.VerticalConnectors;

    //    if (connectorList.Count == 0) {
    //        Debug.LogWarning($"[Expand] No connector prefabs available for {sourcePortal.PortalDirection}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    GameObject connectorPrefab = connectorList[Random.Range(0, connectorList.Count)];

    //    // Instantiate connector first
    //    Vector3 sourcePortalWorldPos = sourcePortal.GetWorldPosition();
    //    GameObject connectorInstanceObj = Instantiate(connectorPrefab, sourcePortalWorldPos, Quaternion.identity, levelParent);

    //    RoomData connectorRoomData = connectorInstanceObj.GetComponent<RoomData>();
    //    connectorRoomData.InitializePortals();

    //    // Get entrance/exit portals from instance
    //    PortalData connectorEntrance = connectorRoomData.GetPortalInDirection(sourcePortal.GetOppositeDirection());
    //    PortalData connectorExit = connectorRoomData.GetPortalInDirection(sourcePortal.PortalDirection);

    //    if (connectorEntrance == null || connectorExit == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector instance {connectorInstanceObj.name} missing entrance/exit for {sourcePortal.PortalDirection}. Closing portal.");
    //        Destroy(connectorInstanceObj);
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    // Align connector so entrance matches source portal
    //    Vector3 connectorEntranceLocalPos = connectorEntrance.transform.localPosition;
    //    connectorInstanceObj.transform.position = sourcePortalWorldPos - connectorEntranceLocalPos;

    //    // Grid position for connector
    //    Vector2Int connectorGridPos = new Vector2Int(
    //        Mathf.RoundToInt(connectorInstanceObj.transform.position.x),
    //        Mathf.RoundToInt(connectorInstanceObj.transform.position.y)
    //    );

    //    if (!IsWithinLevelBounds(connectorGridPos, connectorRoomData) || !IsRoomSpaceFree(connectorGridPos, connectorRoomData)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector {connectorInstanceObj.name} invalid at {connectorGridPos}. Closing portal.");
    //        Destroy(connectorInstanceObj);
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    // Register connector room
    //    RoomInstance connectorInstance = new RoomInstance(connectorRoomData, connectorGridPos);
    //    placedRooms.Add(connectorGridPos, connectorInstance);

    //    // Connect portals
    //    sourceRoom.ConnectPortals(sourcePortal, connectorEntrance);

    //    // --- Place next room ---

    //    GameObject nextRoomPrefab = roomSelector.GetValidRoomPrefab(connectorExit);
    //    if (nextRoomPrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] No valid next room prefab for exit {connectorExit.PortalDirection}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    GameObject nextRoomInstanceObj = Instantiate(nextRoomPrefab, Vector3.zero, Quaternion.identity, levelParent);
    //    RoomData nextRoomData = nextRoomInstanceObj.GetComponent<RoomData>();
    //    nextRoomData.InitializePortals();

    //    PortalData nextRoomEntrance = nextRoomData.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    if (nextRoomEntrance == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Next room {nextRoomPrefab.name} missing entrance for {connectorExit.PortalDirection}. Closing connector exit.");
    //        Destroy(nextRoomInstanceObj);
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    // Align next room so entrance matches connector exit
    //    Vector3 connectorExitWorldPos = connectorExit.GetWorldPosition();
    //    Vector3 nextRoomEntranceLocalPos = nextRoomEntrance.transform.localPosition;
    //    nextRoomInstanceObj.transform.position = connectorExitWorldPos - nextRoomEntranceLocalPos;

    //    // Grid position for next room
    //    Vector2Int nextRoomGridPos = new Vector2Int(
    //        Mathf.RoundToInt(nextRoomInstanceObj.transform.position.x),
    //        Mathf.RoundToInt(nextRoomInstanceObj.transform.position.y)
    //    );

    //    if (!IsWithinLevelBounds(nextRoomGridPos, nextRoomData) || !IsRoomSpaceFree(nextRoomGridPos, nextRoomData)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Next room {nextRoomPrefab.name} invalid at {nextRoomGridPos}. Closing connector exit.");
    //        Destroy(nextRoomInstanceObj);
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    // Register next room
    //    RoomInstance nextRoomInstance = new RoomInstance(nextRoomData, nextRoomGridPos);
    //    placedRooms.Add(nextRoomGridPos, nextRoomInstance);

    //    // Connect connector exit to next room entrance
    //    connectorInstance.ConnectPortals(connectorExit, nextRoomEntrance);

    //    // Enqueue new portals
    //    foreach (PortalData portal in nextRoomInstance.GetActiveUnconnectedPortals()) {
    //        if (portal != nextRoomEntrance) {
    //            if (debugMode) Debug.Log($"[Expand] Enqueueing new portal {portal.PortalDirection} from room {nextRoomData.name}");
    //            openPortals.Enqueue(new PortalTask(nextRoomInstance, portal));
    //        }
    //    }
    //}
    //void TryExpandFromPortal(PortalTask portalTaskIn) {
    //    // Helpers
    //    RoomInstance sourceRoom = portalTaskIn.SourceRoom;
    //    PortalData sourcePortal = portalTaskIn.SourcePortal;

    //    if (debugMode) {
    //        Debug.Log($"[Expand] Trying to expand from {sourcePortal.PortalDirection} portal of room {sourceRoom.RoomData.name} at {sourceRoom.GridPosition}");
    //    }

    //    // === 1. Connector selection ===
    //    List<GameObject> connectorList =
    //        (sourcePortal.PortalDirection == PortalDirection.LEFT || sourcePortal.PortalDirection == PortalDirection.RIGHT)
    //            ? levelData.HorizontalConnectors : levelData.VerticalConnectors;

    //    GameObject connectorPrefab = connectorList[Random.Range(0, connectorList.Count)];

    //    // Place connector
    //    Vector3 sourcePortalWorldPos = sourcePortal.GetWorldPosition();
    //    RoomData connectorPrefabData = connectorPrefab.GetComponent<RoomData>();
    //    PortalData connectorEntrancePrefab = connectorPrefabData.GetPortalInDirection(sourcePortal.GetOppositeDirection());

    //    if (connectorEntrancePrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector prefab {connectorPrefab.name} missing entrance for {sourcePortal.PortalDirection}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    Vector3 connectorEntranceLocalPos = connectorEntrancePrefab.transform.localPosition;
    //    Vector3 connectorWorldPos = sourcePortalWorldPos - connectorEntranceLocalPos;
    //    Vector2Int connectorGridPos = new Vector2Int(Mathf.RoundToInt(connectorWorldPos.x), Mathf.RoundToInt(connectorWorldPos.y));

    //    if (!IsWithinLevelBounds(connectorGridPos, connectorPrefabData) || !IsRoomSpaceFree(connectorGridPos, connectorPrefabData)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector {connectorPrefab.name} invalid placement at {connectorGridPos}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    GameObject connectorInstanceObj = Instantiate(connectorPrefab, connectorWorldPos, Quaternion.identity, levelParent);
    //    RoomData connectorRoomData = connectorInstanceObj.GetComponent<RoomData>();
    //    connectorRoomData.InitializePortals();
    //    RoomInstance connectorInstance = new RoomInstance(connectorRoomData, connectorGridPos);
    //    placedRooms.Add(connectorGridPos, connectorInstance);

    //    PortalData connectorEntrance = connectorRoomData.GetPortalInDirection(sourcePortal.GetOppositeDirection());
    //    PortalData connectorExit = connectorRoomData.GetPortalInDirection(sourcePortal.PortalDirection);

    //    if (connectorExit == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Connector instance {connectorInstanceObj.name} missing exit for {sourcePortal.PortalDirection}. Closing portal.");
    //        sourcePortal.ClosePortal();
    //        return;
    //    }

    //    // Connect source to connector
    //    sourceRoom.ConnectPortals(sourcePortal, connectorEntrance);

    //    // === 2. Next room ===
    //    GameObject nextRoomPrefab = roomSelector.GetValidRoomPrefab(connectorExit);
    //    if (nextRoomPrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] No valid next room for {connectorExit.PortalDirection}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    RoomData nextRoomPrefabData = nextRoomPrefab.GetComponent<RoomData>();
    //    PortalData nextRoomEntrancePrefab = nextRoomPrefabData.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    if (nextRoomEntrancePrefab == null) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Next room prefab {nextRoomPrefab.name} missing entrance for {connectorExit.PortalDirection}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    Vector3 connectorExitWorldPos = connectorExit.GetWorldPosition();
    //    Vector3 nextRoomEntranceLocalPos = nextRoomEntrancePrefab.transform.localPosition;
    //    Vector3 nextRoomWorldPos = connectorExitWorldPos - nextRoomEntranceLocalPos;
    //    Vector2Int nextRoomGridPos = new Vector2Int(Mathf.RoundToInt(nextRoomWorldPos.x), Mathf.RoundToInt(nextRoomWorldPos.y));

    //    if (!IsWithinLevelBounds(nextRoomGridPos, nextRoomPrefabData) || !IsRoomSpaceFree(nextRoomGridPos, nextRoomPrefabData)) {
    //        if (debugMode) Debug.LogWarning($"[Expand] Next room {nextRoomPrefab.name} invalid placement at {nextRoomGridPos}. Closing connector exit.");
    //        connectorExit.ClosePortal();
    //        return;
    //    }

    //    GameObject nextRoomInstanceObj = Instantiate(nextRoomPrefab, nextRoomWorldPos, Quaternion.identity, levelParent);
    //    RoomData nextRoomData = nextRoomInstanceObj.GetComponent<RoomData>();
    //    nextRoomData.InitializePortals();
    //    RoomInstance nextRoomInstance = new RoomInstance(nextRoomData, nextRoomGridPos);
    //    placedRooms.Add(nextRoomGridPos, nextRoomInstance);

    //    PortalData nextRoomEntrance = nextRoomData.GetPortalInDirection(connectorExit.GetOppositeDirection());
    //    connectorInstance.ConnectPortals(connectorExit, nextRoomEntrance);

    //    // Enqueue all other portals
    //    foreach (PortalData portal in nextRoomInstance.GetActiveUnconnectedPortals()) {
    //        if (portal != nextRoomEntrance) {
    //            if (debugMode) Debug.Log($"[Expand] Enqueueing new portal {portal.PortalDirection} from room {nextRoomData.name}");
    //            openPortals.Enqueue(new PortalTask(nextRoomInstance, portal));
    //        }
    //    }
    //}
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
        List<PortalData> unconnected = placedRoomInstance.GetActiveUnconnectedPortals();
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
        // Destroy all child objects
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