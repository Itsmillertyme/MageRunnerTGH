using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour {
    [SerializeField] PathNodeType type; //DO NOT SET IN INSPECTOR
    [SerializeField] public List<GameObject> neighbors = new List<GameObject>();//DO NOT SET IN INSPECTOR

    public PathNodeType Type { get => type; set => type = value; }
    //public List<PathNode> Neighbors { get => neighbors; set => neighbors = value; }


}

public enum PathNodeType {
    ROOM = 0,
    CORRIDOR = 1
}
