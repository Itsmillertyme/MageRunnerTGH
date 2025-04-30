using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DungeonCreator : MonoBehaviour {

    [Header("Dungeon Seed Settings")]
    [ReadOnly]
    public int levelSeed;
    [Tooltip("Keep current seed during dungeon clear?")]
    public bool keepSeed;

    [Header("Dungeon Size Settings")]
    public int dungeonHeight;
    public int dungeonWidth;
    public int roomHeightMin;
    public int roomWidthMin;
    public int corridorSize;

    [Header("Generator Settings")]
    public bool dungeonFlatMode;

    public int maxIterations;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1f)]
    public float roomTopCornerModifier;
    [Range(0, 2)]
    public int roomOffset;
    [Range(0, 25)]
    public int NavMeshLinkJumpDistance;

    [Header("Parent References")]
    public Transform roomParent;
    public Transform corridorParent;
    public Transform levelParent;
    public Transform pathNodeParent;
    public Transform wallParent;
    public Transform maskParent;
    public Transform platformParent;
    public Transform decorationParent;
    public Transform enemiesParent;
    public Transform devParent;

    [Header("Prefab References")]
    public GameObject wallHorizontal;
    public GameObject wallVertical;
    public GameObject maskPrefab;
    public GameObject playerPrefab;
    public GameObject bossPrefab;
    public GameObject corridorEffect;
    public GameObject platformPrefab;
    public GameObject castleWall5x5Prefab;
    public GameObject castleWall1x5Prefab;


    [Header("Misc References")]
    public Material roomMaterial;
    public Material corridorMaterial;
    public Mesh pathNodeMesh;
    public Material pathNodeBaseMaterial;
    public Material pathNodeStartMaterial;
    public Material pathNodeEndMaterial;
    public LevelDecorations levelDecorations;
    [SerializeField] GameManager gameManager;
    [SerializeField] LevelDecorator levelDecorator;

    //public LevelEnemies levelEnemies;
    //public NavMeshSurface navMeshSurface;

    List<WallData> possibleDoorHorizontalPosition;
    List<WallData> possibleDoorVerticalPosition;
    List<WallData> possibleWallPosition;
    List<WallData> possibleFloorCeilingPosition;

    List<Vector3> platformLocations;
    List<GameObject> backgroundWalls;
    List<GameObject> pathNodeObjects;


    //**Unity Methods*
    void Start() {
        if (gameManager.GenerateLevelOnLoad) {
            RetryGeneration();
        }
    }
    //
    private void OnApplicationQuit() {
        NavMeshSurface nms = GetComponent<NavMeshSurface>();
        nms.RemoveData();
    }

    //**Generation Methods**
    private void CreateDungeon() { //Main method for making dungeon

        //*PRIME PSEUDO RANDOM NUMBER GENERATOR*
        //RandomSeedManager.DebugLevelSeeds();

        if (levelSeed == 0) {
            levelSeed = RandomSeedManager.SetRandomSeed();
        }
        else {
            RandomSeedManager.SetSeed(levelSeed);
        }


        //*BEGIN DUNGEON GENERATION*
        //Create instance of generator script
        DungeonGenerator generator = new DungeonGenerator(dungeonHeight, dungeonWidth);
        //Generate list of rooms
        var listOfRooms = generator.CalculateDungeon(maxIterations,
                                                   roomHeightMin,
                                                   roomWidthMin,
                                                   roomBottomCornerModifier,
                                                   roomTopCornerModifier,
                                                   roomOffset);

        //Instantiate lists
        possibleDoorHorizontalPosition = new List<WallData>();
        possibleDoorVerticalPosition = new List<WallData>();
        possibleWallPosition = new List<WallData>();
        possibleFloorCeilingPosition = new List<WallData>();


        //wallParent.parent = transform;

        //Generate list of corridors
        var listOfCorridors = generator.CalculateCorridors(corridorSize);

        //Create path nodes
        pathNodeObjects = CreatePathNodes(listOfRooms, listOfCorridors);

        //Create mesh and object from list of rooms
        for (int i = 0; i < listOfRooms.Count; i++) {

            backgroundWalls = CreateRoomBackground(listOfRooms[i], roomMaterial, roomParent);
            GenerateWallPositions(listOfRooms[i], roomMaterial, roomParent);
        }
        //Create mesh and object from list of corridors
        for (int i = 0; i < listOfCorridors.Count; i++) {

            CreateCorridorBackground(listOfCorridors[i], corridorMaterial, corridorParent);
            GenerateWallPositions(listOfCorridors[i], corridorMaterial, corridorParent);
        }

        //Create walls
        CreateWalls(wallParent);


        List<Transform> walls = new List<Transform>();
        for (int i = 0; i < wallParent.childCount; i++) {

            walls.Add(wallParent.GetChild(i));
        }

        //Create backmask for level
        CreateMasks(listOfRooms, listOfCorridors);

        //*END DUNGEON GENERATION*

        //*RUN PATH FINDER*
        PathFinder pf = new PathFinder(pathNodeObjects);
        for (int i = 0; i < pf.EndPoints.Count; i++) {
            pf.EndPoints[i].GetComponent<MeshRenderer>().sharedMaterial = pathNodeEndMaterial;
        }
        pf.StartPoint.GetComponent<MeshRenderer>().sharedMaterial = pathNodeStartMaterial;
        GetComponent<GameManager>().SetCurrentPathNode(pf.StartPoint, true); //set current player pathnodep

        //*PLACE ROOM SPECIFIC OBJECTS*     
        foreach (PathNode node in pf.PathNodes) {
            //Create platforms
            if (node.Type == PathNodeType.ROOM) {
                platformLocations = CalculatePlatforms(node);
            }

            //Create corridor effects
            if (node.Type == PathNodeType.CORRIDOR) {
                GameObject effect = Instantiate(corridorEffect, corridorParent);
                CorridorEffectController cec = effect.GetComponent<CorridorEffectController>();

                cec.CorridorPathNode = node;
                node.CorridorEffectController = cec;
                if (!gameManager.UnlockAllPaths) {
                    cec.SetCorridorState(false);
                }


                if (node.Direction == Direction.VERTICAL) {
                    effect.transform.position = new Vector3(node.RoomTopLeftCorner.x + (corridorSize / 2) + .5f, 2.5f, node.RoomTopLeftCorner.y - 1f);
                    effect.transform.localRotation = Quaternion.Euler(0, 90, 0);
                }
                else {
                    effect.transform.position = new Vector3(node.RoomTopLeftCorner.x + 1f, 2.5f, node.RoomTopLeftCorner.y - (corridorSize / 2) - .5f);
                    effect.transform.localRotation = Quaternion.Euler(0, 0, 0);

                }

                cec.SetupEffect(effect.transform.localPosition, node.Direction, node.Direction == Direction.VERTICAL ? node.RoomDimensions.y - 2f : node.RoomDimensions.x - 2f);
            }

            //Create Safety Platforms
            if (node.Type == PathNodeType.CORRIDOR && node.Direction == Direction.HORTIZONTAL) {
                //use rays to check left (bottom) side

                Vector3 checkOrigin = new Vector3(node.RoomTopLeftCorner.x, 2.5f, node.RoomTopLeftCorner.y - (node.RoomDimensions.y / 2f));

                RaycastHit hitInfo = new RaycastHit();
                Ray rayLeft = new Ray(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z + 5) - checkOrigin);
                Ray rayCenter = new Ray(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z) - checkOrigin);
                Ray rayRight = new Ray(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z - 5) - checkOrigin);

                bool hitLeft = Physics.Raycast(rayLeft, out hitInfo, 9.4f);
                bool hitCenter = Physics.Raycast(rayCenter, out hitInfo, 8f);
                bool hitRight = Physics.Raycast(rayRight, out hitInfo, 9.4f);


                //Debug.Log("Corridor(" + node.name + ") platform check - Left) " + hitLeft + ", Center) " + hitCenter + ", Right) " + hitRight);

                if (!hitLeft && !hitCenter && !hitRight) {
                    GameObject platform = Instantiate(platformPrefab, new Vector3(node.RoomTopLeftCorner.x - 5, -0.5f, node.RoomTopLeftCorner.y), Quaternion.Euler(0, 0, 0), platformParent);
                    platform.name = "SAFETY PLATFORM";
                    platform.layer = 9; //navmesh ground
                }

                if (gameManager.DebugLevelGeneration && dungeonFlatMode) {
                    GameObject test = new GameObject("Just Testing");
                    test.transform.position = checkOrigin;
                    test.transform.parent = corridorParent;

                    Debug.DrawLine(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z), Color.blue, 60);
                    Debug.DrawLine(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z + 5), Color.blue, 60);
                    Debug.DrawLine(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z - 5), Color.blue, 60);
                }
            }
        }

        //*PLACE LEVEL DECORATIONS*
        //Floor
        List<Vector3> itemLocations = levelDecorator.GenerateFloorItemLocations(platformLocations);
        levelDecorator.SpawnFloorDecorationItems(itemLocations, decorationParent);
        //Walls
        levelDecorator.DecorateLevelWalls(backgroundWalls, wallParent);
        //Player door
        //levelDecorator.PlacePlayerDoorDecoration(playerPrefab.transform.position);

        //*PLACE PLAYER*
        PlacePlayer(pf.StartPoint);

        //*SPAWN ENEMIES*
        EnemySpawner es = GameObject.Find("GameManager").GetComponent<EnemySpawner>();
        //Mobs
        foreach (PathNode room in pf.PathNodes) {
            if (room.Type == PathNodeType.ROOM && room != pf.EndPoints[pf.EndPoints.Count - 1]) {
                if (room == pf.StartPoint) {
                    room.Enemies = es.SpawnMobEnemies(room, enemiesParent, startRoom: true);
                }
                else {
                    room.Enemies = es.SpawnMobEnemies(room, enemiesParent);
                }

            }
            else if (room.Type == PathNodeType.CORRIDOR) {
                room.Enemies = new List<GameObject>();
            }
        }
        //Boss
        PathNode bossNode = pf.EndPoints[pf.EndPoints.Count - 1];
        GameObject bossObj = es.SpawnBoss(bossNode, (bossNode.RoomTopLeftCorner.y - bossNode.RoomDimensions.y / 2) > (dungeonWidth / 2) ? true : false, enemiesParent);
        bossNode.Enemies = new List<GameObject> { bossObj };

        //*ROTATE TO PROPER PERSPECTIVE*
        if (!dungeonFlatMode) {
            //DEV - rotate parent to show vertically
            levelParent.rotation = Quaternion.Euler(0, 90, 90);
            levelParent.position = new Vector3(0, 0, -0.5f);
        }

        //*BAKE NAVMESH
        NavMeshSurface nms = GetComponent<NavMeshSurface>();
        nms.BuildNavMesh();

        //*ENSURE PROPER LINK PLACEMENT*
        NavMeshLink[] links = FindObjectsByType<NavMeshLink>(FindObjectsSortMode.None);
        for (int i = 0; i < links.Length; i++) {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(links[i].startPoint, out hit, .5f, NavMesh.AllAreas)) {
                links[i].startPoint = hit.position;
            }
            hit = new NavMeshHit();
            if (NavMesh.SamplePosition(links[i].endPoint, out hit, .5f, NavMesh.AllAreas)) {
                links[i].endPoint = hit.position;
            }
        }

        //*TURN ON NAVMESH AGENTS FOR ALL ENEMIES AND INIT BEHAVIORS*
        for (int i = 0; i < pf.PathNodes.Count; i++) {
            PathNode room = pf.PathNodes[i];
            //Debug.Log(room.name);
            //Debug.Log(room.Enemies.Count);
            foreach (GameObject enemy in room.Enemies) {
                //Debug.Log(enemy.name);
                if (enemy.CompareTag("Mob Enemy")) {
                    //Debug.Log(enemy.name);
                    enemy.GetComponent<NavMeshAgent>().enabled = true;
                    //Debug.Log("Is mob");
                    IBehave[] behaviors = enemy.GetComponents<IBehave>();
                    foreach (IBehave behavior in behaviors) {
                        behavior.Initialize(room, gameManager.DebugEnemySpawning);
                    }
                }
            }
        }

        //DEV ONLY
        if (gameManager.DebugLevelGeneration) {
            for (int i = 0; i < pf.Path.Count - 1; i++) {
                Debug.DrawLine(pf.Path[i].transform.position, pf.Path[i + 1].transform.position, Color.green, 20);
            }
        }
    }
    //
    public void RetryGeneration() {
        //Clear
        ClearDungeon();

        //create new dungeon
        CreateDungeon();
    }

    //**Utility Methods**
    public void ClearDungeon() {
        //get all rooms, corridors, walls and masks
        //Transform[] levelChildren = waypointsParent.GetComponentsInChildren<Transform>(true);
        //for (int i = levelChildren.Length - 1; i > 0; i--) {
        //    Transform[] subChildren = levelChildren[i].GetComponentsInChildren<Transform>(true);
        //    for (int j = subChildren.Length - 1; i > 0; i--) {
        //        DestroyImmediate(subChildren[i].gameObject);
        //    }
        //}        

        Transform[] roomchildren = roomParent.GetComponentsInChildren<Transform>(true);
        Transform[] corridorchildren = corridorParent.GetComponentsInChildren<Transform>(true);
        Transform[] wallchildren = wallParent.GetComponentsInChildren<Transform>(true);
        Transform[] maskchildren = maskParent.GetComponentsInChildren<Transform>(true);
        Transform[] pathNodeChildren = pathNodeParent.GetComponentsInChildren<Transform>(true);
        Transform[] platformChildren = platformParent.GetComponentsInChildren<Transform>(true);
        Transform[] decorationChildren = decorationParent.GetComponentsInChildren<Transform>(true);
        Transform[] enemiesChildren = enemiesParent.GetComponentsInChildren<Transform>(true);
        Transform[] devChildren = devParent.GetComponentsInChildren<Transform>(true);

        //reset dungeon parent rotation
        levelParent.rotation = Quaternion.Euler(0, 0, 0);
        levelParent.position = new Vector3(0, 0, 0);

        //Destroy room objects
        for (int i = roomchildren.Length - 1; i > 0; i--) {
            DestroyImmediate(roomchildren[i].gameObject);
        }
        //Destroy corridor objects
        for (int i = corridorchildren.Length - 1; i > 0; i--) {
            DestroyImmediate(corridorchildren[i].gameObject);
        }
        //Destroy wall objects
        for (int i = wallchildren.Length - 1; i > 0; i--) {
            DestroyImmediate(wallchildren[i].gameObject);
        }
        //Destroy mask objects
        for (int i = maskchildren.Length - 1; i > 0; i--) {
            DestroyImmediate(maskchildren[i].gameObject);
        }
        //Destroy Pathnode objects
        for (int i = pathNodeChildren.Length - 1; i > 0; i--) {
            DestroyImmediate(pathNodeChildren[i].gameObject);
        }
        //Destroy platform objects
        for (int i = platformChildren.Length - 1; i > 0; i--) {
            DestroyImmediate(platformChildren[i].gameObject);
        }
        //Destroy decoration objects
        for (int i = decorationChildren.Length - 1; i > 0; i--) {
            DestroyImmediate(decorationChildren[i].gameObject);
        }
        //Destroy enemies objects
        for (int i = enemiesChildren.Length - 1; i > 0; i--) {
            DestroyImmediate(enemiesChildren[i].gameObject);
        }
        //Destroy DEV waypoint objects
        for (int i = devChildren.Length - 1; i > 0; i--) {
            DestroyImmediate(devChildren[i].gameObject);
        }

        //clear navmesh
        GetComponent<NavMeshSurface>().RemoveData();

        //clear seed field
        levelSeed = keepSeed ? levelSeed : 0;

    }
    //
    private void CreateWalls(Transform wallParent) { //generate all walls
        foreach (WallData wallPosition in possibleWallPosition) {
            CreateWall(wallParent, wallPosition, wallVertical);
        }

        foreach (WallData wallPosition in possibleFloorCeilingPosition) {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }
    }
    //
    private void CreateWall(Transform wallParentIn, WallData wallDataIn, GameObject wallPrefabIn) { //generate a single wall

        if (wallDataIn.direction == WallDirection.LEFT || wallDataIn.direction == WallDirection.RIGHT) {
            //Walls facing East or West (Vertical walls)
            GameObject go = Instantiate(wallPrefabIn, new Vector3(wallDataIn.position.x + 1, wallDataIn.position.y - 0.01f, wallDataIn.direction == WallDirection.LEFT ? wallDataIn.position.z + 0.25f : wallDataIn.position.z - 0.25f), Quaternion.Euler(0, 0, 0), wallParentIn);
        }
        else {
            //Walls facing North or South (Horizontal walls)
            GameObject go = Instantiate(wallPrefabIn, new Vector3(wallDataIn.position.x, wallDataIn.position.y - 0.01f, wallDataIn.position.z), Quaternion.Euler(90, 90, 0), wallParentIn);

            //Set non corridors on ground layer for Navmesh
            //go.layer = wallDataIn.isCorridor ? 0 : 9;
            if (wallDataIn.direction == WallDirection.DOWN && !wallDataIn.isCorridor) {
                go.layer = 9;
            }
        }
    }
    //
    private void CreateMasks(List<RoomNode> roomsIn, List<CorridorNode> corridorsIn) { //Generate masking meshes

        MaskGenerator mg = GetComponent<MaskGenerator>();

        mg.GenerateMaskMesh(roomsIn, corridorsIn, dungeonWidth, dungeonHeight);

    }
    //
    List<GameObject> CreateRoomBackground(RoomNode roomNode, Material materialIn, Transform allRoomParentIn) {
        List<GameObject> fullWallsOut = new List<GameObject>();

        //Create mesh vertices
        Vector3 bottomLeftVertice = new Vector3(roomNode.BottomLeftAreaCorner.x, 0, roomNode.BottomLeftAreaCorner.y);
        Vector3 bottomRightVertice = new Vector3(roomNode.TopRightAreaCorner.x, 0, roomNode.BottomRightAreaCorner.y);
        Vector3 topLeftVertice = new Vector3(roomNode.BottomLeftAreaCorner.x, 0, roomNode.TopRightAreaCorner.y);
        Vector3 topRightVertice = new Vector3(roomNode.TopRightAreaCorner.x, 0, roomNode.TopRightAreaCorner.y);

        GameObject thisRoomParent = new GameObject("Room (" + topLeftVertice.x + ", " + topLeftVertice.z + ")");



        for (int i = (int) bottomLeftVertice.z; i < topLeftVertice.z - 4; i += 5) {
            for (int j = (int) bottomLeftVertice.x; j < bottomRightVertice.x - 4; j += 5) {

                GameObject prefab = castleWall5x5Prefab;

                //float randomFloat = UnityEngine.Random.value;

                //if (randomFloat < wallDecorationFrequency) {
                //    int choice = UnityEngine.Random.Range(0, 2);
                //    if (choice == 0) {
                //        prefab = castleWall5x5DrainPrefab;
                //    }
                //    else {
                //        prefab = castleWall5x5WindowPrefab;
                //    }
                //}

                Vector3 wallPos = new Vector3(j, -0.25f, i);
                GameObject wallPiece = Instantiate(prefab, wallPos, Quaternion.Euler(90, 90, 0), thisRoomParent.transform);
                fullWallsOut.Add(wallPiece);
            }

            int roomWidth = (int) Math.Abs(bottomLeftVertice.x - bottomRightVertice.x);

            if ((roomWidth % 5) != 0) {
                for (int j = (int) i; j < i + 5; j++) {
                    Vector3 wallPos = new Vector3(bottomRightVertice.x - (roomWidth % 5), -0.25f, j);
                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), thisRoomParent.transform);
                }
            }
        }
        int roomHeight = (int) Math.Abs(bottomLeftVertice.z - topLeftVertice.z);
        if ((roomHeight % 5) != 0) {
            for (int i = (int) (topLeftVertice.z - (roomHeight % 5)); i < topLeftVertice.z; i++) {
                for (int j = (int) topLeftVertice.x; j < topRightVertice.x; j += 5) {
                    Vector3 wallPos = new Vector3(j, -0.25f, i);
                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), thisRoomParent.transform);
                }
            }
        }

        //Set this room as child of parent
        thisRoomParent.transform.parent = allRoomParentIn;

        //set bottom left corner gameobject as child of mesh
        roomNode.bottomRightCornerObject.transform.parent = thisRoomParent.transform;

        return fullWallsOut;
    }
    //
    void CreateCorridorBackground(CorridorNode corridorNode, Material materialIn, Transform newObjectParent) {

        //Create mesh vertices
        Vector3 bottomLeftVertice = new Vector3(corridorNode.BottomLeftAreaCorner.x, 0, corridorNode.BottomLeftAreaCorner.y);
        Vector3 bottomRightVertice = new Vector3(corridorNode.TopRightAreaCorner.x, 0, corridorNode.BottomRightAreaCorner.y);
        Vector3 topLeftVertice = new Vector3(corridorNode.BottomLeftAreaCorner.x, 0, corridorNode.TopRightAreaCorner.y);
        Vector3 topRightVertice = new Vector3(corridorNode.TopRightAreaCorner.x, 0, corridorNode.TopRightAreaCorner.y);

        GameObject corridorParent = new GameObject("Corridor (" + topLeftVertice.x + ", " + topLeftVertice.z + ")");



        if (corridorNode.Direction == Direction.HORTIZONTAL) {

            for (int i = (int) bottomLeftVertice.x; i < bottomRightVertice.x - 4; i += 5) {
                Vector3 wallPos = new Vector3(i, -0.25f, bottomRightVertice.z);
                GameObject wallPiece = Instantiate(castleWall5x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
            }

            int corridorWidth = (int) Math.Abs(bottomLeftVertice.x - bottomRightVertice.x);

            if ((corridorWidth % 5) != 0) {
                for (int i = (int) bottomRightVertice.z; i < bottomRightVertice.z + 5; i++) {
                    Vector3 wallPos = new Vector3(bottomRightVertice.x - (corridorWidth % 5), -0.25f, i);
                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
                }
            }
        }
        else {
            for (int i = (int) bottomLeftVertice.z; i < topLeftVertice.z - 4; i += 5) {
                Vector3 wallPos = new Vector3(bottomLeftVertice.x, -0.25f, i);
                GameObject wallPiece = Instantiate(castleWall5x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
            }

            int corridorHeight = (int) Math.Abs(bottomLeftVertice.z - topLeftVertice.z);

            if ((corridorHeight % 5) != 0) {
                for (int i = (int) topLeftVertice.z - (corridorHeight % 5); i < topLeftVertice.z; i++) {
                    Vector3 wallPos = new Vector3(bottomLeftVertice.x, -0.25f, i);
                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
                }
            }
        }
        corridorParent.transform.parent = newObjectParent;

        //set bottom left corner gameobject as child of mesh
        corridorNode.bottomRightCornerObject.transform.parent = corridorParent.transform;

    }
    //
    void GenerateWallPositions(Node node, Material materialIn, Transform newObjectParent) { //Generate wall positions

        //Helpers
        bool isCorridor = node is CorridorNode;

        //Create mesh vertices
        Vector3 bottomLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.BottomLeftAreaCorner.y);
        Vector3 bottomRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.BottomRightAreaCorner.y);
        Vector3 topLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.TopRightAreaCorner.y);
        Vector3 topRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.TopRightAreaCorner.y);

        //GENERATE WALL POSITIONS
        //Right side of room
        for (int row = (int) bottomLeftVertice.x; row < (int) bottomRightVertice.x; row++) {
            var wallPosition = new Vector3(row, 0, bottomLeftVertice.z);
            AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.RIGHT, isCorridor);
        }
        //Left side of room
        for (int row = (int) topLeftVertice.x; row < (int) node.TopRightAreaCorner.x; row++) {
            var wallPosition = new Vector3(row, 0, topRightVertice.z);
            AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.LEFT, isCorridor);
        }
        //Bottom of Room
        for (int col = (int) bottomLeftVertice.z; col < (int) topLeftVertice.z; col++) {
            var wallPosition = new Vector3(bottomLeftVertice.x, 0, col);
            AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.DOWN, isCorridor);
        }
        //top of room
        for (int col = (int) bottomRightVertice.z; col < (int) topRightVertice.z; col++) {
            var wallPosition = new Vector3(bottomRightVertice.x, 0, col);
            AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.UP, isCorridor);
        }

    }
    //
    private List<GameObject> CreatePathNodes(List<RoomNode> listOfRooms, List<CorridorNode> listOfCorridors) {

        List<GameObject> pathNodesObjectsOut = new List<GameObject>();

        for (int i = 0; i < listOfRooms.Count; i++) {
            Vector2Int centerPoint = (listOfRooms[i].TopLeftAreaCorner + listOfRooms[i].BottomRightAreaCorner) / 2;

            //create path node object
            GameObject pathNodeObject = new GameObject("PathNode - room " + i, typeof(MeshFilter), typeof(MeshRenderer));


            if (gameManager.DebugLevelGeneration) {
                pathNodeObject.GetComponent<MeshFilter>().mesh = pathNodeMesh;
                pathNodeObject.GetComponent<MeshRenderer>().material = pathNodeBaseMaterial;
            }

            //add pathnode script 
            PathNode pathNode = pathNodeObject.AddComponent<PathNode>();
            pathNode.Type = PathNodeType.ROOM;
            pathNode.Direction = Direction.VERTICAL;
            pathNode.RoomDimensions = new Vector2Int(listOfRooms[i].Width, listOfRooms[i].Length);
            pathNode.RoomTopLeftCorner = listOfRooms[i].TopLeftAreaCorner;

            pathNodeObject.transform.parent = pathNodeParent;

            pathNodeObject.transform.position = new Vector3(centerPoint.x, 5, centerPoint.y);

            //PATH NODES
            listOfRooms[i].pathNode = pathNodeObject;

            pathNodesObjectsOut.Add(pathNodeObject);
        }

        for (int i = 0; i < listOfCorridors.Count; i++) {
            Vector2Int centerPoint = (listOfCorridors[i].TopLeftAreaCorner + listOfCorridors[i].BottomRightAreaCorner) / 2;

            //create path node object
            GameObject pathNodeObject = new GameObject("PathNode - corridor " + i, typeof(MeshFilter), typeof(MeshRenderer));


            if (gameManager.DebugLevelGeneration) {
                pathNodeObject.GetComponent<MeshFilter>().mesh = pathNodeMesh;
                pathNodeObject.GetComponent<MeshRenderer>().material = pathNodeBaseMaterial;
            }

            //add pathnode script 
            PathNode pathNode = pathNodeObject.AddComponent<PathNode>();
            pathNode.Type = PathNodeType.CORRIDOR;
            pathNode.Direction = listOfCorridors[i].Direction;
            pathNode.RoomDimensions = new Vector2Int(listOfCorridors[i].Width, listOfCorridors[i].Length);
            pathNode.RoomTopLeftCorner = listOfCorridors[i].TopLeftAreaCorner;


            pathNodeObject.transform.parent = pathNodeParent;
            pathNodeObject.transform.position = new Vector3(centerPoint.x, 5, centerPoint.y);

            //PATH NODES
            listOfCorridors[i].pathNode = pathNodeObject;


            List<GameObject> adjacentStructuresPathNodeObjects = new List<GameObject> {
                listOfCorridors[i].Structure1.pathNode,
                listOfCorridors[i].Structure2.pathNode
            };


            //set corridor neighors with rooms
            pathNode.neighbors = adjacentStructuresPathNodeObjects;

            //Set corridor structures to have the corridor as a neighbor
            listOfCorridors[i].Structure1.pathNode.GetComponent<PathNode>().neighbors.Add(pathNodeObject);


            listOfCorridors[i].Structure2.pathNode.GetComponent<PathNode>().neighbors.Add(pathNodeObject);


            pathNodesObjectsOut.Add(pathNodeObject);
        }

        return pathNodesObjectsOut;
    }
    //
    private void AddWallPositionToList(Vector3 wallPositionIn, List<WallData> wallListIn, List<WallData> doorListIn, WallDirection directionIn, bool isCorridorIn) { //sets positions of walls to proper list
        //get point from wall position
        Vector3Int point = Vector3Int.CeilToInt(wallPositionIn);
        WallData temp = new WallData(point, directionIn, isCorridorIn);
        int index = -1;

        //Check if wall list already contains this point
        for (int i = 0; i < wallListIn.Count; i++) {
            if (wallListIn[i].position == temp.position) {
                index = i;
                break;
            }
        }

        //Test index need to be a door
        if (index != -1) {
            wallListIn.RemoveAt(index);
            doorListIn.Add(temp);
        }
        else {
            //add to wall list
            wallListIn.Add(temp);
        }
    }
    //
    public void PlacePlayer(PathNode spawnRoomPathNode) {
        Vector3 spawnPos = Vector3.zero;

        if ((spawnRoomPathNode.RoomTopLeftCorner.y - spawnRoomPathNode.RoomDimensions.y / 2) > dungeonWidth / 2) {
            //Spawn on "left" side of room
            spawnPos = new Vector3(spawnRoomPathNode.RoomTopLeftCorner.x + .1f, 0, spawnRoomPathNode.RoomTopLeftCorner.y - 2);

            //Rotate player
            playerPrefab.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 270, 0);
            playerPrefab.GetComponent<PlayerController>().IsFacingLeft = false;

        }
        else {
            //spawn on "right" side of room
            spawnPos = new Vector3(spawnRoomPathNode.RoomTopLeftCorner.x + .1f, 0, spawnRoomPathNode.RoomTopLeftCorner.y - spawnRoomPathNode.RoomDimensions.y + 1);

            //Rotate player
            playerPrefab.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 90, 0);
            playerPrefab.GetComponent<PlayerController>().IsFacingLeft = true;
        }


        //places player
        playerPrefab.transform.position = spawnPos;

        levelDecorator.PlacePlayerDoorDecoration(playerPrefab.transform.position, gameManager.DebugLevelGeneration);
        if (!dungeonFlatMode) {
            //rotates player around origin to account for level rotation
            playerPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
            playerPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
        }

        playerPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerPrefab.transform.position = new Vector3(playerPrefab.transform.position.x + 1, spawnPos.x, playerPrefab.transform.position.z + 1);



    }
    //
    public List<Vector3> CalculatePlatforms(PathNode room) {

        List<Vector3> platformLocationsOut = new List<Vector3>();
        List<List<GameObject>> roomPlatformRows = new List<List<GameObject>>();

        int verticalPlatformDistance = 5;
        int platformWidth = 5;
        int roomHeight = room.RoomDimensions.y;
        int roomWidth = room.RoomDimensions.x;
        int groundLevel = room.RoomTopLeftCorner.x;
        int wallSpace = 5;
        int platformSpace = roomHeight - 2 * wallSpace;

        int count = 1;

        for (int i = groundLevel + verticalPlatformDistance; i < groundLevel + roomWidth - 3; i += verticalPlatformDistance) {

            int middleSpace = platformSpace / platformWidth;
            string platformData = count % 2 == 1 ? "0" : "1";

            if (middleSpace == 0) {
                platformData = count % 2 == 1 ? "01" : "10";
            }
            else if (middleSpace > 0 && middleSpace < 3) {
                for (int j = 0; j < middleSpace; j++) {
                    platformData += count % 2 == 1 ? "1" : "0";
                }
                platformData += count % 2 == 1 ? "0" : "1";
            }
            else {
                for (int j = 0; j < middleSpace; j++) {
                    platformData += count % 2 == 1 ? "1" : "0";
                }
                platformData += count % 2 == 1 ? "0" : "1";
            }

            //01110 -> 10001 -> 

            //add or remove random
            System.Random rand = new System.Random();
            int randomIndex = rand.Next(1, platformData.Length - 1);
            char charToCheck = platformData[randomIndex];

            if (platformData[randomIndex - 1] == charToCheck && platformData[randomIndex + 1] == charToCheck) {
                string newPlatformData = platformData.Substring(0, randomIndex);
                newPlatformData += charToCheck == '0' ? "1" : "0";
                newPlatformData += platformData.Substring(randomIndex + 1);
                platformData = newPlatformData;
            }
            List<GameObject> lineOfPlatforms = new List<GameObject>();

            platformLocationsOut.AddRange(SpawnLineOfPlatforms(platformData, i, room, platformWidth, out lineOfPlatforms));

            count++;

            roomPlatformRows.Add(lineOfPlatforms);
        }

        LinkPlatformRows(roomPlatformRows);

        return platformLocationsOut;
    }
    //
    List<Vector3> SpawnLineOfPlatforms(string lineData, int x, PathNode room, int platformWidth, out List<GameObject> lineOfPlatformsOut) {

        //Helpers
        int numSpaces = 0;
        List<Vector3> platformLocationsOut = new List<Vector3>();
        lineOfPlatformsOut = new List<GameObject>();
        Vector3 spawnPos = new Vector3();
        float totalOffset = 0;

        //Validate linedata 
        foreach (char c in lineData.ToCharArray()) {
            if (c != '0' && c != '1') {
                return platformLocationsOut;
            }
            if (c == '0') {

                numSpaces++;
            }
        }

        //Setup space offset
        int extraSpace = room.RoomDimensions.y % 5;
        float spaceOffsetPerCell;
        if (numSpaces == 0) {
            spaceOffsetPerCell = 0;
        }
        else {
            spaceOffsetPerCell = (float) extraSpace / numSpaces;
        }

        //Debug.Log("Number \"cells\" without a platform: " + numSpaces);
        //Debug.Log("Room size remainder: " + extraSpace);
        //Debug.Log("/Cell offset " + spaceOffsetPerCell);
        //Debug.Log("=============================================");

        //Loop through line data
        for (int i = 0; i < lineData.Length; i++) {
            //Skips spaces (represented by '0')
            if (lineData[i] == '0') {
                totalOffset += spaceOffsetPerCell;
                continue;
            }

            //Set spawn position
            spawnPos = new Vector3(x, -0.5f, room.RoomTopLeftCorner.y - (i * platformWidth) - totalOffset - platformWidth / 2f);

            //Instantiate and setup
            GameObject platform = Instantiate(platformPrefab, spawnPos, Quaternion.Euler(0, 0, 0));
            platform.transform.parent = platformParent;
            SetAllChildrenToLayer(platform, 9);// Set to Ground layer

            //Add to output variables
            platformLocationsOut.Add(spawnPos);
            lineOfPlatformsOut.Add(platform);
        }

        //Link Navmeshes on line of platforms
        LinkPlatformLine(lineOfPlatformsOut);

        return platformLocationsOut;
    }
    //
    void LinkPlatformLine(List<GameObject> platformLine) {

        for (int i = 0; i < platformLine.Count - 1; i++) {
            // Calculate the distance between platforms
            float dist = Vector3.Distance(platformLine[i].transform.position, platformLine[i + 1].transform.position);

            //Ensure check is only running if there is a gap with neighbor
            if (dist > 5.5 && dist < NavMeshLinkJumpDistance) { // 5 is the width of the platform
                GameObject startPlatform = platformLine[i].transform.GetChild(0).gameObject;
                GameObject endPlatform = platformLine[i + 1].transform.GetChild(0).gameObject;

                // Add NavMeshLink component
                NavMeshLink link = startPlatform.AddComponent<NavMeshLink>();

                //Set up link points (RELATIVE TO GAMEOBJECT WORLD POSITION)
                Vector3 localP1 = new Vector3(-0.5f, 0.1f, 2.5f);
                Vector3 localP2 = new Vector3(dist - 4, 0.1f, 2.5f);

                // Link the start and end positions to the NavMeshLink
                link.startPoint = localP1;
                link.endPoint = localP2;
                link.bidirectional = true;
                link.autoUpdate = true;

                // Enable and apply the link
                link.enabled = false;
                link.enabled = true;
            }
        }
    }
    //
    void LinkPlatformRows(List<List<GameObject>> roomPlatformRows) {
        //Loop through all rows except bottom (lowest row on cam)
        for (int i = roomPlatformRows.Count - 1; i >= 0; i--) {
            //Loop through all platforms in row
            for (int j = 0; j < roomPlatformRows[i].Count; j++) {

                //Helpers
                GameObject platform = roomPlatformRows[i][j];
                Vector3 wallCheckOrigin = new Vector3(platform.transform.position.x + 0.5f, platform.transform.position.y + 2.5f, platform.transform.position.z);
                bool doRightSideCheck = true;
                bool doLeftSideCheck = true;


                //DEV ONLY - DEBUG ORBS
                //GameObject go = new GameObject("Testing platform locations", typeof(MeshFilter), typeof(MeshRenderer));
                //go.GetComponent<MeshFilter>().mesh = gameManager.debugObjectMesh;
                //go.GetComponent<MeshRenderer>().material = gameManager.debugMaterial;
                //go.transform.position = wallCheckOrigin;
                //go.transform.parent = devParent;

                //Test if against right wall
                if (j == roomPlatformRows[i].Count - 1) {
                    //Right wall check
                    Vector3 wallCheckEnd = new Vector3(0, 0, -3);

                    Ray ray = new Ray(wallCheckOrigin, wallCheckEnd.normalized);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 3)) {
                        //Flag
                        doRightSideCheck = false;

                        //DEV ONLY
                        //go.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1);
                        //Debug.DrawRay(wallCheckOrigin, wallCheckEnd, Color.blue, 25);
                    }
                }
                //Test if against left wall
                else if (j == 0) {
                    //left wall check
                    Vector3 wallCheckEnd = new Vector3(0, 0, 3);

                    Ray ray = new Ray(wallCheckOrigin, wallCheckEnd.normalized);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 3)) {
                        //Flag
                        doLeftSideCheck = false;

                        //DEV ONLY
                        //go.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1);
                        //Debug.DrawRay(wallCheckOrigin, wallCheckEnd, Color.blue, 25);
                    }
                }

                //Check down right
                if (doRightSideCheck) {
                    //Flag
                    bool doLink = true;

                    //Ensure all platforms but last in row
                    if (j != roomPlatformRows[i].Count - 1) {
                        //Helpers
                        GameObject rightNeighbor = roomPlatformRows[i][j + 1];
                        float dist = Vector3.Distance(platform.transform.position, rightNeighbor.transform.position);

                        //Ensure check is only running if there is a gap with neighbor
                        if (dist <= 5.5f) {
                            doLink = false;
                        }
                    }


                    //If there is a gap, check for platforms below
                    if (doLink) {

                        //Helpers
                        Vector3 rayOrigin = new Vector3(platform.transform.position.x - 1, platform.transform.position.y + 2.5f, platform.transform.position.z - 2.5f);
                        Vector3 rayEnd = new Vector3(-4.5f, 0, -2f);
                        Ray ray = new Ray(rayOrigin, rayEnd.normalized);
                        RaycastHit hit;

                        //DEV ONLY
                        //GameObject testGO = new GameObject("Testing Right Ray Origin", typeof(MeshFilter), typeof(MeshRenderer));
                        //testGO.GetComponent<MeshFilter>().mesh = gameManager.debugObjectMesh;
                        //testGO.GetComponent<MeshRenderer>().material = gameManager.debugMaterial;
                        //testGO.transform.position = rayOrigin;
                        //testGO.transform.parent = devParent;

                        if (Physics.Raycast(ray, out hit, 4.5f)) {
                            //testGO.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1);

                            //add link component
                            NavMeshLink link = platform.transform.GetChild(0).gameObject.AddComponent<NavMeshLink>();

                            //Setup up link points
                            Vector3 localP1 = new Vector3(-0.5f, 0.1f, 2.5f);
                            Vector3 localP2 = new Vector3(2.25f, -5.1f, 2.5f);

                            // Link the start and end positions to the NavMeshLink
                            link.startPoint = localP1;
                            link.endPoint = localP2;
                            link.bidirectional = true;
                            link.autoUpdate = true;

                            // Enable and apply the link
                            link.enabled = false;
                            link.enabled = true;

                        }
                        //Debug.DrawRay(rayOrigin, rayEnd, Color.red, 25);
                    }
                }

                //Check down left
                if (doLeftSideCheck) {
                    //Flag
                    bool doLink = true;
                    //Ensure all platforms but first in row
                    if (j != 0) {
                        //Helpers
                        GameObject LeftNeighbor = roomPlatformRows[i][j - 1];
                        float dist = Vector3.Distance(platform.transform.position, LeftNeighbor.transform.position);

                        //Ensure check is only running if there is a gap with neighbor
                        if (dist <= 5.5f) {
                            doLink = false;
                        }
                    }


                    //If there is a gap, check for platforms below
                    if (doLink) {

                        //Helpers
                        Vector3 rayOrigin = new Vector3(platform.transform.position.x - 1, platform.transform.position.y + 2.5f, platform.transform.position.z + 2.5f);
                        Vector3 rayEnd = new Vector3(-4.5f, 0, 2f);
                        Ray ray = new Ray(rayOrigin, rayEnd.normalized);
                        RaycastHit hit;

                        //DEV ONLY
                        //GameObject testGO = new GameObject("Testing Left Ray Origin", typeof(MeshFilter), typeof(MeshRenderer));
                        //testGO.GetComponent<MeshFilter>().mesh = gameManager.debugObjectMesh;
                        //testGO.GetComponent<MeshRenderer>().material = gameManager.debugMaterial;
                        //testGO.transform.position = rayOrigin;
                        //testGO.transform.parent = devParent;


                        if (Physics.Raycast(ray, out hit, 4.5f)) {
                            //testGO.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1);

                            //add link component
                            NavMeshLink link = platform.transform.GetChild(0).gameObject.AddComponent<NavMeshLink>();

                            //Setup up link points
                            Vector3 localP1 = new Vector3(-4.5f, 0.1f, 2.5f);
                            Vector3 localP2 = new Vector3(-7.25f, -5.1f, 2.5f); ;

                            // Link the start and end positions to the NavMeshLink
                            link.startPoint = localP1;
                            link.endPoint = localP2;
                            link.bidirectional = true;
                            link.autoUpdate = true;


                            // Enable and apply the link
                            link.enabled = false;
                            link.enabled = true;
                        }
                        //Debug.DrawRay(rayOrigin, rayEnd, Color.red, 25);
                    }
                }
            }
        }
    }
    //
    void SetAllChildrenToLayer(GameObject objIn, int layerIndexIn) {
        //Set layer
        objIn.layer = layerIndexIn;

        //Recursively set for children
        foreach (Transform child in objIn.transform) {
            SetAllChildrenToLayer(child.gameObject, layerIndexIn);
        }
    }
    //
    public void SaveSeed() {
        //RandomSeedManager.SaveSeedToGoodList(gameManager.lvl1SeedFilePath, levelSeed);
        //RandomSeedManager.SaveSeedToJSON(1, levelSeed, gameManager.levelSeedDataFilePath);
        RandomSeedManager.SaveSeedToJSON(gameManager.CurrentLevel, levelSeed);
    }
}

