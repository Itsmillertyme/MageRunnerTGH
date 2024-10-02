using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour {

    [Header("Dungeon Size Settings")]
    public int dungeonWidth;
    public int dungeonLength;
    public int roomWidthMin;
    public int roomLengthMin;
    public int corridorWidth;

    [Header("Generator Settings")]
    public bool generateOnLoad;
    public int maxIterations;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1f)]
    public float roomTopCornerModifier;
    [Range(0, 2)]
    public int roomOffset;

    [Header("Misc References")]
    public Material roomMaterial;
    public Material corridorMaterial;
    public Transform roomParent;
    public Transform corridorParent;
    public Transform dungeonParent;
    public Transform wallParent;
    public GameObject wallHorizontal;
    public GameObject wallVertical;
    public GameObject playerPrefab;


    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;


    // Start is called before the first frame update
    void Start() {
        if (generateOnLoad) {
            RetryGeneration();
        }
    }

    public void RetryGeneration() {

        ClearDungeon();

        //create new dungeon
        CreateDungeon();
    }

    //Main method for making dungeon
    private void CreateDungeon() {
        //Create instance of generator script
        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
        //Generate list of rooms
        var listOfRooms = generator.CalculateDungeon(maxIterations,
                                                   roomWidthMin,
                                                   roomLengthMin,
                                                   roomBottomCornerModifier,
                                                   roomTopCornerModifier,
                                                   roomOffset
                                                   );

        //Instantiate lists
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();

        //wallParent.parent = transform;

        //Generate list of corridors
        var listOfCorridors = generator.CalculateCorridors(corridorWidth);

        //create mesh and object from list of rooms
        for (int i = 0; i < listOfRooms.Count; i++) {

            CreateGameObjectsAndMesh(listOfRooms[i], roomMaterial, roomParent);
        }
        //create mesh and object from list of corridors
        for (int i = 0; i < listOfCorridors.Count; i++) {

            CreateGameObjectsAndMesh(listOfCorridors[i], corridorMaterial, corridorParent);
        }

        CreateWalls(wallParent);

        PlacePlayer(listOfRooms);

        //DEV - rotate parent to show vertically
        dungeonParent.rotation = Quaternion.Euler(0, 90, 90);
        dungeonParent.position = new Vector3(0, 0, -0.5f);
    }

    public void ClearDungeon() {
        //get all rooms and corridors
        Transform[] roomchildren = roomParent.GetComponentsInChildren<Transform>();
        Transform[] corridorchildren = corridorParent.GetComponentsInChildren<Transform>();
        Transform[] wallchildren = wallParent.GetComponentsInChildren<Transform>();

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
    }

    //generate all walls
    private void CreateWalls(Transform wallParent) {
        foreach (Vector3Int wallPosition in possibleWallHorizontalPosition) {
            CreateWall(wallParent, wallPosition, wallVertical);
        }

        foreach (Vector3Int wallPosition in possibleWallVerticalPosition) {
            CreateWall(wallParent, wallPosition, wallHorizontal);
        }
    }

    //generate a single wall
    private void CreateWall(Transform wallParent, Vector3Int wallPosition, GameObject wallPrefab) {
        Instantiate(wallPrefab, new Vector3(wallPosition.x, wallPosition.y, wallPosition.z), Quaternion.identity, wallParent);
    }

    //creates mesh and gameobjects, instantiates them
    void CreateGameObjectsAndMesh(Node node, Material materialIn, Transform newObjectParent) {


        //Create mesh vertices
        Vector3 bottomLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.BottomLeftAreaCorner.y);
        Vector3 bottomRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.BottomRightAreaCorner.y);
        Vector3 topLeftVertice = new Vector3(node.BottomLeftAreaCorner.x, 0, node.TopRightAreaCorner.y);
        Vector3 topRightVertice = new Vector3(node.TopRightAreaCorner.x, 0, node.TopRightAreaCorner.y);

        Vector3[] vertices = new Vector3[] {
                //ORDER IMPORTANT HERE FOR UNITY
                topLeftVertice,
                topRightVertice,
                bottomLeftVertice,
                bottomRightVertice
            };

        //create uv's for mesh
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++) {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        //create triangles
        int[] triangles = new int[] {
                //ORDER MATTERS HERE FOR UNITY
                0,
                1,
                2,
                2,
                1,
                3
            };

        //create mesh
        Mesh mesh = new Mesh();
        //set attributes
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        //create new gameObject with a given name and components
        GameObject dungeonFloor = new GameObject("Mesh" + node.distanceFromOrigin, typeof(MeshFilter), typeof(MeshRenderer));

        //set transforms
        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        dungeonFloor.transform.localScale = Vector3.one;
        //set other components
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = materialIn;
        //set parent object
        dungeonFloor.transform.parent = newObjectParent;
        //set bottom left corner gameobject as child of mesh
        node.bottomRightCornerObject.transform.parent = dungeonFloor.transform;



        //GENERATE WALL POSITIONS
        //bottom of room
        for (int row = (int) bottomLeftVertice.x; row < (int) bottomRightVertice.x; row++) {
            var wallPosition = new Vector3(row, 0, bottomLeftVertice.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        //top of room
        for (int row = (int) topLeftVertice.x; row < (int) node.TopRightAreaCorner.x; row++) {
            var wallPosition = new Vector3(row, 0, topRightVertice.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        //left side of room
        for (int col = (int) bottomLeftVertice.z; col < (int) topLeftVertice.z; col++) {
            var wallPosition = new Vector3(bottomLeftVertice.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        //right side of room
        for (int col = (int) bottomRightVertice.z; col < (int) topRightVertice.z; col++) {
            var wallPosition = new Vector3(bottomRightVertice.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }

    }

    //sets positions of walls to proper list
    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList) {
        //get point from wall position
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        //test if already in the wall list
        if (wallList.Contains(point)) {
            //add to door list
            doorList.Add(point);
            //remove from wall list
            wallList.Remove(point);
        }
        else {
            //add to wall list
            wallList.Add(point);
        }
    }

    public void PlacePlayer(List<RoomNode> rooms) {
        Vector3 spawnPos = Vector3.zero;
        float distFromOrigin = Mathf.Infinity;

        foreach (RoomNode roomNode in rooms) {
            if (roomNode.distanceFromOrigin < distFromOrigin) {
                distFromOrigin = roomNode.distanceFromOrigin;
                spawnPos = roomNode.bottomRightCornerObject.transform.position;
            }
        }

        //places player
        playerPrefab.transform.position = spawnPos;
        //rotates player around origin to account for level rotation
        playerPrefab.transform.RotateAround(Vector3.zero, Vector3.up, 90);
        playerPrefab.transform.RotateAround(Vector3.zero, Vector3.left, 90);
        playerPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
        playerPrefab.transform.position = new Vector3(playerPrefab.transform.position.x + 1, spawnPos.x + 1, playerPrefab.transform.position.z + 1);
    }

}
