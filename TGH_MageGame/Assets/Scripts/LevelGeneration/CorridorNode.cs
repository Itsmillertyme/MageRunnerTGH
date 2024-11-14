using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CorridorNode : Node {
    private RoomNode structure1;
    private RoomNode structure2;
    private int corridorWidth;
    private int modifierDistanceFromWall = 1;
    private Direction direction;

    public GameObject pathNode;
    //public PathNode PathNode { get => pathNode; set => pathNode = value; }
    public RoomNode Structure1 { get => structure1; }
    public RoomNode Structure2 { get => structure2; }

    public int Width { get => (int) (TopRightAreaCorner.x - BottomLeftAreaCorner.x); }
    public int Length { get => (int) (TopRightAreaCorner.y - BottomLeftAreaCorner.y); }
    public Direction Direction { get => direction; set => direction = value; }


    //constructor passes in null to super constructor for parent attribute, Makes corridors not connected
    public CorridorNode(RoomNode node1, RoomNode node2, int corridorWidth) : base(null) {
        this.structure1 = node1;
        this.structure2 = node2;
        this.corridorWidth = corridorWidth;

        GenerateCorridor();
        distanceFromOrigin = Distance2D(Vector2Int.zero, BottomLeftAreaCorner);
        area = Mathf.Abs(BottomLeftAreaCorner.x - TopRightAreaCorner.x) * Mathf.Abs(BottomLeftAreaCorner.y - TopRightAreaCorner.y);
        string temp = area.ToString();
        bottomRightCornerObject = new GameObject(temp + "'s \"Bottom\" Left Corner");
        //bottomRightCornerObject.name = temp;
        bottomRightCornerObject.transform.position = new Vector3(BottomLeftAreaCorner.x, 0, BottomLeftAreaCorner.y);

    }

    //Creates a corridor
    private void GenerateCorridor() {
        //get relative position of structure 2 to structure 1
        var relativePositionOfStructure2 = CheckPositionStructure2AgainstStructure1();
        //process based on relative position
        switch (relativePositionOfStructure2) {
            case RelativePosition.UP:
                ProcessRoomsInRelativeUpOrDown(this.structure1, this.structure2);
                direction = Direction.VERTICAL;
                break;
            case RelativePosition.DOWN:
                direction = Direction.VERTICAL;
                ProcessRoomsInRelativeUpOrDown(this.structure2, this.structure1);
                break;
            case RelativePosition.RIGHT:
                direction = Direction.HORTIZONTAL;
                ProcessRoomsInRelativeRightOrLeft(this.structure1, this.structure2);
                break;
            case RelativePosition.LEFT:
                direction = Direction.HORTIZONTAL;
                ProcessRoomsInRelativeRightOrLeft(this.structure2, this.structure1);
                break;
        }
    }

    //Gets rooms best suited to connect that are horizontally adjacent
    private void ProcessRoomsInRelativeRightOrLeft(Node structure1In, Node structure2In) {
        Node leftStructure = null;
        List<Node> leftStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure1In);
        Node rightStructure = null;
        List<Node> rightStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure2In);

        //Sort all children of the left structure by x value in top right corner of each child (we need to get most right aligned structure in left structure's children to connect to right structure's children)
        var sortedLeftStructures = leftStructureChildren.OrderByDescending(child => child.TopRightAreaCorner.x).ToList();

        //set left structure to connect
        if (sortedLeftStructures.Count == 1) {
            leftStructure = sortedLeftStructures[0];
        }
        else {
            //get max x value of the right side of every left structures children from sorted list
            int maxX = sortedLeftStructures[0].TopRightAreaCorner.x;
            //trim the children from the sorted list if there x value in top right corner is less than a threshold of 10 less than maxX (trims children nodes that are too far away to connect)
            sortedLeftStructures = sortedLeftStructures.Where(children => Math.Abs(maxX - children.TopRightAreaCorner.x) < 10).ToList();
            //get random index from sorted list
            int index = Random.Range(0, sortedLeftStructures.Count);

            leftStructure = sortedLeftStructures[index];
        }

        var possibleNeightborsInRightStructureList = rightStructureChildren.Where(
            child => GetValidYForNeighborLeftRight(leftStructure.TopRightAreaCorner,
                                                   leftStructure.BottomRightAreaCorner,
                                                   child.TopLeftAreaCorner,
                                                   child.BottomLeftAreaCorner
                                                   ) != -1
            ).OrderBy(child => child.BottomRightAreaCorner.x).ToList();

        //set right structure to connect
        if (possibleNeightborsInRightStructureList.Count <= 0) {
            rightStructure = structure2In;
        }
        else {
            rightStructure = possibleNeightborsInRightStructureList[0];
        }
        //get valid y
        int y = GetValidYForNeighborLeftRight(leftStructure.TopLeftAreaCorner,
                                              leftStructure.BottomRightAreaCorner,
                                              rightStructure.TopLeftAreaCorner,
                                              rightStructure.BottomLeftAreaCorner);

        //loop while first set of neighbors isn't valid and there are more structures in the sorted trimmed list
        while (y == -1 && sortedLeftStructures.Count > 1) {
            //delete current structure from our list
            sortedLeftStructures = sortedLeftStructures.Where(child => child.TopLeftAreaCorner.y != leftStructure.TopLeftAreaCorner.y).ToList();
            //reset left structure
            leftStructure = sortedLeftStructures[0];
            //get new valid y for corridor
            y = GetValidYForNeighborLeftRight(leftStructure.TopLeftAreaCorner,
                                              leftStructure.BottomRightAreaCorner,
                                              rightStructure.TopLeftAreaCorner,
                                              rightStructure.BottomLeftAreaCorner);
        }

        //Calculate corridor corners
        BottomLeftAreaCorner = new Vector2Int(leftStructure.BottomRightAreaCorner.x, y);
        TopRightAreaCorner = new Vector2Int(rightStructure.TopLeftAreaCorner.x, y + this.corridorWidth);
        BottomRightAreaCorner = new Vector2Int(rightStructure.TopLeftAreaCorner.x, y);
        TopLeftAreaCorner = new Vector2Int(leftStructure.BottomRightAreaCorner.x, y + this.corridorWidth);

        //SET STRUCTURE NODES
        structure1 = (RoomNode) leftStructure;
        structure2 = (RoomNode) rightStructure;
    }

    //Gets rooms best suited to connect that are Vertically adjacent
    private void ProcessRoomsInRelativeUpOrDown(Node structure1In, Node structure2In) {
        Node bottomStructure = null;
        List<Node> bottomStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure1In);
        Node topStructure = null;
        List<Node> topStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure2In);

        //Sort all children of the bottom structure by y value in top right corner of each child (we need to get most top aligned structure in bottom structure's children to connect to top structure's children)
        var sortedBottomStructure = bottomStructureChildren.OrderByDescending(child => child.TopRightAreaCorner.y).ToList();

        //set bottom structure to connect
        if (sortedBottomStructure.Count == 1) {
            bottomStructure = bottomStructureChildren[0];
        }
        else {
            //get max y value of the top side of every bottom structures children from sorted list            
            int maxY = sortedBottomStructure[0].TopLeftAreaCorner.y;
            //trim the children from the sorted list if there y value in top left corner is less than a threshold of 10 less than maxY (trims children nodes that are too far away to connect)
            sortedBottomStructure = sortedBottomStructure.Where(child => Mathf.Abs(maxY - child.TopLeftAreaCorner.y) < 10).ToList();
            //get random index from sorted list
            int index = Random.Range(0, sortedBottomStructure.Count);

            bottomStructure = sortedBottomStructure[index];
        }

        var possibleNeighboursInTopStructure = topStructureChildren.Where(
            child => GetValidXForNeighbourUpDown(bottomStructure.TopLeftAreaCorner,
                                                 bottomStructure.TopRightAreaCorner,
                                                 child.BottomLeftAreaCorner,
                                                 child.BottomRightAreaCorner)
            != -1).OrderBy(child => child.BottomRightAreaCorner.y).ToList();

        //set top structure to connect
        if (possibleNeighboursInTopStructure.Count == 0) {
            topStructure = structure2In;
        }
        else {
            topStructure = possibleNeighboursInTopStructure[0];
        }



        //get valid x
        int x = GetValidXForNeighbourUpDown(bottomStructure.TopLeftAreaCorner,
                                            bottomStructure.TopRightAreaCorner,
                                            topStructure.BottomLeftAreaCorner,
                                            topStructure.BottomRightAreaCorner);

        //loop while first set of neighbors isn't valid and there are more structures in the sorted trimmed list
        while (x == -1 && sortedBottomStructure.Count > 1) {
            //delete current structure from our list            
            sortedBottomStructure = sortedBottomStructure.Where(child => child.TopLeftAreaCorner.x != topStructure.TopLeftAreaCorner.x).ToList();
            //reset bottom structure            
            bottomStructure = sortedBottomStructure[0];
            //get new valid x for corridor
            x = GetValidXForNeighbourUpDown(bottomStructure.TopLeftAreaCorner,
                                            bottomStructure.TopRightAreaCorner,
                                            topStructure.BottomLeftAreaCorner,
                                            topStructure.BottomRightAreaCorner);
        }
        //Calculate corridor corners
        BottomLeftAreaCorner = new Vector2Int(x, bottomStructure.TopLeftAreaCorner.y);
        TopRightAreaCorner = new Vector2Int(x + this.corridorWidth, topStructure.BottomLeftAreaCorner.y);
        BottomRightAreaCorner = new Vector2Int(x + this.corridorWidth, bottomStructure.TopLeftAreaCorner.y);
        TopLeftAreaCorner = new Vector2Int(x, topStructure.BottomLeftAreaCorner.y);


        //SET STRUCTURE NODES
        structure1 = (RoomNode) topStructure;
        structure2 = (RoomNode) bottomStructure;
    }

    //gets valid x for corridor
    private int GetValidXForNeighbourUpDown(Vector2Int bottomNodeLeft,
        Vector2Int bottomNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight) {

        //Check if top structure's left is more left than bottom's left and top's right is more right than bottom's right
        if (topNodeLeft.x < bottomNodeLeft.x && bottomNodeRight.x < topNodeRight.x) {
            //return middle point
            //return StructureHelper.CalculateMiddlePoint(bottomNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
            //                                            bottomNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
            //                                            ).x;
            return bottomNodeLeft.x;
        }
        //Check if bottom structure's left is more left than top's left and bottom's right is more right than top's right
        if (topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x) {
            //return middle point
            //return StructureHelper.CalculateMiddlePoint(topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
            //                                            topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
            //                                            ).x;
            return topNodeLeft.x;
        }
        //Check if bottom structure's left is between top's left and top's right (bottom right MUST be more right than top right)
        if (bottomNodeLeft.x >= topNodeLeft.x && bottomNodeLeft.x <= topNodeRight.x) {
            //return middle point
            //return StructureHelper.CalculateMiddlePoint(bottomNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
            //                                            topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
            //                                            ).x;
            return bottomNodeLeft.x;
        }
        //Check if bottom structure's right is between top's left and top's right (bottom left MUST be more left than top left)
        if (bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x) {
            //return middle point
            //return StructureHelper.CalculateMiddlePoint(topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
            //                                            bottomNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
            //                                            ).x;
            return topNodeLeft.x;
        }
        //return negative to indicate no valid x
        return -1;
    }

    //gets valid y for corridor
    private int GetValidYForNeighborLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown) {

        //Check if left structure's top is higher than right's top and left's bottom is lower than right's bottom
        if (rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y) {
            //return middle point
            return StructureHelper.CalculateMiddlePoint(leftNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                                                        leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                                                        ).y;
        }
        //Check if left structure's top is lower than right's top and left's bottom is higher than right's bottom
        if (rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <= rightNodeDown.y) {
            //return middle point
            return StructureHelper.CalculateMiddlePoint(rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                                                        rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                                                        ).y;
        }
        //Check if left structure's top is between right's top and right's bottom (left bottom MUST be lower than right's bottom)
        if (leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y) {
            //return middle point
            return StructureHelper.CalculateMiddlePoint(rightNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                                                        leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                                                        ).y;
        }
        //Check if left structure's bottom is between right's top and right's bottom (left top MUST be higher than right's top)
        if (leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y) {
            return StructureHelper.CalculateMiddlePoint(leftNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                                                        rightNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                                                        ).y;
        }

        //return negative to indicate no valid y
        return -1;
    }


    //Checks relative position of structure 2 to structure 1
    private RelativePosition CheckPositionStructure2AgainstStructure1() {
        Vector2 middlePointStructure1Temp = ((Vector2) structure1.TopRightAreaCorner + structure1.BottomLeftAreaCorner) / 2;
        Vector2 middlePointStructure2Temp = ((Vector2) structure2.TopRightAreaCorner + structure2.BottomLeftAreaCorner) / 2;
        float angle = CalculateAngle(middlePointStructure1Temp, middlePointStructure2Temp);

        //Select return position based on angle
        if ((angle < 45 && angle >= 0) || (angle > -45 && angle < 0)) {
            return RelativePosition.RIGHT;
        }
        else if (angle > 45 && angle < 135) {
            return RelativePosition.UP;
        }
        else if (angle > -135 && angle < -45) {
            return RelativePosition.DOWN;
        }
        else {
            return RelativePosition.LEFT;
        }
    }

    private float CalculateAngle(Vector2 middlePointStructure1Temp, Vector2 middlePointStructure2Temp) {
        return MathF.Atan2(middlePointStructure2Temp.y - middlePointStructure1Temp.y,
                           middlePointStructure2Temp.x - middlePointStructure1Temp.x) * Mathf.Rad2Deg;
    }
}