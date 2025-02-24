using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour, IBehave {
    [SerializeField] int maxSearchDistance;
    [SerializeField] float edgeOffset;
    [SerializeField] float extraTurnSpeed;
    [SerializeField] int numWaypoints;

    List<Vector3> waypointPositions = new List<Vector3>();
    NavMeshAgent agent;
    int currentWaypoint;

    bool initialized = false;

    //DEV ONLY
    [SerializeField] GameObject debugOrb;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        if (initialized && waypointPositions.Count > 0) {
            if (agent.remainingDistance < agent.stoppingDistance) {
                //Cycle to next waypoint
                currentWaypoint++;
                currentWaypoint %= waypointPositions.Count;

                agent.SetDestination(waypointPositions[currentWaypoint]);
            }
        }

        //Fast turning
        Vector3 lookRot = agent.steeringTarget - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRot), extraTurnSpeed * Time.deltaTime);
    }

    //public void Initialize(bool debugMode = false) {


    //    //Helpers
    //    agent = GetComponent<NavMeshAgent>();
    //    Vector3 destination;

    //    //Setup waypoints       
    //    //*LEFT*
    //    Vector3 targetLPosition = transform.position + (Vector3.left * maxSearchDistance);
    //    NavMeshHit lHit;
    //    bool isHitLeft = NavMesh.Raycast(transform.position, targetLPosition, out lHit, 9); // 9 is ground layer

    //    NavMeshLink closestLink;
    //    Vector3 closestPoint = GetClosestNavMeshLinkToPoint(targetLPosition, GameObject.FindWithTag("Level"), out closestLink);
    //    float distance = Vector3.Distance(closestPoint, targetLPosition);

    //    Debug.Log($"Distance to left target position: {distance}");
    //    if (distance < 15) {

    //        Debug.Log($"Target point: {targetLPosition}");
    //        Debug.Log($"Closest Link point: {closestPoint}");
    //        Vector3 otherSideOfLink = closestLink.startPoint == closestPoint ? closestLink.endPoint : closestLink.startPoint;
    //        waypointPositions.Add(otherSideOfLink);
    //    }
    //    else if (isHitLeft) {
    //        //Offset from edge
    //        destination = lHit.position - (Vector3.left * edgeOffset);

    //        //Ensure still on mesh
    //        if (NavMesh.SamplePosition(destination, out NavMeshHit checkHit, 1f, 9)) {// 9 is ground layer
    //            waypointPositions.Add(checkHit.position);
    //        }
    //        else {
    //            //Debug.Log($"FAILED TO LOCATE LEFT PATROL WAYPOINT FOR {gameObject.name} FROM POSITION {transform.position}");
    //            return;
    //        }
    //    }
    //    else {
    //        //No boundary in range
    //        //Debug.Log($"LEFT CHECK FAILED TO FIND END OF NAVMESH, PLACING AT MAX SEARCH RANGE");
    //        waypointPositions.Add(targetLPosition);
    //    }
    //    //*RIGHT*
    //    Vector3 targetRPosition = transform.position + (-Vector3.left * maxSearchDistance);
    //    NavMeshHit rHit;
    //    bool isHitRight = NavMesh.Raycast(transform.position, targetRPosition, out rHit, 9); // 9 is ground layer

    //    if (isHitRight) {
    //        //Offset from edge
    //        destination = rHit.position - (-Vector3.left * edgeOffset);

    //        //Ensure still on mesh
    //        if (NavMesh.SamplePosition(destination, out NavMeshHit checkHit, 1f, 9)) {// 9 is ground layer
    //            waypointPositions.Add(checkHit.position);
    //        }
    //        else {
    //            //Debug.Log($"FAILED TO LOCATE RIGHT PATROL WAYPOINT FOR {gameObject.name} FROM POSITION {transform.position}");
    //            return;
    //        }
    //    }
    //    else {
    //        //No boundary in range

    //        //Debug.Log($"RIGHT CHECK FAILED TO FIND END OF NAVMESH, PLACING AT MAX SEARCH RANGE");
    //        waypointPositions.Add(targetRPosition);
    //    }

    //    //DEV ONLY
    //    if (debugMode) {
    //        //Debug.Log($"Waypoints:\n\t{waypointPositions[0]}\n\t{waypointPositions[1]}");

    //        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    //        Transform parent = GameObject.Find("WaypointsDEV").transform;
    //        //
    //        GameObject lWaypoint = new GameObject("Left Waypoint", typeof(MeshFilter), typeof(MeshRenderer));
    //        lWaypoint.GetComponent<MeshFilter>().mesh = gm.debugObjectMesh;
    //        lWaypoint.GetComponent<MeshRenderer>().material = gm.debugMaterial;
    //        lWaypoint.transform.position = waypointPositions[0];
    //        lWaypoint.transform.parent = parent;
    //        //
    //        GameObject rWaypoint = new GameObject("Right Waypoint", typeof(MeshFilter), typeof(MeshRenderer));
    //        rWaypoint.GetComponent<MeshFilter>().mesh = gm.debugObjectMesh;
    //        rWaypoint.GetComponent<MeshRenderer>().material = gm.debugMaterial;
    //        rWaypoint.transform.position = waypointPositions[1];
    //        rWaypoint.transform.parent = parent;
    //    }


    //    //Set initial destination
    //    currentWaypoint = Random.Range(0, 1);
    //    agent.SetDestination(waypointPositions[currentWaypoint]);

    //    initialized = true;
    //}
    public void Initialize(PathNode roomIn, bool debugMode = false) {

        Debug.Log("Initilizing");

        //Helpers

        //Generate 4 waypoints, 1 in each quadrant. 
        //Vector3 quad1 =


        //GameObject topLeftCorner = new GameObject($"{roomIn.name}'s Top Left");
        //topLeftCorner.transform.position = new Vector3(roomIn.RoomTopLeftCorner.x, roomIn.RoomTopLeftCorner.y, 2.5f);


        //Debug.Log($"{roomIn.name}'s Position: {roomIn.transform.position}");


        //GameObject bottomRightCorner = new GameObject($"{roomIn.name}'s Bottom Right");
        //bottomRightCorner.transform.position = new Vector3(roomIn.transform.position.x - roomIn.RoomDimensions.y / 2, roomIn.transform.position.y + roomIn.RoomDimensions.x / 2, 2f);
        //GameObject bottomLeftCorner = new GameObject($"{roomIn.name}'s Bottom Left");
        //bottomLeftCorner.transform.position = new Vector3(roomIn.transform.position.x - roomIn.RoomDimensions.y / 2, roomIn.transform.position.y - roomIn.RoomDimensions.x / 2, 2f);
        //GameObject topLeftCorner = new GameObject($"{roomIn.name}'s Top Left");
        //topLeftCorner.transform.position = new Vector3(roomIn.transform.position.x + roomIn.RoomDimensions.y / 2, roomIn.transform.position.y - roomIn.RoomDimensions.x / 2, 2f);
        //GameObject topRightCorner = new GameObject($"{roomIn.name}'s Top Right");
        //topRightCorner.transform.position = new Vector3(roomIn.transform.position.x + roomIn.RoomDimensions.y / 2, roomIn.transform.position.y + roomIn.RoomDimensions.x / 2, 2f);



        Vector3 quad1 = new Vector3(roomIn.transform.position.x - roomIn.RoomDimensions.y / 2, roomIn.transform.position.y + roomIn.RoomDimensions.x / 2, 2f);
        Vector3 quad2 = new Vector3(roomIn.transform.position.x - roomIn.RoomDimensions.y / 2, roomIn.transform.position.y - roomIn.RoomDimensions.x / 2, 2f);
        Vector3 quad3 = new Vector3(roomIn.transform.position.x + roomIn.RoomDimensions.y / 2, roomIn.transform.position.y - roomIn.RoomDimensions.x / 2, 2f);
        Vector3 quad4 = new Vector3(roomIn.transform.position.x + roomIn.RoomDimensions.y / 2, roomIn.transform.position.y + roomIn.RoomDimensions.x / 2, 2f);

        float roomLeftBound = roomIn.transform.position.x + roomIn.RoomDimensions.y / 2;
        float roomRightBound = roomIn.transform.position.x - roomIn.RoomDimensions.y / 2;
        float roomUpBound = roomIn.transform.position.y + roomIn.RoomDimensions.x / 2;
        float roomDownBound = roomIn.transform.position.y - roomIn.RoomDimensions.x / 2;

        Vector3 p1 = new Vector3(Random.Range(roomRightBound, roomIn.transform.position.x), Random.Range(roomIn.transform.position.y, roomUpBound), 2f); //Quad 1 (TR from Cam)
        Vector3 p2 = new Vector3(Random.Range(roomRightBound, roomIn.transform.position.x), Random.Range(roomDownBound, roomIn.transform.position.y), 2f); //Quad 2 (BR from Cam)
        Vector3 p3 = new Vector3(Random.Range(roomIn.transform.position.x, roomLeftBound), Random.Range(roomDownBound, roomIn.transform.position.y), 2f); //Quad 3 (BL from Cam)
        Vector3 p4 = new Vector3(Random.Range(roomIn.transform.position.x, roomLeftBound), Random.Range(roomIn.transform.position.y, roomUpBound), 2f); //Quad 4 (TL from Cam)


        NavMeshHit hit;
        //sample p1
        if (NavMesh.SamplePosition(p1, out hit, 11f, NavMesh.AllAreas)) {
            p1 = hit.position;
        }
        else {
            Debug.Log($"{p1} cannot find Navmesh");
        }

        //sample p2
        if (NavMesh.SamplePosition(p2, out hit, 11f, NavMesh.AllAreas)) {
            p2 = hit.position;
        }
        else {
            Debug.Log($"{p2} cannot find Navmesh");
        }

        //sample p3
        if (NavMesh.SamplePosition(p3, out hit, 11f, NavMesh.AllAreas)) {
            p3 = hit.position;
        }
        else {
            Debug.Log($"{p3} cannot find Navmesh");
        }

        //sample p4
        if (NavMesh.SamplePosition(p4, out hit, 11f, NavMesh.AllAreas)) {
            p4 = hit.position;
        }
        else {
            Debug.Log($"{p4} cannot find Navmesh");
        }

        waypointPositions.Add(p1);
        waypointPositions.Add(p2);
        waypointPositions.Add(p3);
        waypointPositions.Add(p4);


        if (debugMode) {
            Transform parent = GameObject.Find("WaypointsDEV").transform;
            GameObject doQ1 = Instantiate(debugOrb, p1, Quaternion.identity, parent);
            Color q1Color = doQ1.GetComponent<MeshRenderer>().sharedMaterial.color;
            doQ1.GetComponent<MeshRenderer>().material.color = new Color(0f, 0.5f, 1f);
            GameObject doQ2 = Instantiate(debugOrb, p2, Quaternion.identity, parent);
            Color q2Color = doQ2.GetComponent<MeshRenderer>().sharedMaterial.color;
            doQ2.GetComponent<MeshRenderer>().material.color = new Color(q2Color.r, q2Color.g, q2Color.b);
            GameObject doQ3 = Instantiate(debugOrb, p3, Quaternion.identity, parent);
            Color q3Color = doQ3.GetComponent<MeshRenderer>().sharedMaterial.color;
            doQ3.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0f);
            GameObject doQ4 = Instantiate(debugOrb, p4, Quaternion.identity, parent);
            Color q4Color = doQ4.GetComponent<MeshRenderer>().sharedMaterial.color;
            doQ4.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0f);
        }

        //Set initial destination
        currentWaypoint = Random.Range(0, waypointPositions.Count);
        agent.SetDestination(waypointPositions[currentWaypoint]);

        initialized = true;
    }


}
