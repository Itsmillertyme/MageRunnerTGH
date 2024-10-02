using System.Collections.Generic;
using UnityEngine;
public abstract class Node {
    List<Node> childrenNodes;

    public List<Node> ChildrenNodes { get => childrenNodes; }

    public bool Visited { get; set; }
    public Vector2Int BottomLeftAreaCorner { get; set; }
    public Vector2Int BottomRightAreaCorner { get; set; }
    public Vector2Int TopRightAreaCorner { get; set; }
    public Vector2Int TopLeftAreaCorner { get; set; }

    public Node parent { get; set; }

    public int TreeLayerIndex { get; set; }

    public GameObject bottomRightCornerObject;
    public float distanceFromOrigin;
    public float area;

    public Node(Node parentNode) {
        childrenNodes = new List<Node>();
        this.parent = parentNode;
        if (parent != null) {
            parentNode.AddChild(this);
        }
    }

    public void AddChild(Node node) {
        childrenNodes.Add(node);
    }
    public void RemoveChild(Node node) {
        childrenNodes.Remove(node);
    }

    public float Distance2D(Vector2Int pos1, Vector2Int pos2) {
        float dist;

        //d=√((x2 – x1)² + (y2 – y1)²)

        float part1 = (pos2.x - pos1.x) ^ 2;
        float part2 = (pos2.y - pos1.y) ^ 2;

        dist = Mathf.Sqrt(part1 + part2);

        return dist;
    }
}