public struct WallData {

    public Vector3Int position;
    public WallDirection direction;
    public bool isCorridor;

    public WallData(Vector3Int positionIn, WallDirection directionIn, bool isCorridorIn) {
        position = positionIn;
        direction = directionIn;
        isCorridor = isCorridorIn;
    }
}

public enum WallDirection {
    LEFT = 0,
    UP = 1,
    RIGHT = 2,
    DOWN = 3
}

//using System;
//using System.Collections.Generic;
//using Unity.AI.Navigation;
//using UnityEngine;

//public class DungeonCreator : MonoBehaviour {

//    [Header("Dungeon Size Settings")]
//    public int dungeonHeight;
//    public int dungeonWidth;
//    public int roomHeightMin;
//    public int roomWidthMin;
//    public int corridorSize;

//    [Header("Generator Settings")]
//    public bool generateOnLoad;
//    public bool debugMode;
//    public bool dungeonFlatMode;
//    public int maxIterations;
//    [Range(0.0f, 0.3f)]
//    public float roomBottomCornerModifier;
//    [Range(0.7f, 1f)]
//    public float roomTopCornerModifier;
//    [Range(0, 2)]
//    public int roomOffset;
//    [Range(0.0f, 1.0f)]
//    public float wallDecorationFrequency;
//    [Range(0.0f, 1.0f)]
//    public float floorDecorationFrequency;

