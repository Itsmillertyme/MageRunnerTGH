using System.Collections.Generic;
using UnityEngine;

public static class StructureHelper {
    //gets list of lowest nodes given a parent node
    public static List<Node> TraverseGraphToExtractLowestLeaves(Node parentNode) {

        Queue<Node> nodesToCheck = new Queue<Node>();
        List<Node> listToReturn = new List<Node>();

        //test if no children
        if (parentNode.ChildrenNodes.Count == 0) {
            return new List<Node> { parentNode };
        }
        //add all child nodes to queue to check
        foreach (Node child in parentNode.ChildrenNodes) {

            nodesToCheck.Enqueue(child);
        }

        //
        while (nodesToCheck.Count > 0) {
            //pop end node
            Node currentNode = nodesToCheck.Dequeue();
            //if no children add this node to return list
            if (currentNode.ChildrenNodes.Count == 0) {
                listToReturn.Add(currentNode);
            }
            else {
                //put each child node into queue to check for children
                foreach (Node child in currentNode.ChildrenNodes) {
                    nodesToCheck.Enqueue(child);
                }
            }
        }
        return listToReturn;
    }

    public static Vector2Int GenerateBottomLeftCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset) {
        int minX = boundaryLeftPoint.x + offset;
        int maxX = boundaryRightPoint.x - offset;
        int minY = boundaryLeftPoint.y + offset;
        int maxY = boundaryRightPoint.y - offset;
        return new Vector2Int(
            Random.Range(minX, (int) (minX + (maxX - minX) * pointModifier)),
            Random.Range(minY, (int) (minY + (maxY - minY) * pointModifier))
            );
    }
    public static Vector2Int GenerateTopRightCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset) {
        int minX = boundaryLeftPoint.x + offset;
        int maxX = boundaryRightPoint.x - offset;
        int minY = boundaryLeftPoint.y + offset;
        int maxY = boundaryRightPoint.y - offset;
        return new Vector2Int(
            Random.Range((int) (minX + (maxX - minX) * pointModifier), maxX),
            Random.Range((int) (minY + (maxY - minY) * pointModifier), maxY)
            );
    }

    public static Vector2Int CalculateMiddlePoint(Vector2Int v1, Vector2Int v2) {
        Vector2 sum = v1 + v2;
        Vector2 tempVector = sum / 2;
        return new Vector2Int((int) tempVector.x, (int) tempVector.y);
    }
}

public enum RelativePosition {
    UP,
    DOWN,
    RIGHT,
    LEFT
}

