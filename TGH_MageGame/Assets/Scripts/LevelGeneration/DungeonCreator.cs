using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour {

    [Header("Dungeon Size Settings")]
    public int dungeonHeight;
    public int dungeonWidth;
    public int roomHeightMin;
    public int roomWidthMin;
    public int corridorSize;

    [Header("Generator Settings")]
    public bool generateOnLoad;
    public bool debugMode;
    public bool dungeonFlatMode;
    public int maxIterations;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1f)]
    public float roomTopCornerModifier;
    [Range(0, 2)]
    public int roomOffset;
    [Range(0.0f, 1.0f)]
    public float wallDecorationFrequency;

    [Header("Prefab References")]
    public Transform roomParent;
    public Transform corridorParent;
    public Transform dungeonParent;
    public Transform pathNodeParent;
    public Transform wallParent;
    public Transform maskParent;
    public Transform platformParent;
    public GameObject wallHorizontal;
    public GameObject wallVertical;
    public GameObject maskPrefab;
    public GameObject playerPrefab;
    public GameObject bossPrefab;
    public GameObject corridorEffect;
    public GameObject castleWall5x5Prefab;
    public GameObject castleWall1x5Prefab;
    public GameObject castleWall5x5WindowPrefab;
    public GameObject castleWall5x5DrainPrefab;

    [Header("Misc References")]
    public Material roomMaterial;
    public Material corridorMaterial;
    public Mesh pathNodeMesh;
    public Material pathNodeBaseMaterial;
    public Material pathNodeStartMaterial;
    public Material pathNodeEndMaterial;

    List<WallData> possibleDoorHorizontalPosition;
    List<WallData> possibleDoorVerticalPosition;
    List<WallData> possibleWallPosition;
    List<WallData> possibleFloorCeilingPosition;


    //**Unity Methods*
    void Start() {
        if (generateOnLoad) {
            RetryGeneration();
        }



    }

    //**Generation Methods**

    //Main method for making dungeon
    private void CreateDungeon() {
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

        //create path nodes
        CreatePathNodes(listOfRooms, listOfCorridors);

        //create mesh and object from list of rooms
        for (int i = 0; i < listOfRooms.Count; i++) {

            CreateRoomBackground(listOfRooms[i], roomMaterial, roomParent);
            GenerateWallPositions(listOfRooms[i], roomMaterial, roomParent);
        }
        //create mesh and object from list of corridors
        for (int i = 0; i < listOfCorridors.Count; i++) {

            CreateCorridorBackground(listOfCorridors[i], corridorMaterial, corridorParent);
            GenerateWallPositions(listOfCorridors[i], corridorMaterial, corridorParent);
        }

        CreateWalls(wallParent);


        List<Transform> walls = new List<Transform>();


        for (int i = 0; i < wallParent.childCount; i++) {

            walls.Add(wallParent.GetChild(i));
        }

        //DEV ONLY
        List<Transform> testList = new List<Transform> {
            walls[wallParent.childCount-1]
        };

        CreateMasks(listOfRooms, listOfCorridors);

        //RUN PATH FINDER
        PathFinder pf = new PathFinder();
        for (int i = 0; i < pf.EndPoints.Count; i++) {
            pf.EndPoints[i].GetComponent<MeshRenderer>().sharedMaterial = pathNodeEndMaterial;
        }
        pf.StartPoint.GetComponent<MeshRenderer>().sharedMaterial = pathNodeStartMaterial;


        foreach (PathNode node in pf.PathNodes) {
            //Create platforms
            if (node.Type == PathNodeType.ROOM) {
                PlacePlatforms(node);
            }
            //create corridor effects
            if (node.Type == PathNodeType.CORRIDOR) {
                GameObject effect = Instantiate(corridorEffect, corridorParent);
                CorridorEffectController cec = effect.GetComponent<CorridorEffectController>();

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

        }

        PlacePlayer(pf.StartPoint);

        PlaceBoss(pf.EndPoints[pf.EndPoints.Count - 1]);


        if (!dungeonFlatMode) {
            //DEV - rotate parent to show vertically
            dungeonParent.rotation = Quaternion.Euler(0, 90, 90);
            dungeonParent.position = new Vector3(0, 0, -0.5f);
        }


        if (debugMode) {
            for (int i = 0; i < pf.Path.Count - 1; i++) {
                Debug.DrawLine(pf.Path[i].transform.position, pf.Path[i + 1].transform.position, Color.green, 60);
            }
        }

    }
    public void RetryGeneration() {

        ClearDungeon();

        //create new dungeon
        CreateDungeon();
    }

    //**Utility Methods**
    public void ClearDungeon() {
        //get all rooms, corridors, walls and masks
        Transform[] roomchildren = roomParent.GetComponentsInChildren<Transform>(true);
        Transform[] corridorchildren = corridorParent.GetComponentsInChildren<Transform>(true);
        Transform[] wallchildren = wallParent.GetComponentsInChildren<Transform>(true);
        Transform[] maskchildren = maskParent.GetComponentsInChildren<Transform>(true);
        Transform[] pathNodeChildren = pathNodeParent.GetComponentsInChildren<Transform>(true);
        Transform[] platformChildren = platformParent.GetComponentsInChildren<Transform>(true);

        //reset dungeon parent rotation
        dungeonParent.rotation = Quaternion.Euler(0, 0, 0);
        dungeonParent.position = new Vector3(0, 0, 0);

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
    }

    //generate all walls
    private void CreateWalls(Transform wallParent) {
        foreach (WallData wallPosition in possibleWallPosition) {
            CreateWall(wallParent, wallPosition, wallVertical);
        }

        foreach (WallData wallPosition in possibleFloorCeilingPosition) {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }
    }

    //generate a single wall
    private void CreateWall(Transform wallParentIn, WallData wallPositionIn, GameObject wallPrefabIn) {

        if (wallPositionIn.direction == WallDirection.LEFT || wallPositionIn.direction == WallDirection.RIGHT) {
            //Walls facing East or West (Vertical walls)
            GameObject go = Instantiate(wallPrefabIn, new Vector3(wallPositionIn.position.x + 1, wallPositionIn.position.y - 0.01f, wallPositionIn.direction == WallDirection.LEFT ? wallPositionIn.position.z + 0.25f : wallPositionIn.position.z - 0.25f), Quaternion.Euler(0, 0, 0), wallParentIn);
        }
        else {
            //Walls facing North or South (Horizontal walls)
            GameObject go = Instantiate(wallPrefabIn, new Vector3(wallPositionIn.position.x, wallPositionIn.position.y - 0.01f, wallPositionIn.position.z), Quaternion.Euler(90, 90, 0), wallParentIn);
        }


        //GameObject go = Instantiate(wallPrefabIn, new Vector3(wallPositionIn.position.x, wallPositionIn.position.y - .01f, wallPositionIn.position.z), Quaternion.Euler(90, 90, 0), wallParentIn);

        //float newRotation = (int) wallPositionIn.direction * 90f;

        //GameObject rotationobject = go.transform.GetChild(0).gameObject;
        //Quaternion rotation = go.transform.rotation;
        //rotationobject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y + newRotation, rotation.z);
    }


    //Generate masking meshes
    private void CreateMasks(List<RoomNode> roomsIn, List<CorridorNode> corridorsIn) {

        MaskGenerator mg = GetComponent<MaskGenerator>();

        mg.GenerateMaskMesh(roomsIn, corridorsIn, dungeonWidth, dungeonHeight);

    }

    //creates ---
    void CreateRoomBackground(RoomNode roomNode, Material materialIn, Transform newObjectParent) {

        //Create mesh vertices
        Vector3 bottomLeftVertice = new Vector3(roomNode.BottomLeftAreaCorner.x, 0, roomNode.BottomLeftAreaCorner.y);
        Vector3 bottomRightVertice = new Vector3(roomNode.TopRightAreaCorner.x, 0, roomNode.BottomRightAreaCorner.y);
        Vector3 topLeftVertice = new Vector3(roomNode.BottomLeftAreaCorner.x, 0, roomNode.TopRightAreaCorner.y);
        Vector3 topRightVertice = new Vector3(roomNode.TopRightAreaCorner.x, 0, roomNode.TopRightAreaCorner.y);

        GameObject roomParent = new GameObject("Room (" + topLeftVertice.x + ", " + topLeftVertice.z + ")");



        for (int i = (int) bottomLeftVertice.z; i < topLeftVertice.z - 4; i += 5) {
            for (int j = (int) bottomLeftVertice.x; j < bottomRightVertice.x - 4; j += 5) {

                GameObject prefab = castleWall5x5Prefab;

                Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint) System.DateTime.UtcNow.Ticks);

                float randomFloat = rand.NextFloat();


                if (randomFloat < wallDecorationFrequency) {
                    bool choice = rand.NextBool();
                    if (choice) {
                        prefab = castleWall5x5DrainPrefab;
                    }
                    else {
                        prefab = castleWall5x5WindowPrefab;
                    }
                }

                Vector3 wallPos = new Vector3(j, -0.25f, i);
                GameObject wallPiece = Instantiate(prefab, wallPos, Quaternion.Euler(90, 90, 0), roomParent.transform);
            }

            int roomWidth = (int) Math.Abs(bottomLeftVertice.x - bottomRightVertice.x);

            if ((roomWidth % 5) != 0) {
                for (int j = (int) i; j < i + 5; j++) {
                    Vector3 wallPos = new Vector3(bottomRightVertice.x - (roomWidth % 5), -0.25f, j);
                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), roomParent.transform);
                }
            }
        }
        int roomHeight = (int) Math.Abs(bottomLeftVertice.z - topLeftVertice.z);
        if ((roomHeight % 5) != 0) {
            for (int i = (int) (topLeftVertice.z - (roomHeight % 5)); i < topLeftVertice.z; i++) {
                for (int j = (int) topLeftVertice.x; j < topRightVertice.x; j += 5) {
                    Vector3 wallPos = new Vector3(j, -0.25f, i);
                    GameObject wallPiece = Instantiate(castleWall1x5Prefab, wallPos, Quaternion.Euler(90, 90, 0), roomParent.transform);
                }
            }
        }




        roomParent.transform.parent = newObjectParent;


        //Vector3[] vertices = new Vector3[] {
        //        //ORDER IMPORTANT HERE FOR UNITY
        //        topLeftVertice,
        //        topRightVertice,
        //        bottomLeftVertice,
        //        bottomRightVertice
        //    };

        ////create uv's for mesh
        //Vector2[] uvs = new Vector2[vertices.Length];
        //for (int i = 0; i < uvs.Length; i++) {
        //    uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        //}

        ////create triangles
        //int[] triangles = new int[] {
        //        //ORDER MATTERS HERE FOR UNITY
        //        0,
        //        1,
        //        2,
        //        2,
        //        1,
        //        3
        //    };

        ////create mesh
        //Mesh mesh = new Mesh();
        ////set attributes
        //mesh.vertices = vertices;
        //mesh.uv = uvs;
        //mesh.triangles = triangles;

        ////create new gameObject with a given name and components
        //GameObject dungeonFloor = new GameObject("Mesh" + node.distanceFromOrigin, typeof(MeshFilter), typeof(MeshRenderer));




        ////set transforms
        //dungeonFloor.transform.position = Vector3.zero;
        //dungeonFloor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        //dungeonFloor.transform.localScale = Vector3.one;
        ////set other components
        //dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        //dungeonFloor.GetComponent<MeshRenderer>().material = materialIn;
        ////set parent object
        //dungeonFloor.transform.parent = newObjectParent;

        //set bottom left corner gameobject as child of mesh
        roomNode.bottomRightCornerObject.transform.parent = roomParent.transform;



        ////GENERATE WALL POSITIONS
        ////Right side of room
        //for (int row = (int) bottomLeftVertice.x; row < (int) bottomRightVertice.x; row++) {
        //    var wallPosition = new Vector3(row, 0, bottomLeftVertice.z);
        //    AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.RIGHT);
        //}
        ////Left side of room
        //for (int row = (int) topLeftVertice.x; row < (int) node.TopRightAreaCorner.x; row++) {
        //    var wallPosition = new Vector3(row, 0, topRightVertice.z);
        //    AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.LEFT);
        //}
        ////Bottom of Room
        //for (int col = (int) bottomLeftVertice.z; col < (int) topLeftVertice.z; col++) {
        //    var wallPosition = new Vector3(bottomLeftVertice.x, 0, col);
        //    AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.DOWN);
        //}
        ////top of room
        //for (int col = (int) bottomRightVertice.z; col < (int) topRightVertice.z; col++) {
        //    var wallPosition = new Vector3(bottomRightVertice.x, 0, col);
        //    AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.UP);
        //}

    }

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


    //Generate wall positions
    void GenerateWallPositions(Node node, Material materialIn, Transform newObjectParent) {
        //Create mesh vertices
        Vector3 bottomLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.BottomLeftAreaCorner.y);
        Vector3 bottomRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.BottomRightAreaCorner.y);
        Vector3 topLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.TopRightAreaCorner.y);
        Vector3 topRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.TopRightAreaCorner.y);

        //GENERATE WALL POSITIONS
        //Right side of room
        for (int row = (int) bottomLeftVertice.x; row < (int) bottomRightVertice.x; row++) {
            var wallPosition = new Vector3(row, 0, bottomLeftVertice.z);
            AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.RIGHT);
        }
        //Left side of room
        for (int row = (int) topLeftVertice.x; row < (int) node.TopRightAreaCorner.x; row++) {
            var wallPosition = new Vector3(row, 0, topRightVertice.z);
            AddWallPositionToList(wallPosition, possibleWallPosition, possibleDoorHorizontalPosition, WallDirection.LEFT);
        }
        //Bottom of Room
        for (int col = (int) bottomLeftVertice.z; col < (int) topLeftVertice.z; col++) {
            var wallPosition = new Vector3(bottomLeftVertice.x, 0, col);
            AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.DOWN);
        }
        //top of room
        for (int col = (int) bottomRightVertice.z; col < (int) topRightVertice.z; col++) {
            var wallPosition = new Vector3(bottomRightVertice.x, 0, col);
            AddWallPositionToList(wallPosition, possibleFloorCeilingPosition, possibleDoorVerticalPosition, WallDirection.UP);
        }

    }

    private void CreatePathNodes(List<RoomNode> listOfRooms, List<CorridorNode> listOfCorridors) {
        for (int i = 0; i < listOfRooms.Count; i++) {
            Vector2Int centerPoint = (listOfRooms[i].TopLeftAreaCorner + listOfRooms[i].BottomRightAreaCorner) / 2;

            //create path node object
            GameObject pathNodeObject = new GameObject("PathNode - room " + i, typeof(MeshFilter), typeof(MeshRenderer));


            if (debugMode) {
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
        }
        for (int i = 0; i < listOfCorridors.Count; i++) {
            Vector2Int centerPoint = (listOfCorridors[i].TopLeftAreaCorner + listOfCorridors[i].BottomRightAreaCorner) / 2;

            //create path node object
            GameObject pathNodeObject = new GameObject("PathNode - corridor " + i, typeof(MeshFilter), typeof(MeshRenderer));


            if (debugMode) {
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

        }
    }


    //sets positions of walls to proper list
    private void AddWallPositionToList(Vector3 wallPosition, List<WallData> wallList, List<WallData> doorList, WallDirection direction) {
        //get point from wall position
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        WallData temp = new WallData(point, direction);
        int index = -1;

        //Check if wall list already contains this point
        for (int i = 0; i < wallList.Count; i++) {
            if (wallList[i].position == temp.position) {
                index = i;
                break;
            }
        }

        //Test index need to be a door
        if (index != -1) {
            wallList.RemoveAt(index);
            doorList.Add(temp);
        }
        else {
            //add to wall list
            wallList.Add(temp);
        }
    }

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
            spawnPos = new Vector3(spawnRoomPathNode.RoomTopLeftCorner.x + .1f, 0, spawnRoomPathNode.RoomTopLeftCorner.y - spawnRoomPathNode.RoomDimensions.y + 2);

            //Rotate player
            playerPrefab.transform.GetChild(0).transform.localRotation = Quaternion.Euler(0, 90, 0);
            playerPrefab.GetComponent<PlayerController>().IsFacingLeft = true;
        }


        //places player
        playerPrefab.transform.position = spawnPos;
        if (!dungeonFlatMode) {
            //rotates player around origin to account for level rotation
            playerPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
            playerPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
        }

        playerPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerPrefab.transform.position = new Vector3(playerPrefab.transform.position.x + 1, spawnPos.x, playerPrefab.transform.position.z + 1);
    }

    public void PlaceBoss(PathNode bossRoomPathNode) {
        Vector3 spawnPos = Vector3.zero;

        if ((bossRoomPathNode.RoomTopLeftCorner.y - bossRoomPathNode.RoomDimensions.y / 2) > dungeonWidth / 2) {
            //Spawn on "left" side of room
            spawnPos = new Vector3(bossRoomPathNode.RoomTopLeftCorner.x + .1f, 0, bossRoomPathNode.RoomTopLeftCorner.y - 2);

        }
        else {
            //spawn on "right" side of room
            spawnPos = new Vector3(bossRoomPathNode.RoomTopLeftCorner.x + .1f, 0, bossRoomPathNode.RoomTopLeftCorner.y - bossRoomPathNode.RoomDimensions.y + 2);

        }


        //places boss
        bossPrefab.transform.position = spawnPos;
        if (!dungeonFlatMode) {
            //rotates boss around origin to account for level rotation
            bossPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
            bossPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
        }

        bossPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
        bossPrefab.transform.position = new Vector3(bossPrefab.transform.position.x, spawnPos.x + 3f, bossPrefab.transform.position.z + 1);
    }

    public void PlacePlatforms(PathNode room) {

        int verticalPlatformDistance = 4;
        int platformWidth = 5;
        int roomHeight = room.RoomDimensions.y;
        int roomWidth = room.RoomDimensions.x;
        int groundLevel = room.RoomTopLeftCorner.x + roomOffset;
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

            SpawnLineOfPlatforms(platformData, i, room, platformWidth);
            count++;
        }
    }


    void SpawnLineOfPlatforms(string lineData, int x, PathNode room, int platformWidth) {

        //Helper
        int numSpaces = 0;

        //Validate linedata
        foreach (char c in lineData.ToCharArray()) {
            if (c != '0' && c != '1') {
                return;
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


        GameObject platform = (GameObject) Resources.Load("CastlePlatform1");
        Vector3 spawnPos = new Vector3();

        float totalOffset = 0;
        for (int i = 0; i < lineData.Length; i++) {
            if (lineData[i] == '0') {
                totalOffset += spaceOffsetPerCell;
                continue;
            }
            spawnPos = new Vector3(x, -0.5f, room.RoomTopLeftCorner.y - (i * platformWidth) - totalOffset - platformWidth / 2f);

            platform = Instantiate(platform, spawnPos, Quaternion.Euler(0, 0, 0));
            platform.transform.parent = platformParent;
        }
    }
}

public struct WallData {

    public Vector3Int position;
    public WallDirection direction;

    public WallData(Vector3Int positionIn, WallDirection directionIn) {
        position = positionIn;
        direction = directionIn;
    }
}

public enum WallDirection {
    LEFT = 0,
    UP = 1,
    RIGHT = 2,
    DOWN = 3
}
