using UnityEngine;

public class PortalData : MonoBehaviour {

    [Header("Portal Settings")]
    [SerializeField] PortalDirection portalDirection;
    [SerializeField] GameObject portalDoor;

    [Header("Runtime State")]
    [SerializeField] bool isActive; //portal open or closed
    [SerializeField] bool isConnected; //portal linked to another room
    [SerializeField] RoomData portalRoom; //the room this portal belongs to
    [SerializeField] PortalData linkedPortal; //the room this portal is linked to

    //**PROPERTIES**
    public PortalDirection PortalDirection { get => portalDirection; set => portalDirection = value; }
    public GameObject PortalDoor { get => portalDoor; }
    public bool IsActive { get => isActive; set => isActive = value; }
    public bool IsConnected { get => isConnected; set => isConnected = value; }
    public RoomData PortalRoom { get => portalRoom; set => portalRoom = value; }
    public PortalData LinkedPortal { get => linkedPortal; set => linkedPortal = value; }



    //**UTILITY METHODS**
    //Get oppopsite direction fo this portal
    public PortalDirection GetOppositeDirection() {
        switch (portalDirection) {
            case PortalDirection.UP:
                return PortalDirection.DOWN;
            case PortalDirection.DOWN:
                return PortalDirection.UP;
            case PortalDirection.LEFT:
                return PortalDirection.RIGHT;
            case PortalDirection.RIGHT:
                return PortalDirection.LEFT;
            default:
                return portalDirection;
        }
    }

    // Fetch portal world position
    public Vector3 GetWorldPosition() {
        return transform.position;
    }
    //Connects two portals 
    public void Connect(PortalData otherPortal) {

        if (otherPortal != null) {
            IsConnected = true;
            LinkedPortal = otherPortal;

            otherPortal.IsConnected = true;
            otherPortal.LinkedPortal = this;
        }
    }
    //Close a portal
    public void ClosePortal() {

        if (isConnected) {
            return;
        }

        // Mark this portal as closed
        IsActive = false;
        LinkedPortal = null;

        // If there's a physical door object, enable it
        if (portalDoor != null) {
            portalDoor.SetActive(true);
        }
    }

    public void OpenPortal() {
        if (isConnected) {
            return;
        }

        // Mark this portal as open
        IsActive = true;
        LinkedPortal = null;

        // If there's a physical door object, enable it
        if (portalDoor != null) {
            portalDoor.SetActive(false);
        }
    }

}



public enum PortalDirection {
    UP,
    DOWN,
    LEFT,
    RIGHT
}