//    [Header("Parent References")]
//    public Transform roomParent;
//    public Transform corridorParent;
//    public Transform dungeonParent;
//    public Transform pathNodeParent;
//    public Transform wallParent;
//    public Transform maskParent;
//    public Transform platformParent;
//    public Transform decorationParent;
//    public Transform enemiesParent;

//    [Header("Prefab References")]
//    public GameObject wallHorizontal;
//    public GameObject wallVertical;
//    public GameObject maskPrefab;
//    public GameObject playerPrefab;
//    public GameObject bossPrefab;
//    public GameObject corridorEffect;
//    public GameObject platformPrefab;
//    public GameObject castleWall5x5Prefab;
//    public GameObject castleWall1x5Prefab;
//    public GameObject castleWall5x5WindowPrefab;
//    public GameObject castleWall5x5DrainPrefab;
//    public GameObject castleWall5x5DoorPrefab;

//    [Header("Misc References")]
//    public Material roomMaterial;
//    public Material corridorMaterial;
//    public Mesh pathNodeMesh;
//    public Material pathNodeBaseMaterial;
//    public Material pathNodeStartMaterial;
//    public Material pathNodeEndMaterial;
//    public LevelDecorations levelDecorations;
//    public LevelEnemies levelEnemies;
//    public NavMeshSurface navMeshSurface;

