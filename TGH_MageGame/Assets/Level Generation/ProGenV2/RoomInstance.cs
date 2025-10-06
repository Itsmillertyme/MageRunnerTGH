using System.Collections.Generic;
using UnityEngine;

public class RoomInstance {

    //**PROPERTIES**
    public RoomData RoomData { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public List<PortalData> Portals { get; private set; }

    //**CONSTRUCTORS**
    public RoomInstance(RoomData roomData, Vector2Int gridPosition) {
        RoomData = roomData;
        GridPosition = gridPosition;
        Portals = new List<PortalData>(roomData.Portals);
    }

    //**UTILITY METHODS**
    //Fetches unlinked portals from the room instance
    public List<PortalData> GetActiveUnconnectedPortals(bool debugMode = false) {

        List<PortalData> openPortals = new List<PortalData>();
        foreach (PortalData portal in Portals) {
            if (debugMode) {
                Debug.Log($"[Level Generation] Checking portal {portal.name} (Active={portal.IsActive}, Connected={portal.IsConnected})");
            }
            if (portal.IsActive && !portal.IsConnected) {
                openPortals.Add(portal);
            }
        }
        return openPortals;
    }

    public List<PortalData> GetInactivePortals(bool debugMode = false) {
        List<PortalData> inactivePortals = new List<PortalData>();
        foreach (PortalData portal in Portals) {
            if (!portal.IsActive) {
                inactivePortals.Add(portal);
            }
        }
        return inactivePortals;
    }

    //Connects two portals together, marking them as connected
    public void ConnectPortals(PortalData thisPortalIn, PortalData otherPortalin) {
        if (thisPortalIn != null && otherPortalin != null) {
            if (Portals.Contains(thisPortalIn)) {
                thisPortalIn.Connect(otherPortalin);
            }
            else {
                Debug.LogError("[Level Generation] Tried to connect a portal that does not belong to this RoomInstance.");
            }
        }
    }

}
