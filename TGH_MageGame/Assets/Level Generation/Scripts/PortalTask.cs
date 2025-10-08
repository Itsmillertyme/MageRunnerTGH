public class PortalTask {

    //**PROPERTIES**
    public RoomInstance SourceRoom { get; private set; }
    public PortalData SourcePortal { get; private set; }


    //**CONTRUCTORS**
    public PortalTask(RoomInstance sourceRoomIn, PortalData sourcePortalIn) {
        SourceRoom = sourceRoomIn;
        SourcePortal = sourcePortalIn;
    }
}