//    List<WallData> possibleDoorHorizontalPosition;
//    List<WallData> possibleDoorVerticalPosition;
//    List<WallData> possibleWallPosition;
//    List<WallData> possibleFloorCeilingPosition;


//    //**Unity Methods*
//    void Start() {
//        if (generateOnLoad) {
//            RetryGeneration();
//        }



//    }

//    //**Generation Methods**

//    //Main method for making dungeon
//    private void CreateDungeon() {
//        //Create instance of generator script
//        DungeonGenerator generator = new DungeonGenerator(dungeonHeight, dungeonWidth);
//        //Generate list of rooms
//        var listOfRooms = generator.CalculateDungeon(maxIterations,
//                                                   roomHeightMin,
//                                                   roomWidthMin,
//                                                   roomBottomCornerModifier,
//                                                   roomTopCornerModifier,
//                                                   roomOffset);

//        //Instantiate lists
//        possibleDoorHorizontalPosition = new List<WallData>();
//        possibleDoorVerticalPosition = new List<WallData>();
//        possibleWallPosition = new List<WallData>();
//        possibleFloorCeilingPosition = new List<WallData>();


//        //wallParent.parent = transform;

//        //Generate list of corridors
//        var listOfCorridors = generator.CalculateCorridors(corridorSize);

