public interface IBehave {


    void Initialize(PathNode roomIn, bool debugMode = false);
    void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false);
}
