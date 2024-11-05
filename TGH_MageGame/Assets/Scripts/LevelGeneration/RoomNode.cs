using System.Collections.Generic;
using UnityEngine;
public class RoomNode : Node {



    public int Width { get => (int) (TopRightAreaCorner.x - BottomLeftAreaCorner.x); }
    public int Length { get => (int) (TopRightAreaCorner.y - BottomLeftAreaCorner.y); }

    public List<CorridorNode> corridors;

    public GameObject pathNode;

    //public List<CorridorNode> Corridors { get => corridors; }
    //public GameObject PathNode { get => pathNode; set => pathNode = value; }

    //public float distanceFromOrigin;

    //public GameObject bottomLeftCornerObject; 

    public RoomNode(Vector2Int BottomLeftAreaCorner, Vector2Int TopRightAreaCorner, Node parentNode, int index) : base(parentNode) {
        this.BottomLeftAreaCorner = BottomLeftAreaCorner;
        this.TopRightAreaCorner = TopRightAreaCorner;
        this.BottomRightAreaCorner = new Vector2Int(TopRightAreaCorner.x, BottomLeftAreaCorner.y);
        this.TopLeftAreaCorner = new Vector2Int(BottomLeftAreaCorner.x, TopRightAreaCorner.y);
        this.TreeLayerIndex = index;

        distanceFromOrigin = Distance2D(Vector2Int.zero, BottomLeftAreaCorner);
        area = Mathf.Abs(BottomLeftAreaCorner.x - TopRightAreaCorner.x) * Mathf.Abs(BottomLeftAreaCorner.y - TopRightAreaCorner.y);
        string temp = area.ToString();
        bottomRightCornerObject = new GameObject(temp + "'s \"Bottom\" Left Corner");
        //bottomRightCornerObject.name = temp;
        bottomRightCornerObject.transform.position = new Vector3(BottomLeftAreaCorner.x, 0, BottomLeftAreaCorner.y);
    }

    public void AddCorridor(CorridorNode corridor) {
        corridors.Add(corridor);
    }
}