//        //create path nodes
//        CreatePathNodes(listOfRooms, listOfCorridors);

//        //create mesh and object from list of rooms
//        for (int i = 0; i < listOfRooms.Count; i++) {

//            CreateRoomBackground(listOfRooms[i], roomMaterial, roomParent);
//            GenerateWallPositions(listOfRooms[i], roomMaterial, roomParent);
//        }
//        //create mesh and object from list of corridors
//        for (int i = 0; i < listOfCorridors.Count; i++) {

//            CreateCorridorBackground(listOfCorridors[i], corridorMaterial, corridorParent);
//            GenerateWallPositions(listOfCorridors[i], corridorMaterial, corridorParent);
//        }

//        CreateWalls(wallParent);


//        List<Transform> walls = new List<Transform>();


//        for (int i = 0; i < wallParent.childCount; i++) {

//            walls.Add(wallParent.GetChild(i));
//        }

//        //DEV ONLY
//        List<Transform> testList = new List<Transform> {
//            walls[wallParent.childCount-1]
//        };

//        CreateMasks(listOfRooms, listOfCorridors);

//        //RUN PATH FINDER
//        PathFinder pf = new PathFinder();
//        for (int i = 0; i < pf.EndPoints.Count; i++) {
//            pf.EndPoints[i].GetComponent<MeshRenderer>().sharedMaterial = pathNodeEndMaterial;
//        }
//        pf.StartPoint.GetComponent<MeshRenderer>().sharedMaterial = pathNodeStartMaterial;


//        foreach (PathNode node in pf.PathNodes) {
//            //Create platforms
//            if (node.Type == PathNodeType.ROOM) {
//                PlacePlatforms(node);
//            }
//            //create corridor effects
//            if (node.Type == PathNodeType.CORRIDOR) {
//                GameObject effect = Instantiate(corridorEffect, corridorParent);
//                CorridorEffectController cec = effect.GetComponent<CorridorEffectController>();

//                if (node.Direction == Direction.VERTICAL) {
//                    effect.transform.position = new Vector3(node.RoomTopLeftCorner.x + (corridorSize / 2) + .5f, 2.5f, node.RoomTopLeftCorner.y - 1f);
//                    effect.transform.localRotation = Quaternion.Euler(0, 90, 0);
//                }
//                else {
//                    effect.transform.position = new Vector3(node.RoomTopLeftCorner.x + 1f, 2.5f, node.RoomTopLeftCorner.y - (corridorSize / 2) - .5f);
//                    effect.transform.localRotation = Quaternion.Euler(0, 0, 0);

//                }

//                cec.SetupEffect(effect.transform.localPosition, node.Direction, node.Direction == Direction.VERTICAL ? node.RoomDimensions.y - 2f : node.RoomDimensions.x - 2f);
//            }
//        }

//        foreach (PathNode node in pf.PathNodes) {
//            if (node.Type == PathNodeType.CORRIDOR && node.Direction == Direction.HORTIZONTAL) {
//                //use rays to check left (bottom) side

//                Vector3 checkOrigin = new Vector3(node.RoomTopLeftCorner.x, 2.5f, node.RoomTopLeftCorner.y - (node.RoomDimensions.y / 2f));

//                RaycastHit hitInfo = new RaycastHit();
//                Ray rayLeft = new Ray(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z + 5) - checkOrigin);
//                Ray rayCenter = new Ray(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z) - checkOrigin);
//                Ray rayRight = new Ray(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z - 5) - checkOrigin);

//                bool hitLeft = Physics.Raycast(rayLeft, out hitInfo, 9.4f);
//                bool hitCenter = Physics.Raycast(rayCenter, out hitInfo, 8f);
//                bool hitRight = Physics.Raycast(rayRight, out hitInfo, 9.4f);


//                //Debug.Log("Corridor(" + node.name + ") platform check - Left) " + hitLeft + ", Center) " + hitCenter + ", Right) " + hitRight);

//                if (!hitLeft && !hitCenter && !hitRight) {
//                    GameObject platform = Instantiate(platformPrefab, new Vector3(node.RoomTopLeftCorner.x - 5, -0.5f, node.RoomTopLeftCorner.y), Quaternion.Euler(0, 0, 0), platformParent);
//                    platform.name = "SAFETY PLATFORM";
//                }

