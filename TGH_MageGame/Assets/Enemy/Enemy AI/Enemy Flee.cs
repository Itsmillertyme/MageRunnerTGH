using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFlee : MonoBehaviour, IBehave {
    //**PROPERTIES**    
    [SerializeField] float extraTurnSpeed;
    [SerializeField] float playerDetectionRadius;
    [SerializeField] float playerForgetRadius;
    [SerializeField] GameObject debugOrb;

    NavMeshAgent agent;
    Animator animator;
    GameObject player;

    List<Vector3> cornerPoints = new List<Vector3>();

    bool initialized = false;
    bool playerInRange = false;


    GameObject debugDestination;

    //**UNITY METHODS**
    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    private void Update() {

        //Update Animations
        if (agent.velocity.magnitude > 0.1f && !agent.isStopped) {
            animator.SetBool("isWalking", true);
        }
        else {
            animator.SetBool("isWalking", false);
        }

        //Look for player
        if (!playerInRange && Vector3.Distance(player.transform.position, transform.position) < playerDetectionRadius) {
            playerInRange = true;
        }
        if (playerInRange && Vector3.Distance(player.transform.position, transform.position) > playerForgetRadius) {
            playerInRange = false;
        }

        DestroyImmediate(debugDestination);

        //Set destination
        if (playerInRange) {
            //Flee
            agent.destination = GetFarthersPointFromPlayer();

            debugDestination = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugDestination.transform.position = agent.destination;
        }
        else {
            //Hold position
            agent.SetDestination(transform.position);
        }

    }


    //**UTILITY METHODS**   
    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {

        ////Helper
        //float navMeshSampleRadius = 6f;

        ////Find corner points
        //Vector3 cBottomLeft = new Vector3(roomDataIn.TopLeftObject.position.x, roomDataIn.BottomRightObject.position.y, -2.5f);
        //Vector3 cTopLeft = new Vector3(roomDataIn.TopLeftObject.position.x, roomDataIn.TopLeftObject.position.y, -2.5f);
        //Vector3 cTopRight = new Vector3(roomDataIn.BottomRightObject.position.x, roomDataIn.TopLeftObject.position.y, -2.5f);
        //Vector3 cBottomRight = new Vector3(roomDataIn.BottomRightObject.position.x, roomDataIn.BottomRightObject.position.y, -2.5f);

        ////Sample
        //NavMeshHit hit;
        //if (NavMesh.SamplePosition(cBottomLeft, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
        //    cBottomLeft = hit.position;
        //}
        //if (NavMesh.SamplePosition(cTopLeft, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
        //    cTopLeft = hit.position;
        //}
        //if (NavMesh.SamplePosition(cTopRight, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
        //    cTopRight = hit.position;
        //}
        //if (NavMesh.SamplePosition(cBottomRight, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
        //    cBottomRight = hit.position;
        //}

        ////DEBUG

        //cornerPoints.Add(cBottomLeft);
        //cornerPoints.Add(cTopLeft);
        //cornerPoints.Add(cTopRight);
        //cornerPoints.Add(cBottomRight);

        ////Flag
        //initialized = true;

        float navMeshSampleRadius = 6f;
        int maxRetries = 5;

        // Calculate room center
        Vector3 roomCenter = (roomDataIn.TopLeftObject.position + roomDataIn.BottomRightObject.position) / 2f;
        roomCenter.z = -2.5f;

        // Define corners from room data
        List<Vector3> rawCorners = new List<Vector3>
        {
        new Vector3(roomDataIn.TopLeftObject.position.x, roomDataIn.BottomRightObject.position.y, -2.5f),  // bottom left
        new Vector3(roomDataIn.TopLeftObject.position.x, roomDataIn.TopLeftObject.position.y, -2.5f),      // top left
        new Vector3(roomDataIn.BottomRightObject.position.x, roomDataIn.TopLeftObject.position.y, -2.5f),  // top right
        new Vector3(roomDataIn.BottomRightObject.position.x, roomDataIn.BottomRightObject.position.y, -2.5f) // bottom right
        };

        cornerPoints.Clear();
        GameObject parentObj = new GameObject($"{name}: Waypoints");

        foreach (Vector3 corner in rawCorners) {
            Vector3 sampled = corner;
            NavMeshHit hit;
            bool foundValid = false;

            int retries = 0;
            while (retries < maxRetries) {
                if (NavMesh.SamplePosition(sampled, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
                    if (IsInsideRoomBounds(hit.position, roomDataIn)) {
                        sampled = hit.position;
                        foundValid = true;
                        break;
                    }
                }

                // Nudge 20% closer to room center each retry
                sampled = Vector3.Lerp(sampled, roomCenter, 0.2f);
                retries++;
            }

            if (foundValid) {
                cornerPoints.Add(sampled);
                if (spawningDebugMode) {
                    // show shift from raw corner to final
                    Debug.DrawLine(corner, sampled, Color.green, 15f);
                    // mark final accepted point
                    GameObject debugWaypoint = Instantiate(debugOrb, sampled, Quaternion.identity, parentObj.transform);
                    debugWaypoint.name = $"{name}: Corner Point";
                }
            }
            else if (spawningDebugMode) {
                Debug.LogWarning($"[Enemy Spawning] No valid NavMesh point found for corner near {corner}");
            }
        }
        parentObj.transform.parent = transform.parent;

        initialized = true;
    }
    //
    Vector3 GetFarthersPointFromPlayer() {

        //Helpers
        float dist = Mathf.NegativeInfinity;
        Vector3 farthestPoint = Vector3.zero;

        foreach (Vector3 point in cornerPoints) {
            float currentDist = Vector3.Distance(player.transform.position, point);
            if (currentDist > dist) {
                dist = currentDist;
                farthestPoint = point;
            }
        }

        return farthestPoint;
    }

    bool IsInsideRoomBounds(Vector3 point, RoomData room) {
        float minX = room.TopLeftObject.position.x;
        float maxX = room.BottomRightObject.position.x;
        float minY = room.BottomRightObject.position.y;
        float maxY = room.TopLeftObject.position.y;

        return (point.x >= minX && point.x <= maxX &&
                point.y >= minY && point.y <= maxY);
    }


}
