using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour {

    [Header("Room Data")]
    [SerializeField] RoomType roomType;

    [Header("GameObject References")]
    [SerializeField] List<PortalData> portals;
    [SerializeField] List<Transform> enemySpawns;
    [SerializeField] Transform playerSpawn;
    [SerializeField] Transform pathNode;
    [SerializeField] Transform topLeftObject;
    [SerializeField] Transform bottomRightObject;



    //**PROPERTIES**
    public RoomType RoomType { get => roomType; }
    public List<PortalData> Portals { get => portals; }
    public List<Transform> EnemySpawns { get => enemySpawns; }
    public Transform PlayerSpawn { get => playerSpawn; }
    public Transform PathNode { get => pathNode; }
    public Transform TopLeftObject { get => topLeftObject; }
    public Transform BottomRightObject { get => bottomRightObject; }

    public int Width => Mathf.Abs(Mathf.RoundToInt(bottomRightObject.position.x - topLeftObject.position.x));
    public int Height => Mathf.Abs(Mathf.RoundToInt(topLeftObject.position.y - bottomRightObject.position.y));
    public Vector2Int Center => new Vector2Int(Mathf.RoundToInt((topLeftObject.position.x + bottomRightObject.position.x) / 2), Mathf.RoundToInt((topLeftObject.position.y + bottomRightObject.position.y) / 2));



    //**UNITY METHODS**
    private void Awake() {

        // Initialize all portals
        if (Application.isPlaying) {
            InitializePortals();
        }
    }

    //**UTILITY METHODS**
    //Fetches whether there is an active portal in the given direction
    public bool HasPortalInDirection(PortalDirection direction) {
        foreach (PortalData portal in portals) {
            if (portal.PortalDirection == direction) {
                return true;
            }
        }
        return false;
    }
    //Fetches the portal in the given direction, or null if none exists
    public PortalData GetPortalInDirection(PortalDirection direction) {
        foreach (PortalData portal in portals) {
            if (portal.PortalDirection == direction) {
                return portal;
            }
        }
        return null;
    }
    //
    public void InitializePortals() {
        foreach (PortalData portal in portals) {
            if (portal != null) {
                portal.PortalRoom = this;
                portal.IsActive = true; //default to active
                portal.IsConnected = false; //default to unconnected
                portal.LinkedPortal = null;
            }
        }
    }

}

public enum RoomType {
    Junction,
    Straight,
    Corner,
    DeadEnd,
    Connector,
    Special
}