//                if (debugMode && dungeonFlatMode) {
//                    GameObject test = new GameObject("Just Testing");
//                    test.transform.position = checkOrigin;
//                    test.transform.parent = corridorParent;

//                    Debug.DrawLine(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z), Color.blue, 60);
//                    Debug.DrawLine(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z + 5), Color.blue, 60);
//                    Debug.DrawLine(checkOrigin, new Vector3(checkOrigin.x - 7, checkOrigin.y, checkOrigin.z - 5), Color.blue, 60);
//                }
//            }
//        }


//        PlacePlayer(pf.StartPoint);

//        foreach (PathNode room in pf.PathNodes) {
//            if (room.Type == PathNodeType.ROOM && room != pf.EndPoints[pf.EndPoints.Count - 1]) {
//                PlaceEnemies(room);
//            }
//        }

//        PlaceBoss(pf.EndPoints[pf.EndPoints.Count - 1]);



//        if (!dungeonFlatMode) {
//            //DEV - rotate parent to show vertically
//            dungeonParent.rotation = Quaternion.Euler(0, 90, 90);
//            dungeonParent.position = new Vector3(0, 0, -0.5f);
//        }


//        if (debugMode) {
//            for (int i = 0; i < pf.Path.Count - 1; i++) {
//                Debug.DrawLine(pf.Path[i].transform.position, pf.Path[i + 1].transform.position, Color.green, 60);
//            }


//        }

//    }
//    //
//    public void RetryGeneration() {

//        ClearDungeon();

//        //create new dungeon
//        CreateDungeon();
//    }

//    //**Utility Methods**
//    public void ClearDungeon() {
//        //get all rooms, corridors, walls and masks
//        Transform[] roomchildren = roomParent.GetComponentsInChildren<Transform>(true);
//        Transform[] corridorchildren = corridorParent.GetComponentsInChildren<Transform>(true);
//        Transform[] wallchildren = wallParent.GetComponentsInChildren<Transform>(true);
//        Transform[] maskchildren = maskParent.GetComponentsInChildren<Transform>(true);
//        Transform[] pathNodeChildren = pathNodeParent.GetComponentsInChildren<Transform>(true);
//        Transform[] platformChildren = platformParent.GetComponentsInChildren<Transform>(true);
//        Transform[] decorationChildren = decorationParent.GetComponentsInChildren<Transform>(true);
//        Transform[] enemiesChildren = enemiesParent.GetComponentsInChildren<Transform>(true);

//        //reset dungeon parent rotation
//        dungeonParent.rotation = Quaternion.Euler(0, 0, 0);
//        dungeonParent.position = new Vector3(0, 0, 0);

//        //Destroy room objects
//        for (int i = roomchildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(roomchildren[i].gameObject);
//        }
//        //Destroy corridor objects
//        for (int i = corridorchildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(corridorchildren[i].gameObject);
//        }
//        //Destroy wall objects
//        for (int i = wallchildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(wallchildren[i].gameObject);
//        }
//        //Destroy mask objects
//        for (int i = maskchildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(maskchildren[i].gameObject);
//        }
//        //Destroy Pathnode objects
//        for (int i = pathNodeChildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(pathNodeChildren[i].gameObject);
//        }
//        //Destroy platform objects
//        for (int i = platformChildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(platformChildren[i].gameObject);
//        }
//        //Destroy decoration objects
//        for (int i = decorationChildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(decorationChildren[i].gameObject);
//        }
//        //Destroy enemies objects
//        for (int i = enemiesChildren.Length - 1; i > 0; i--) {
//            DestroyImmediate(enemiesChildren[i].gameObject);
//        }
//    }

//    //
//    private void CreateWalls(Transform wallParent) { //generate all walls
//        foreach (WallData wallPosition in possibleWallPosition) {
//            CreateWall(wallParent, wallPosition, wallVertical);
//        }

//        foreach (WallData wallPosition in possibleFloorCeilingPosition) {
//            CreateWall(wallParent, wallPosition, wallHorizontal);
//        }
//    }

//    //
//    private void CreateWall(Transform wallParentIn, WallData wallDataIn, GameObject wallPrefabIn) { //generate a single wall

//        if (wallPositionIn.direction == WallDirection.LEFT || wallPositionIn.direction == WallDirection.RIGHT) {
//            //Walls facing East or West (Vertical walls)
//            GameObject go = Instantiate(wallPrefabIn, new Vector3(wallPositionIn.position.x + 1, wallPositionIn.position.y - 0.01f, wallPositionIn.direction == WallDirection.LEFT ? wallPositionIn.position.z + 0.25f : wallPositionIn.position.z - 0.25f), Quaternion.Euler(0, 0, 0), wallParentIn);
//        }
//        else {
//            //Walls facing North or South (Horizontal walls)
//            GameObject go = Instantiate(wallPrefabIn, new Vector3(wallPositionIn.position.x, wallPositionIn.position.y - 0.01f, wallPositionIn.position.z), Quaternion.Euler(90, 90, 0), wallParentIn);
//        }


//        //GameObject go = Instantiate(wallPrefabIn, new Vector3(wallPositionIn.position.x, wallPositionIn.position.y - .01f, wallPositionIn.position.z), Quaternion.Euler(90, 90, 0), wallParentIn);

//        //float newRotation = (int) wallPositionIn.direction * 90f;

//        //GameObject rotationobject = go.transform.GetChild(0).gameObject;
//        //Quaternion rotation = go.transform.rotation;
//        //rotationobject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y + newRotation, rotation.z);
//    }

//    //
//    private void CreateMasks(List<RoomNode> roomsIn, List<CorridorNode> corridorsIn) { //Generate masking meshes

//        MaskGenerator mg = GetComponent<MaskGenerator>();

//        mg.GenerateMaskMesh(roomsIn, corridorsIn, dungeonWidth, dungeonHeight);

//    }

//    //
//    void CreateRoomBackground(RoomNode roomNode, Material materialIn, Transform newObjectParent) {

//        //Create mesh vertices
//        Vector3 bottomLeftVertice = new Vector3(roomNode.BottomLeftAreaCorner.x, 0, roomNode.BottomLeftAreaCorner.y);
//        Vector3 bottomRightVertice = new Vector3(roomNode.TopRightAreaCorner.x, 0, roomNode.BottomRightAreaCorner.y);
//        Vector3 topLeftVertice = new Vector3(roomNode.BottomLeftAreaCorner.x, 0, roomNode.TopRightAreaCorner.y);
//        Vector3 topRightVertice = new Vector3(roomNode.TopRightAreaCorner.x, 0, roomNode.TopRightAreaCorner.y);

//        GameObject roomParent = new GameObject("Room (" + topLeftVertice.x + ", " + topLeftVertice.z + ")");



//        for (int i = (int) bottomLeftVertice.z; i < topLeftVertice.z - 4; i += 5) {
//            for (int j = (int) bottomLeftVertice.x; j < bottomRightVertice.x - 4; j += 5) {

//                GameObject prefab = castleWall5x5Prefab;

//                float randomFloat = UnityEngine.Random.value;

//                if (randomFloat < wallDecorationFrequency) {
//                    int choice = UnityEngine.Random.Range(0, 2);
//                    if (choice == 0) {
//                        prefab = castleWall5x5DrainPrefab;
//                    }
//                    else {
//                        prefab = castleWall5x5WindowPrefab;
//                    }
//                }

//                Vector3 wallPos = new Vector3(j, -0.25f, i);
//                GameObject wallPiece = Instantiate(prefab, wallPos, Quaternion.Euler(90, 90, 0), roomParent.transform);
//            }

//            int roomWidth = (int) Math.Abs(bottomLeftVertice.x - bottomRightVertice.x);

//            if ((roomWidth % 5) != 0) {
//                for (int j = (int) i; j < i + 5; j++) {
//                    Vector3 wallPos = new Vector3(bottomRightVertice.x - (roomWidth % 5), -0.25f, j);
//                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), roomParent.transform);
//                }
//            }
//        }
//        int roomHeight = (int) Math.Abs(bottomLeftVertice.z - topLeftVertice.z);
//        if ((roomHeight % 5) != 0) {
//            for (int i = (int) (topLeftVertice.z - (roomHeight % 5)); i < topLeftVertice.z; i++) {
//                for (int j = (int) topLeftVertice.x; j < topRightVertice.x; j += 5) {
//                    Vector3 wallPos = new Vector3(j, -0.25f, i);
//                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), roomParent.transform);
//                }
//            }
//        }




//        roomParent.transform.parent = newObjectParent;


//        //Vector3[] vertices = new Vector3[] {
//        //        //ORDER IMPORTANT HERE FOR UNITY
//        //        topLeftVertice,
//        //        topRightVertice,
//        //        bottomLeftVertice,
//        //        bottomRightVertice
//        //    };

//        ////create uv's for mesh
//        //Vector2[] uvs = new Vector2[vertices.Length];
//        //for (int i = 0; i < uvs.Length; i++) {
//        //    uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
//        //}

//        ////create triangles
//        //int[] triangles = new int[] {
//        //        //ORDER MATTERS HERE FOR UNITY
//        //        0,
//        //        1,
//        //        2,
//        //        2,
//        //        1,
//        //        3
//        //    };

//        ////create mesh
//        //Mesh mesh = new Mesh();
//        ////set attributes
//        //mesh.vertices = vertices;
//        //mesh.uv = uvs;
//        //mesh.triangles = triangles;

//        ////create new gameObject with a given name and components
//        //GameObject dungeonFloor = new GameObject("Mesh" + node.distanceFromOrigin, typeof(MeshFilter), typeof(MeshRenderer));




//        ////set transforms
//        //dungeonFloor.transform.position = Vector3.zero;
//        //dungeonFloor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
//        //dungeonFloor.transform.localScale = Vector3.one;
//        ////set other components
//        //dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
//        //dungeonFloor.GetComponent<MeshRenderer>().material = materialIn;
//        ////set parent object
//        //dungeonFloor.transform.parent = newObjectParent;

//        //set bottom left corner gameobject as child of mesh
//        roomNode.bottomRightCornerObject.transform.parent = roomParent.transform;



