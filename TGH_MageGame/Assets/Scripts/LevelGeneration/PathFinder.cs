using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder {
    List<PathNode> pathNodes;
    PathNode startPoint;
    List<PathNode> endPoints;
    List<PathNode> path;
    public PathNode StartPoint { get => startPoint; }
    public List<PathNode> EndPoints { get => endPoints; }
    public List<PathNode> Path { get => path; }


    public PathFinder() {
        pathNodes = GameObject.Find("PathNodes").GetComponentsInChildren<PathNode>().ToList();
        endPoints = new List<PathNode>();
        path = new List<PathNode>();

        for (int i = 0; i < pathNodes.Count; i++) {
            if (pathNodes[i].neighbors.Count == 1) {
                if (startPoint == null) {
                    startPoint = pathNodes[i];
                }
                else if (Distance2D(new Vector2Int((int) pathNodes[i].transform.position.x, (int) pathNodes[i].transform.position.z), new Vector2Int(0, 0)) < Distance2D(new Vector2Int((int) startPoint.transform.position.x, (int) startPoint.transform.position.z), new Vector2Int(0, 0))) {
                    startPoint = pathNodes[i];
                }
                else {
                    endPoints.Add(pathNodes[i]);
                }
            }
        }

        CalculatePath();
    }

    public void CalculatePath() {
        path.Add(startPoint);
        path.Add(startPoint.neighbors[0].GetComponent<PathNode>());

        bool nodesLeft = true;
        while (nodesLeft) {

            if (path[path.Count - 1].neighbors.Count > 1) {
                if (path.Contains(path[path.Count - 1].neighbors[0].GetComponent<PathNode>())) {
                    path.Add(path[path.Count - 1].neighbors[1].GetComponent<PathNode>());
                }
                else {
                    path.Add(path[path.Count - 1].neighbors[0].GetComponent<PathNode>());
                }
            }
            else {
                nodesLeft = false;

            }
        }
    }

    public float Distance2D(Vector2Int pos1, Vector2Int pos2) {
        float dist;

        //d=√((x2 – x1)² + (y2 – y1)²)

        float part1 = (pos2.x - pos1.x) ^ 2;
        float part2 = (pos2.y - pos1.y) ^ 2;

        dist = Mathf.Sqrt(part1 + part2);

        return dist;
    }
}
