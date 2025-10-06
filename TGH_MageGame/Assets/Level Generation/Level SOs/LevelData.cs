using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject {

    [Header("Special Room")]
    [SerializeField] GameObject startRoom;
    [SerializeField] GameObject bossRoom;

    [Header("Room Prefabs")]
    [SerializeField] List<GameObject> straights;
    [SerializeField] List<GameObject> corners;
    [SerializeField] List<GameObject> junctions;
    [SerializeField] List<GameObject> deadEnds;
    [SerializeField] List<GameObject> horizontalConnectors;
    [SerializeField] List<GameObject> verticalConnectors;

    [Header("Level Settings")]
    [SerializeField] int maxWidth;
    [SerializeField] int maxHeight;

    //**PROPERTIES**
    public GameObject StartRoom { get => startRoom; }
    public GameObject BossRoom { get => bossRoom; }
    //
    public List<GameObject> Straights { get => straights; }
    public List<GameObject> Corners { get => corners; }
    public List<GameObject> Junctions { get => junctions; }
    public List<GameObject> DeadEnds { get => deadEnds; }
    public List<GameObject> HorizontalConnectors { get => horizontalConnectors; }
    public List<GameObject> VerticalConnectors { get => verticalConnectors; }
    //
    public int MaxWidth { get => maxWidth; }
    public int MaxHeight { get => maxHeight; }

}
