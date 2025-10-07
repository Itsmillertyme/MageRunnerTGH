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

    //public void BuildAll() {
    //    ClearLinks();

    //    if (LevelRoot == null) {
    //        LevelRoot = surface.transform;
    //    }

    //    LinkAnchor[] anchors = LevelRoot.GetComponentsInChildren<LinkAnchor>(true);
    //    int anchorsLength = anchors.Length;

    //    for (int i = 0; i < anchorsLength; i++) {
    //        for (int j = i + 1; j < anchorsLength; j++) {
    //            // Only link anchors inside the same room root (RoomData on a parent)
    //            Transform roomA = FindRoomRoot(anchors[i].transform);
    //            Transform roomB = FindRoomRoot(anchors[j].transform);
    //            if (roomA == null || roomB == null || roomA != roomB) continue;

    //            Vector3 a = anchors[i].transform.position;
    //            Vector3 b = anchors[j].transform.position;

    //            Vector3 delta = b - a;
    //            float horizontal = new Vector2(delta.x, delta.z).magnitude;
    //            float vertical = delta.y;

    //            if (horizontal < MinHorizontal || horizontal > MaxHorizontal) continue;
    //            if (Mathf.Abs(vertical) > MaxVertical) continue;
    //            if (vertical > 0.0f && !anchors[i].AllowJump) continue; // jumping up from A
    //            if (vertical < 0.0f && !anchors[i].AllowDrop) continue; // dropping from A

    //            if (LinkBlocked(a, b)) continue;

    //            // Snap endpoints to the baked NavMesh
    //            Vector3 start;
    //            Vector3 end;
    //            if (!SampleNavMesh(a, 1.0f, out start)) continue;
    //            if (!SampleNavMesh(b, 1.0f, out end)) continue;

    //            if (SkipIfPathAlreadyWalkable && HasPlainNavMeshPath(start, end, PlainPathAreaMask, MaxDetourRatio)) {
    //                continue;
    //            }

    //            // Check if this link is redundant and/or replaceable
    //            int replaceIndex;
    //            if (IsRedundantLink(start, end, links, out replaceIndex)) {
    //                continue; // existing shorter link found
    //            }

    //            NavMeshLinkData data = new NavMeshLinkData {
    //                startPosition = start,
    //                endPosition = end,
    //                width = AgentRadius * 2.0f,
    //                bidirectional = Bidirectional,
    //                area = 0, // Walkable
    //                agentTypeID = surface.agentTypeID
    //            };

    //            NavMeshLinkInstance instance = NavMesh.AddLink(data);
    //            if (instance.valid) {
    //                if (replaceIndex >= 0) {
    //                    // Remove longer link and replace
    //                    if (links[replaceIndex].instance.valid) {
    //                        links[replaceIndex].instance.Remove();
    //                    }
    //                    links[replaceIndex] = new LinkRecord { instance = instance, start = start, end = end };
    //                }
    //                else {
    //                    // Add new link
    //                    links.Add(new LinkRecord { instance = instance, start = start, end = end });
    //                }
    //            }
    //        }
    //    }
    //}

    public void BuildAll() {
        ClearLinks();

        if (LevelRoot == null) {
            LevelRoot = surface != null ? surface.transform : transform;
        }

        LinkAnchor[] anchors = LevelRoot.GetComponentsInChildren<LinkAnchor>(true);
        int anchorsLength = anchors.Length;

        Debug.Log($"[LinkBuilder] Starting link generation for {anchorsLength} anchors under {LevelRoot.name}");

        for (int i = 0; i < anchorsLength; i++) {
            for (int j = i + 1; j < anchorsLength; j++) {

                // Check: Room root consistency
                Transform roomA = FindRoomRoot(anchors[i].transform);
                Transform roomB = FindRoomRoot(anchors[j].transform);
                if (roomA == null || roomB == null) {
                    Debug.LogWarning($"[LinkBuilder] Skipped pair {anchors[i].name} ↔ {anchors[j].name}: missing RoomData parent");
                    continue;
                }
                if (roomA != roomB) {
                    // Links only within same room root
                    continue;
                }

                Vector3 a = anchors[i].transform.position;
                Vector3 b = anchors[j].transform.position;
                Vector3 delta = b - a;

                float horizontal = new Vector2(delta.x, delta.z).magnitude;
                float vertical = delta.y;

                //  Horizontal and vertical thresholds
                if (horizontal < MinHorizontal || horizontal > MaxHorizontal) {
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}: horizontal {horizontal:F2} outside [{MinHorizontal}, {MaxHorizontal}]");
                    continue;
                }
                if (Mathf.Abs(vertical) > MaxVertical) {
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}: vertical {vertical:F2} > {MaxVertical}");
                    continue;
                }

                //  Jump / drop permissions
                if (vertical > 0.0f && !anchors[i].AllowJump) {
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[i].name} → {anchors[j].name} in room {roomA.name}: upward link but AllowJump is false");
                    continue;
                }
                if (vertical < 0.0f && !anchors[i].AllowDrop) {
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[i].name} → {anchors[j].name} in room {roomA.name}: downward link but AllowDrop is false");
                    continue;
                }

                // Blockage test
                if (LinkBlocked(a, b)) {
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}: line of sight blocked by collider");
                    continue;
                }

                // Snap endpoints to NavMesh
                Vector3 start;
                Vector3 end;

                if (!SampleNavMesh(a, 1.5f, out start)) {
                    Debug.DrawRay(a, Vector3.up * 2f, Color.red, 10f);
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[i].name} in room {roomA.name}: no NavMesh found near {a}");
                    continue;
                }
                if (!SampleNavMesh(b, 1.5f, out end)) {
                    Debug.DrawRay(b, Vector3.up * 2f, Color.red, 10f);
                    Debug.LogWarning($"[LinkBuilder] Skipped {anchors[j].name} in room {roomA.name}: no NavMesh found near {b}");
                    continue;
                }

                // Check if already walkable
                if (SkipIfPathAlreadyWalkable && HasPlainNavMeshPath(start, end, PlainPathAreaMask, MaxDetourRatio)) {
                    Debug.Log($"[LinkBuilder] Skipped walkable pair {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}: already reachable on NavMesh");
                    continue;
                }

                // Redundancy check
                int replaceIndex;
                if (IsRedundantLink(start, end, links, out replaceIndex)) {
                    Debug.Log($"[LinkBuilder] Skipped redundant link {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}");
                    continue;
                }

                // Construct and register link
                NavMeshLinkData data = new NavMeshLinkData {
                    startPosition = start,
                    endPosition = end,
                    width = AgentRadius * 2.0f,
                    bidirectional = Bidirectional,
                    area = 0, // Walkable
                    agentTypeID = surface != null ? surface.agentTypeID : 0
                };

                NavMeshLinkInstance instance = NavMesh.AddLink(data);
                if (instance.valid) {
                    if (replaceIndex >= 0) {
                        if (links[replaceIndex].instance.valid) {
                            links[replaceIndex].instance.Remove();
                        }
                        links[replaceIndex] = new LinkRecord { instance = instance, start = start, end = end };
                        Debug.Log($"[LinkBuilder] Replaced existing link {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}");
                    }
                    else {
                        links.Add(new LinkRecord { instance = instance, start = start, end = end });
                        Debug.Log($"[LinkBuilder] Added new link {anchors[i].name} ↔ {anchors[j].name} (dist {horizontal:F2}, height {vertical:F2}) in room {roomA.name}");
                    }
                }
                else {
                    Debug.LogWarning($"[LinkBuilder] Failed to add link {anchors[i].name} ↔ {anchors[j].name} in room {roomA.name}");
                }
            }
        }

        Debug.Log($"[LinkBuilder] Finished building links. Total valid links: {links.Count}");
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
        //if (Physics.Raycast(a, dir.normalized, dist, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
        //    return true;
        //}
        if (Physics.Raycast(a, dir.normalized, out RaycastHit hit, dist, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
            Debug.LogWarning($"[LinkBuilder] Blocked by {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)} at {hit.point}");
            Debug.DrawLine(a, hit.point, Color.red, 10f);
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
