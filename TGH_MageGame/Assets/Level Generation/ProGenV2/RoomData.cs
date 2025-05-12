using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour {
    //**PROPERTIES**
    [Header("Room Data")]
    [SerializeField] RoomType roomType;

    [Header("GameObject References")]
    [SerializeField] List<PortalData> portals;
    [SerializeField] List<Transform> enemySpawns;
    [SerializeField] Transform playerSpawn;
    [SerializeField] Transform pathNode;
    [SerializeField] Transform topLeftObject;
    [SerializeField] Transform bottomRightObject;

    //**FIELDS**
    public RoomType RoomType { get => roomType; set => roomType = value; }
    public List<PortalData> Portals { get => portals; set => portals = value; }
    public List<Transform> EnemySpawns { get => enemySpawns; set => enemySpawns = value; }
    public Transform PlayerSpawn { get => playerSpawn; set => playerSpawn = value; }
    public Transform PathNode { get => pathNode; set => pathNode = value; }
    public Transform TopLeftObject { get => topLeftObject; set => topLeftObject = value; }
    public Transform BottomRightObject { get => bottomRightObject; set => bottomRightObject = value; }



    //**UNITY METHODS**
    private void Awake() {

    }
}

public enum RoomType {
    Junction,
    Straight,
    Corner,
    DeadEnd
}
