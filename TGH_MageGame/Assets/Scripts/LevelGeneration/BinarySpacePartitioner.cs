using System.Collections.Generic;
using UnityEngine;

//split big room into smaller rooms
public class BinarySpacePartitioner {


    RoomNode rootNode;

    public RoomNode RootNode { get => rootNode; }


    public BinarySpacePartitioner(int dungeonWidth, int dungeonLength) {
        this.rootNode = new RoomNode(new Vector2Int(0, 0), new Vector2Int(dungeonWidth, dungeonLength), null, 0);

    }

    //get all location nodes for spaces
    public List<RoomNode> PrepareNodesCollection(int maxIterations, int roomWidthMin, int roomLengthMin) {

        Queue<RoomNode> graph = new Queue<RoomNode>();
        List<RoomNode> listToReturn = new List<RoomNode>();

        //push root to graph queue
        graph.Enqueue(this.rootNode);
        //add root to list
        listToReturn.Add(this.rootNode);

        int iterations = 0;

        //iterat over graph, splitting as needed
        while (iterations < maxIterations && graph.Count > 0) {

            iterations++;

            //pop node from queue
            RoomNode currentNode = graph.Dequeue();
            //make sure roomnode is large enough to split
            if (currentNode.width >= roomWidthMin * 2 || currentNode.length >= roomLengthMin * 2) {
                //split
                SplitTheSpace(currentNode, listToReturn, roomWidthMin, roomLengthMin, graph);
                //kill corner object from split node
                Object.DestroyImmediate(currentNode.bottomRightCornerObject);
            }

        }
        return listToReturn;
    }

    //splits a room
    private void SplitTheSpace(RoomNode currentNode, List<RoomNode> listToReturn, int roomWidthMin, int roomLengthMin, Queue<RoomNode> graph) {
        //grab a line to split room
        Line line = GetLineDivingSpace(currentNode.BottomLeftAreaCorner, currentNode.TopRightAreaCorner, roomWidthMin, roomLengthMin);

        //new rooms
        RoomNode node1, node2;

        //test oriantation of line
        if (line.Orientation == Orientation.HORIZONTAL) {
            //set nodes
            node1 = new RoomNode(currentNode.BottomLeftAreaCorner,
                                 new Vector2Int(currentNode.TopRightAreaCorner.x, line.Coordinates.y),
                                 currentNode,
                                 currentNode.TreeLayerIndex + 1);
            node2 = new RoomNode(new Vector2Int(currentNode.BottomLeftAreaCorner.x, line.Coordinates.y),
                                 currentNode.TopRightAreaCorner,
                                 currentNode,
                                 currentNode.TreeLayerIndex + 1);
        }
        else {
            //set nodes
            node1 = new RoomNode(currentNode.BottomLeftAreaCorner,
                                 new Vector2Int(line.Coordinates.x, currentNode.TopRightAreaCorner.y),
                                 currentNode,
                                 currentNode.TreeLayerIndex + 1);
            node2 = new RoomNode(new Vector2Int(line.Coordinates.x, currentNode.BottomLeftAreaCorner.y),
                                 currentNode.TopRightAreaCorner,
                                 currentNode,
                                 currentNode.TreeLayerIndex + 1);
        }

        //add new room nodes to graph queue and return list
        AddNewNodeToCollections(listToReturn, graph, node1);
        AddNewNodeToCollections(listToReturn, graph, node2);
    }

    private void AddNewNodeToCollections(List<RoomNode> listToReturn, Queue<RoomNode> graph, RoomNode node) {
        listToReturn.Add(node);
        graph.Enqueue(node);
    }

    //gets line to split roomnode on
    private Line GetLineDivingSpace(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, int roomWidthMin, int roomLengthMin) {
        Orientation orientation;
        //test dimensions to see which way can be split
        bool lengthStatus = (topRightAreaCorner.y - bottomLeftAreaCorner.y) >= 2 * roomLengthMin;
        bool widthStatus = (topRightAreaCorner.x - bottomLeftAreaCorner.x) >= 2 * roomWidthMin;

        //If both, pick random
        if (lengthStatus && widthStatus) {
            orientation = (Orientation) (Random.Range(0, 2));
        }
        //if only one pick that one
        else if (widthStatus) {
            orientation = Orientation.VERTICAL;
        }
        else {
            orientation = Orientation.HORIZONTAL;
        }

        return new Line(orientation, GetCoordinatesForOrientation(orientation, bottomLeftAreaCorner, topRightAreaCorner, roomWidthMin, roomLengthMin));
    }

    //generate in world coordinates for line that splits roomnode
    private Vector2Int GetCoordinatesForOrientation(Orientation orientation, Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, int roomWidthMin, int roomLengthMin) {

        Vector2Int coordinates = Vector2Int.zero;
        //test orientation of line
        if (orientation == Orientation.HORIZONTAL) {

            coordinates = new Vector2Int(0, Random.Range(bottomLeftAreaCorner.y + roomLengthMin, topRightAreaCorner.y - roomLengthMin));
        }
        else {
            coordinates = new Vector2Int(Random.Range(bottomLeftAreaCorner.x + roomWidthMin, topRightAreaCorner.x - roomWidthMin), 0);
        }

        return coordinates;
    }
}