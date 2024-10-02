using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator {
    private int maxIterations;
    private int roomWidthMin;
    private int roomLengthMin;

    public RoomGenerator(int maxIterations, int roomWidthMin, int roomLengthMin) {
        this.maxIterations = maxIterations;
        this.roomWidthMin = roomWidthMin;
        this.roomLengthMin = roomLengthMin;
    }

    //generates room sizes based on spaces. Rooms always slightly smaller than space it is allowed
    public List<RoomNode> GenerateRoomsInGivenSpaces(List<Node> roomSpaces, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset) {
        List<RoomNode> listToReturn = new List<RoomNode>();
        //loop for every space
        for (int i = 0; i < roomSpaces.Count; i++) {
            //calculate offsetted BL and TR points based on point mult and offset
            Vector2Int newBottomLeftPoint = StructureHelper.GenerateBottomLeftCornerBetween(roomSpaces[i].BottomLeftAreaCorner, roomSpaces[i].TopRightAreaCorner, roomBottomCornerModifier, roomOffset);
            Vector2Int newTopRightPoint = StructureHelper.GenerateTopRightCornerBetween(roomSpaces[i].BottomLeftAreaCorner, roomSpaces[i].TopRightAreaCorner, roomTopCornerModifier, roomOffset);

            //set the space to use calculated room points now
            roomSpaces[i].BottomLeftAreaCorner = newBottomLeftPoint;
            roomSpaces[i].TopRightAreaCorner = newTopRightPoint;
            roomSpaces[i].BottomRightAreaCorner = new Vector2Int(newTopRightPoint.x, newBottomLeftPoint.y);
            roomSpaces[i].TopLeftAreaCorner = new Vector2Int(newBottomLeftPoint.x, newTopRightPoint.y);

            RoomNode temp = (RoomNode) roomSpaces[i];
            temp.bottomRightCornerObject.transform.position = new Vector3(newBottomLeftPoint.x, 0, newBottomLeftPoint.y);

            listToReturn.Add(temp);
        }

        return listToReturn;
    }
}