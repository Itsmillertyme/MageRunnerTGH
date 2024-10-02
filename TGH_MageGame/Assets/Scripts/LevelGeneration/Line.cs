
using UnityEngine;

public class Line {

    Orientation orientation;
    Vector2Int coordinates;

    public Orientation Orientation { get => orientation; set => orientation = value; }
    public Vector2Int Coordinates { get => coordinates; set => coordinates = value; }

    public Line(Orientation orientationIn, Vector2Int coordinatesIn) {
        this.orientation = orientationIn;
        this.coordinates = coordinatesIn;
    }
}
public enum Orientation {
    HORIZONTAL = 0,
    VERTICAL = 1
}