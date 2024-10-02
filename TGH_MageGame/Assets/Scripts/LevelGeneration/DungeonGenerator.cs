using System.Collections.Generic;

//Does the generating
public class DungeonGenerator {

    int dungeonWidth;
    int dungeonLength;

    RoomNode rootNode;
    List<RoomNode> allNodesCollection = new List<RoomNode>();

    BinarySpacePartitioner bsp;


    //constructor
    public DungeonGenerator(int dungeonWidth, int dungeonLength) {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
        bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);

    }

    //calculates dimensions or locations of all rooms, MUST BE CALLED BEFORE CalculateCorriodors()!!!
    public List<RoomNode> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset) {
        //Create instance of BSP script
        //BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
        //get list of spaces with area
        allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);

        //get room spaces list from structure helper, given root (Gets all of the smallest spaces in our graph because they are the lowest leaves on a branch
        List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);

        //generate rooms
        RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomWidthMin, roomLengthMin);
        List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBottomCornerModifier, roomTopCornerModifier, roomOffset);


        //CorridorGenerator corridorGenerator = new CorridorGenerator();
        //var corridorList = corridorGenerator.CreateCorridors(allNodesCollection, corridorWidth);

        //return list of rooms concatenated with list of corridors        
        return roomList;
    }

    public List<Node> CalculateCorridors(int corridorWidth) {
        CorridorGenerator corridorGenerator = new CorridorGenerator();
        var corridorList = corridorGenerator.CreateCorridors(allNodesCollection, corridorWidth);
        return corridorList;
    }
}