//        ////GENERATE WALL POSITIONS
//        ////Right side of room
//        //for (int row = (int) bottomLeftVertice.x; row < (int) bottomRightVertice.x; row++) {
//        //    var wallPosition = new Vector3(row, 0, bottomLeftVertice.z);
//        //    AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.RIGHT);
//        //}
//        ////Left side of room
//        //for (int row = (int) topLeftVertice.x; row < (int) node.TopRightAreaCorner.x; row++) {
//        //    var wallPosition = new Vector3(row, 0, topRightVertice.z);
//        //    AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.LEFT);
//        //}
//        ////Bottom of Room
//        //for (int col = (int) bottomLeftVertice.z; col < (int) topLeftVertice.z; col++) {
//        //    var wallPosition = new Vector3(bottomLeftVertice.x, 0, col);
//        //    AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.DOWN);
//        //}
//        ////top of room
//        //for (int col = (int) bottomRightVertice.z; col < (int) topRightVertice.z; col++) {
//        //    var wallPosition = new Vector3(bottomRightVertice.x, 0, col);
//        //    AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.UP);
//        //}

//    }
//    //
//    void CreateCorridorBackground(CorridorNode corridorNode, Material materialIn, Transform newObjectParent) {

//        //Create mesh vertices
//        Vector3 bottomLeftVertice = new Vector3(corridorNode.BottomLeftAreaCorner.x, 0, corridorNode.BottomLeftAreaCorner.y);
//        Vector3 bottomRightVertice = new Vector3(corridorNode.TopRightAreaCorner.x, 0, corridorNode.BottomRightAreaCorner.y);
//        Vector3 topLeftVertice = new Vector3(corridorNode.BottomLeftAreaCorner.x, 0, corridorNode.TopRightAreaCorner.y);
//        Vector3 topRightVertice = new Vector3(corridorNode.TopRightAreaCorner.x, 0, corridorNode.TopRightAreaCorner.y);

//        GameObject corridorParent = new GameObject("Corridor (" + topLeftVertice.x + ", " + topLeftVertice.z + ")");



//        if (corridorNode.Direction == Direction.HORTIZONTAL) {

//            for (int i = (int) bottomLeftVertice.x; i < bottomRightVertice.x - 4; i += 5) {
//                Vector3 wallPos = new Vector3(i, -0.25f, bottomRightVertice.z);
//                GameObject wallPiece = Instantiate(castleWall5x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
//            }

//            int corridorWidth = (int) Math.Abs(bottomLeftVertice.x - bottomRightVertice.x);

//            if ((corridorWidth % 5) != 0) {
//                for (int i = (int) bottomRightVertice.z; i < bottomRightVertice.z + 5; i++) {
//                    Vector3 wallPos = new Vector3(bottomRightVertice.x - (corridorWidth % 5), -0.25f, i);
//                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
//                }
//            }
//        }
//        else {
//            for (int i = (int) bottomLeftVertice.z; i < topLeftVertice.z - 4; i += 5) {
//                Vector3 wallPos = new Vector3(bottomLeftVertice.x, -0.25f, i);
//                GameObject wallPiece = Instantiate(castleWall5x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
//            }

//            int corridorHeight = (int) Math.Abs(bottomLeftVertice.z - topLeftVertice.z);

//            if ((corridorHeight % 5) != 0) {
//                for (int i = (int) topLeftVertice.z - (corridorHeight % 5); i < topLeftVertice.z; i++) {
//                    Vector3 wallPos = new Vector3(bottomLeftVertice.x, -0.25f, i);
//                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), corridorParent.transform);
//                }
//            }
//        }
//        corridorParent.transform.parent = newObjectParent;

//        //set bottom left corner gameobject as child of mesh
//        corridorNode.bottomRightCornerObject.transform.parent = corridorParent.transform;

//    }


//    //Generate wall positions
//    void GenerateWallPositions(Node node, Material materialIn, Transform newObjectParent) {
//        bool isCorridor = node is CorridorNode;
//        //Create mesh vertices
//        Vector3 bottomLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.BottomLeftAreaCorner.y);
//        Vector3 bottomRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.BottomRightAreaCorner.y);
//        Vector3 topLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.TopRightAreaCorner.y);
//        Vector3 topRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.TopRightAreaCorner.y);

//        //GENERATE WALL POSITIONS
//        //Right side of room
//        for (int row = (int) bottomLeftVertice.x; row < (int) bottomRightVertice.x; row++) {
//            var wallPosition = new Vector3(row, 0, bottomLeftVertice.z);
//            AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.RIGHT);
//        }
//        //Left side of room
//        for (int row = (int) topLeftVertice.x; row < (int) node.TopRightAreaCorner.x; row++) {
//            var wallPosition = new Vector3(row, 0, topRightVertice.z);
//            AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.LEFT);
//        }
//        //Bottom of Room
//        for (int col = (int) bottomLeftVertice.z; col < (int) topLeftVertice.z; col++) {
//            var wallPosition = new Vector3(bottomLeftVertice.x, 0, col);
//            AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.DOWN);
//        }
//        //top of room
//        for (int col = (int) bottomRightVertice.z; col < (int) topRightVertice.z; col++) {
//            var wallPosition = new Vector3(bottomRightVertice.x, 0, col);
//            AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.UP);
//        }

//    }

//    //
//    private List<GameObject> CreatePathNodes(List<RoomNode> listOfRooms, List<CorridorNode> listOfCorridors) {

//        List<GameObject> pathNodesObjectsOut = new List<GameObject>();

//        for (int i = 0; i < listOfRooms.Count; i++) {
//            Vector2Int centerPoint = (listOfRooms[i].TopLeftAreaCorner + listOfRooms[i].BottomRightAreaCorner) / 2;

//            //create path node object
//            GameObject pathNodeObject = new GameObject("PathNode - room " + i, typeof(MeshFilter), typeof(MeshRenderer));


//            if (debugMode) {
//                pathNodeObject.GetComponent<MeshFilter>().mesh = pathNodeMesh;
//                pathNodeObject.GetComponent<MeshRenderer>().material = pathNodeBaseMaterial;
//            }

//            //add pathnode script 
//            PathNode pathNode = pathNodeObject.AddComponent<PathNode>();
//            pathNode.Type = PathNodeType.ROOM;
//            pathNode.Direction = Direction.VERTICAL;
//            pathNode.RoomDimensions = new Vector2Int(listOfRooms[i].Width, listOfRooms[i].Length);
//            pathNode.RoomTopLeftCorner = listOfRooms[i].TopLeftAreaCorner;


//            pathNodeObject.transform.parent = pathNodeParent;

//            pathNodeObject.transform.position = new Vector3(centerPoint.x, 5, centerPoint.y);

//            //PATH NODES
//            listOfRooms[i].pathNode = pathNodeObject;
//        }
//        for (int i = 0; i < listOfCorridors.Count; i++) {
//            Vector2Int centerPoint = (listOfCorridors[i].TopLeftAreaCorner + listOfCorridors[i].BottomRightAreaCorner) / 2;

//            //create path node object
//            GameObject pathNodeObject = new GameObject("PathNode - corridor " + i, typeof(MeshFilter), typeof(MeshRenderer));


//            if (debugMode) {
//                pathNodeObject.GetComponent<MeshFilter>().mesh = pathNodeMesh;
//                pathNodeObject.GetComponent<MeshRenderer>().material = pathNodeBaseMaterial;
//            }

//            //add pathnode script 
//            PathNode pathNode = pathNodeObject.AddComponent<PathNode>();
//            pathNode.Type = PathNodeType.CORRIDOR;
//            pathNode.Direction = listOfCorridors[i].Direction;
//            pathNode.RoomDimensions = new Vector2Int(listOfCorridors[i].Width, listOfCorridors[i].Length);
//            pathNode.RoomTopLeftCorner = listOfCorridors[i].TopLeftAreaCorner;

//            pathNodeObject.transform.parent = pathNodeParent;
//            pathNodeObject.transform.position = new Vector3(centerPoint.x, 5, centerPoint.y);

//            //PATH NODES
//            listOfCorridors[i].pathNode = pathNodeObject;


//            List<GameObject> adjacentStructuresPathNodeObjects = new List<GameObject> {
//                listOfCorridors[i].Structure1.pathNode,
//                listOfCorridors[i].Structure2.pathNode
//            };


//            //set corridor neighors with rooms
//            pathNode.neighbors = adjacentStructuresPathNodeObjects;

//            //Set corridor structures to have the corridor as a neighbor
//            listOfCorridors[i].Structure1.pathNode.GetComponent<PathNode>().neighbors.Add(pathNodeObject);


//            listOfCorridors[i].Structure2.pathNode.GetComponent<PathNode>().neighbors.Add(pathNodeObject);

//        }
//    }

//    //
//    private void AddWallPositionToList(Vector3 wallPositionIn, List<WallData> wallListIn, List<WallData> doorListIn, WallDirection directionIn, bool isCorridorIn) { //sets positions of walls to proper list

//        //get point from wall position
//        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
//        WallData temp = new WallData(point, direction);
//        int index = -1;

//        //Check if wall list already contains this point
//        for (int i = 0; i < wallList.Count; i++) {
//            if (wallList[i].position == temp.position) {
//                index = i;
//                break;
//            }
//        }

//        //Test index need to be a door
//        if (index != -1) {
//            wallList.RemoveAt(index);
//            doorList.Add(temp);
//        }
//        else {
//            //add to wall list
//            wallList.Add(temp);
//        }
//    }
//    //
//    public void PlacePlayer(PathNode spawnRoomPathNode) {
//        Vector3 spawnPos = Vector3.zero;

//        if ((spawnRoomPathNode.RoomTopLeftCorner.y - spawnRoomPathNode.RoomDimensions.y / 2) > dungeonWidth / 2) {
//            //Spawn on "left" side of room
//            spawnPos = new Vector3(spawnRoomPathNode.RoomTopLeftCorner.x + .1f, 0, spawnRoomPathNode.RoomTopLeftCorner.y - 2);

//            //Rotate player
//            playerPrefab.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 270, 0);
//            playerPrefab.GetComponent<PlayerController>().IsFacingLeft = false;

//        }
//        else {
//            //spawn on "right" side of room
//            spawnPos = new Vector3(spawnRoomPathNode.RoomTopLeftCorner.x + .1f, 0, spawnRoomPathNode.RoomTopLeftCorner.y - spawnRoomPathNode.RoomDimensions.y + 1);

//            //Rotate player
//            playerPrefab.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 90, 0);
//            playerPrefab.GetComponent<PlayerController>().IsFacingLeft = true;
//        }


