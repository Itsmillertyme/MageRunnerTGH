using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour {
    [SerializeField] PathNodeType type; //DO NOT SET IN INSPECTOR
    [SerializeField] public List<GameObject> neighbors = new List<GameObject>();//DO NOT SET IN INSPECTOR
    [SerializeField] Vector2Int roomDimensions; //DO NOT SET IN INSPECTOR
    [SerializeField] Vector2Int roomTopLeftCorner; //DO NOT SET IN INSPECTOR
    [SerializeField] Direction direction; //DO NOT SET IN INSPECTOR

    public PathNodeType Type { get => type; set => type = value; }
    public Vector2Int RoomDimensions { get => roomDimensions; set => roomDimensions = value; }
    public Vector2Int RoomTopLeftCorner { get => roomTopLeftCorner; set => roomTopLeftCorner = value; }
    public Direction Direction { get => direction; set => direction = value; }
}

public enum PathNodeType {
    ROOM = 0,
    CORRIDOR = 1
}
