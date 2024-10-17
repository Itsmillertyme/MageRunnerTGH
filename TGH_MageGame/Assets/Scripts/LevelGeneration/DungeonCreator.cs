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
    public Transform maskParent;
    public GameObject wallHorizontal;
    public GameObject wallVertical;
    public GameObject maskPrefab;
    public GameObject playerPrefab;


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
        possibleDoorHorizontalPosition = new List<WallData>();
        possibleDoorVerticalPosition = new List<WallData>();
        possibleWallPosition = new List<WallData>();
        possibleFloorCeilingPosition = new List<WallData>();


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

        CreateMasks(maskParent);

        PlacePlayer(listOfRooms);

        //DEV - rotate parent to show vertically
        dungeonParent.rotation = Quaternion.Euler(0, 90, 90);
        dungeonParent.position = new Vector3(0, 0, -0.5f);
    }

    public void RetryGeneration() {

        ClearDungeon();

        //create new dungeon
        CreateDungeon();
    }

    //**Utility Methods**
    public void ClearDungeon() {
        //get all rooms, corridors, walls and masks
        Transform[] roomchildren = roomParent.GetComponentsInChildren<Transform>();
        Transform[] corridorchildren = corridorParent.GetComponentsInChildren<Transform>();
        Transform[] wallchildren = wallParent.GetComponentsInChildren<Transform>();
        Transform[] maskchildren = maskParent.GetComponentsInChildren<Transform>();

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
    private void CreateWall(Transform wallParent, WallData wallPosition, GameObject wallPrefab) {

        GameObject go = Instantiate(wallPrefab, new Vector3(wallPosition.position.x, wallPosition.position.y, wallPosition.position.z), Quaternion.identity, wallParent);

        float newRotation = (int) wallPosition.direction * 90f;

        GameObject rotationobject = go.transform.GetChild(0).gameObject;
        Quaternion rotation = go.transform.rotation;
        rotationobject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y + newRotation, rotation.z);
    }


    //Generate masking meshes
    private void CreateMasks(Transform maskParent) {

        //foreach (Vector3Int wallPosition in possibleWallPosition) {
        //    Vector3Int maskPosition = new Vector3Int(wallPosition.x, 3, wallPosition.z);

        //    Instantiate(maskPrefab, maskPosition, Quaternion.identity, maskParent);
        //    Instantiate(maskPrefab, new Vector3Int(maskPosition.x, 3, maskPosition.z - 1), Quaternion.identity, maskParent);
        //    Instantiate(maskPrefab, new Vector3Int(maskPosition.x, 3, maskPosition.z - 2), Quaternion.identity, maskParent);
        //    Instantiate(maskPrefab, new Vector3Int(maskPosition.x, 3, maskPosition.z - 3), Quaternion.identity, maskParent);
        //    Instantiate(maskPrefab, new Vector3Int(maskPosition.x, 3, maskPosition.z - 4), Quaternion.identity, maskParent);
        //}

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

struct WallData {

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