//        //places player
//        playerPrefab.transform.position = spawnPos;

//        SpawnDoorAtPlayerStart();
//        if (!dungeonFlatMode) {
//            //rotates player around origin to account for level rotation
//            playerPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
//            playerPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
//        }

//        playerPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
//        playerPrefab.transform.position = new Vector3(playerPrefab.transform.position.x + 1, spawnPos.x, playerPrefab.transform.position.z + 1);

//    }

//    //
//    public List<Vector3> PlacePlatforms(PathNode room) {


//        List<Vector3> spawnLocations = new List<Vector3>();
//        int roomArea = room.RoomDimensions.x * room.RoomDimensions.y;
//        int numEnemySpawns = 0;

//        if (roomArea < 250) {
//            numEnemySpawns = 1;
//        }
//        else if (roomArea < 500) {
//            numEnemySpawns = 2;
//        }
//        else if (roomArea < 750) {
//            numEnemySpawns = 3;
//        }
//        else if (roomArea < 1000) {
//            numEnemySpawns = 4;
//        }
//        else if (roomArea < 1250) {
//            numEnemySpawns = 5;
//        }
//        else if (roomArea < 1500) {
//            numEnemySpawns = 6;
//        }
//        else {
//            numEnemySpawns = 7;
//        }

//        //get random enemy spawn points
//        List<int> xLevels = new List<int>();
//        for (int i = 0; i < numEnemySpawns; i++) {
//            //find x values
//            for (int j = room.RoomTopLeftCorner.x; j < room.RoomTopLeftCorner.x + room.RoomDimensions.x - 3; j += 4) {
//                xLevels.Add(j);
//            }

//            bool locationFound = false;
//            float randX = 0;
//            float randZ = 0;

//            while (!locationFound) {
//                randX = xLevels[UnityEngine.Random.Range(0, xLevels.Count)] + 0.15f;
//                randZ = UnityEngine.Random.Range(room.RoomTopLeftCorner.y - room.RoomDimensions.y + 2, room.RoomTopLeftCorner.y - 2);

//                //test if on platform
//                Vector3 spawnPos = new Vector3(randX, 2.5f, randZ);
//                Ray platformCheckRay = new Ray(spawnPos, Vector3.left);

//                if (debugMode) {
//                    Debug.DrawLine(spawnPos, spawnPos + Vector3.left * 2f, Color.red, 60);
//                }

//                if (Physics.Raycast(platformCheckRay, 2f)) {
//                    locationFound = true;
//                    if (debugMode) {
//                        Debug.DrawLine(spawnPos, spawnPos + Vector3.left * 2f, Color.yellow, 60);
//                    }
//                }
//            }

//            if (locationFound) {
//                spawnLocations.Add(new Vector3(randX, 2.5f, randZ));
//            }
//        }

//        foreach (Vector3 spawnPos in spawnLocations) {
//            //get random enemy
//            GameObject enemy = levelEnemies.enemies[UnityEngine.Random.Range(0, levelEnemies.enemies.Count)];

//            enemy = Instantiate(enemy, spawnPos, Quaternion.Euler(0, 0, -90), enemiesParent);
//        }


//    }

//    public void PlaceBoss(PathNode bossRoomPathNode) {
//        Vector3 spawnPos = Vector3.zero;

//        if ((bossRoomPathNode.RoomTopLeftCorner.y - bossRoomPathNode.RoomDimensions.y / 2) > dungeonWidth / 2) {
//            //Spawn on "left" side of room
//            spawnPos = new Vector3(bossRoomPathNode.RoomTopLeftCorner.x + .1f, 0, bossRoomPathNode.RoomTopLeftCorner.y - 2);

//        }
//        else {
//            //spawn on "right" side of room
//            spawnPos = new Vector3(bossRoomPathNode.RoomTopLeftCorner.x + .1f, 0, bossRoomPathNode.RoomTopLeftCorner.y - bossRoomPathNode.RoomDimensions.y + 2);

//        }

//        //places boss
//        bossPrefab.transform.position = spawnPos;
//        if (!dungeonFlatMode) {
//            //rotates boss around origin to account for level rotation
//            bossPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
//            bossPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
//        }

//        bossPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
//        bossPrefab.transform.position = new Vector3(bossPrefab.transform.position.x, spawnPos.x + 3f, bossPrefab.transform.position.z + 1);
//    }

//    public void PlacePlatforms(PathNode room) {

//        int verticalPlatformDistance = 4;
//        int platformWidth = 5;
//        int roomHeight = room.RoomDimensions.y;
//        int roomWidth = room.RoomDimensions.x;
//        int groundLevel = room.RoomTopLeftCorner.x;
//        int wallSpace = 5;
//        int platformSpace = roomHeight - 2 * wallSpace;

//        int count = 1;
//        for (int i = groundLevel + verticalPlatformDistance; i < groundLevel + roomWidth - 3; i += verticalPlatformDistance) {

//            int middleSpace = platformSpace / platformWidth;
//            string platformData = count % 2 == 1 ? "0" : "1";

//            if (middleSpace == 0) {
//                platformData = count % 2 == 1 ? "01" : "10";
//            }
//            else if (middleSpace > 0 && middleSpace < 3) {
//                for (int j = 0; j < middleSpace; j++) {
//                    platformData += count % 2 == 1 ? "1" : "0";
//                }
//                platformData += count % 2 == 1 ? "0" : "1";
//            }
//            else {
//                for (int j = 0; j < middleSpace; j++) {
//                    platformData += count % 2 == 1 ? "1" : "0";
//                }
//                platformData += count % 2 == 1 ? "0" : "1";
//            }

//            //add or remove random
//            System.Random rand = new System.Random();
//            int randomIndex = rand.Next(1, platformData.Length - 1);
//            char charToCheck = platformData[randomIndex];

//            if (platformData[randomIndex - 1] == charToCheck && platformData[randomIndex + 1] == charToCheck) {
//                string newPlatformData = platformData.Substring(0, randomIndex);
//                newPlatformData += charToCheck == '0' ? "1" : "0";
//                newPlatformData += platformData.Substring(randomIndex + 1);
//                platformData = newPlatformData;
//            }

//            SpawnLineOfPlatforms(platformData, i, room, platformWidth);
//            count++;
//        }
//    }
//    //
//    public void SpawnDoorAtPlayerStart() {

//        //Debug.Log("Player Pos: " + playerPrefab.transform.position);

//        Collider[] hits = Physics.OverlapSphere(playerPrefab.transform.position, 15f);
//        GameObject closestGO = null;
//        float closestDistance = Mathf.Infinity;

//        foreach (Collider collider in hits) {
//            GameObject go = collider.gameObject;
//            if (go.name == "CastleWall_5x5(Clone)") {
//                float distance = Vector3.Distance(playerPrefab.transform.position, go.transform.position);
//                if (distance < closestDistance) {
//                    closestDistance = distance;
//                    closestGO = go;
//                }
//            }
//        }

//        //Debug.Log("Closest Distance: " + closestDistance);
//        //Debug.Log("Closest wall: " + closestGO.transform.position);

//        GameObject door = Instantiate(castleWall5x5DoorPrefab, closestGO.transform.parent);
//        door.name = "door";
//        door.transform.position = closestGO.transform.localPosition;
//        door.transform.rotation = Quaternion.Euler(90, 90, 0);
//        closestGO.SetActive(false);
//    }
//    //
//    List<Vector3> SpawnLineOfPlatforms(string lineData, int x, PathNode room, int platformWidth) {

//        List<Vector3> decorationItemLocationsOut = new List<Vector3>();


//        //Helper
//        int numSpaces = 0;

//        //Validate linedata
//        foreach (char c in lineData.ToCharArray()) {
//            if (c != '0' && c != '1') {
//                return;
//            }
//            if (c == '0') {

//                numSpaces++;
//            }
//        }

//        //Setup space offset
//        int extraSpace = room.RoomDimensions.y % 5;
//        float spaceOffsetPerCell;
//        if (numSpaces == 0) {
//            spaceOffsetPerCell = 0;
//        }
//        else {
//            spaceOffsetPerCell = (float) extraSpace / numSpaces;
//        }

//        //Debug.Log("Number \"cells\" without a platform: " + numSpaces);
//        //Debug.Log("Room size remainder: " + extraSpace);
//        //Debug.Log("/Cell offset " + spaceOffsetPerCell);
//        //Debug.Log("=============================================");

//        Vector3 spawnPos = new Vector3();

//        float totalOffset = 0;
//        for (int i = 0; i < lineData.Length; i++) {
//            if (lineData[i] == '0') {
//                totalOffset += spaceOffsetPerCell;
//                continue;
//            }
//            spawnPos = new Vector3(x, -0.5f, room.RoomTopLeftCorner.y - (i * platformWidth) - totalOffset - platformWidth / 2f);

//            GameObject platform = Instantiate(platformPrefab, spawnPos, Quaternion.Euler(0, 0, 0));
//            platform.transform.parent = platformParent;

//            //spawn decoration
//            float roll = UnityEngine.Random.value;

//            if (roll < floorDecorationFrequency) {
//                SpawnDecorationItem(spawnPos);
//            }
//        }
//    }

//    //
//    void SetAllChildrenToLayer(GameObject objIn, int layerIndexIn) {
//        //Set layer
//        objIn.layer = layerIndexIn;

//        GameObject decoration = levelDecorations.DecorationPrefabs[UnityEngine.Random.Range(0, levelDecorations.DecorationPrefabs.Count)];

//        float decorZPos = UnityEngine.Random.Range(-2.5f, 2.5f) + platformSpawnPos.z;
//        Vector3 decorSpawnPos = new Vector3(platformSpawnPos.x + .05f, UnityEngine.Random.Range(0, 2) == 0 ? platformSpawnPos.y + 1f : platformSpawnPos.y + 4.75f, decorZPos);

//        decoration = Instantiate(decoration, decorSpawnPos, Quaternion.Euler(UnityEngine.Random.Range(0, 360), 0, -90), decorationParent);
//    }
//}

//public struct WallData {

//    public Vector3Int position;
//    public WallDirection direction;

//    public WallData(Vector3Int positionIn, WallDirection directionIn) {
//        position = positionIn;
//        direction = directionIn;
//    }
//}

//public enum WallDirection {
//    LEFT = 0,
//    UP = 1,
//    RIGHT = 2,
//    DOWN = 3
//}
