using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(1000)]
public class NavMeshLinkBuilder : MonoBehaviour {
    [Header("References")]
    public NavMeshSurface surface;
    public Transform LevelRoot;

    [Header("Link Settings")]
    public float MinHorizontal = 0.5f;
    public float MaxHorizontal = 10.0f;
    public float MaxVertical = 5.5f;
    public float AgentRadius = 0.5f;
    public bool Bidirectional = true;

    [Header("Path Settings")]
    public bool SkipIfPathAlreadyWalkable = true;
    public int PlainPathAreaMask = NavMesh.AllAreas;
    [Range(1.0f, 3.0f)] public float MaxDetourRatio = 1.5f;

    private struct LinkRecord {
        public NavMeshLinkInstance instance;
        public Vector3 start;
        public Vector3 end;
    }

    private List<LinkRecord> links = new List<LinkRecord>();

    public void ClearLinks() {
        for (int i = 0; i < links.Count; i++) {
            if (links[i].instance.valid) {
                links[i].instance.Remove();
            }
        }
        links.Clear();
    }

    public void BuildAll() {
        ClearLinks();

        if (LevelRoot == null) {
            LevelRoot = surface.transform;
        }

        LinkAnchor[] anchors = LevelRoot.GetComponentsInChildren<LinkAnchor>(true);
        int anchorsLength = anchors.Length;

        for (int i = 0; i < anchorsLength; i++) {
            for (int j = i + 1; j < anchorsLength; j++) {
                // Only link anchors inside the same room root (RoomData on a parent)
                Transform roomA = FindRoomRoot(anchors[i].transform);
                Transform roomB = FindRoomRoot(anchors[j].transform);
                if (roomA == null || roomB == null || roomA != roomB) continue;

                Vector3 a = anchors[i].transform.position;
                Vector3 b = anchors[j].transform.position;

                Vector3 delta = b - a;
                float horizontal = new Vector2(delta.x, delta.z).magnitude;
                float vertical = delta.y;

                if (horizontal < MinHorizontal || horizontal > MaxHorizontal) continue;
                if (Mathf.Abs(vertical) > MaxVertical) continue;
                if (vertical > 0.0f && !anchors[i].AllowJump) continue; // jumping up from A
                if (vertical < 0.0f && !anchors[i].AllowDrop) continue; // dropping from A

                if (LinkBlocked(a, b)) continue;

                // Snap endpoints to the baked NavMesh
                Vector3 start;
                Vector3 end;
                if (!SampleNavMesh(a, 1.0f, out start)) continue;
                if (!SampleNavMesh(b, 1.0f, out end)) continue;

                if (SkipIfPathAlreadyWalkable && HasPlainNavMeshPath(start, end, PlainPathAreaMask, MaxDetourRatio)) {
                    continue;
                }

                // Check if this link is redundant and/or replaceable
                int replaceIndex;
                if (IsRedundantLink(start, end, links, out replaceIndex)) {
                    continue; // existing shorter link found
                }

                NavMeshLinkData data = new NavMeshLinkData {
                    startPosition = start,
                    endPosition = end,
                    width = AgentRadius * 2.0f,
                    bidirectional = Bidirectional,
                    area = 0, // Walkable
                    agentTypeID = surface.agentTypeID
                };

                NavMeshLinkInstance instance = NavMesh.AddLink(data);
                if (instance.valid) {
                    if (replaceIndex >= 0) {
                        // Remove longer link and replace
                        if (links[replaceIndex].instance.valid) {
                            links[replaceIndex].instance.Remove();
                        }
                        links[replaceIndex] = new LinkRecord { instance = instance, start = start, end = end };
                    }
                    else {
                        // Add new link
                        links.Add(new LinkRecord { instance = instance, start = start, end = end });
                    }
                }
            }
        }
    }

    private bool IsRedundantLink(Vector3 start, Vector3 end, List<LinkRecord> existingLinks, out int replaceIndex) {
        float candidateDist = Vector3.Distance(start, end);
        replaceIndex = -1;

        for (int i = 0; i < existingLinks.Count; i++) {
            LinkRecord record = existingLinks[i];
            float existingDist = Vector3.Distance(record.start, record.end);

            bool samePair = (Vector3.Distance(start, record.start) < 0.5f && Vector3.Distance(end, record.end) < 0.5f) ||
                            (Vector3.Distance(start, record.end) < 0.5f && Vector3.Distance(end, record.start) < 0.5f);

            if (samePair) {
                if (existingDist <= candidateDist) {
                    // existing is shorter → candidate is redundant
                    return true;
                }
                else {
                    // candidate is shorter → replace existing
                    replaceIndex = i;
                    return false;
                }
            }
        }
        return false; // no duplicate found
    }

    private bool SampleNavMesh(Vector3 pos, float range, out Vector3 result) {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, range, NavMesh.AllAreas)) {
            result = hit.position;
            return true;
        }
        result = pos;
        return false;
    }

    private bool HasPlainNavMeshPath(Vector3 start, Vector3 end, int areaMask, float maxDetourRatio) {
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(start, end, areaMask, path)) return false;
        if (path.status != NavMeshPathStatus.PathComplete) return false;

        float pathLength = 0.0f;
        for (int i = 1; i < path.corners.Length; i++) {
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        float straightLine = Vector3.Distance(start, end);
        return pathLength <= straightLine * maxDetourRatio;
    }

    private bool LinkBlocked(Vector3 a, Vector3 b) {
        Vector3 dir = b - a;
        float dist = dir.magnitude;
        if (Physics.Raycast(a, dir.normalized, dist, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
            return true;
        }
        return false;
    }

    private Transform FindRoomRoot(Transform t) {
        while (t != null) {
            if (t.GetComponent<RoomData>() != null) return t;
            t = t.parent;
        }
        return null;
    }
}
