using UnityEngine;

public class PortalData : MonoBehaviour {

    //**PROPERTIES**
    [SerializeField] PortalDirection portalDirection;
    [SerializeField] GameObject portalDoor;
    [SerializeField] bool isActive;

    //**FIELDS**
    public PortalDirection PortalDirection { get => portalDirection; set => portalDirection = value; }
}

public enum PortalDirection {
    Up,
    Down,
    Left,
    Right
}
