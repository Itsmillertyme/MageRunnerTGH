using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour, IBehave {
    [SerializeField] int maxSearchDistance;
    [SerializeField] float edgeOffset;

    List<Vector3> waypointPositions = new List<Vector3>();
    NavMeshAgent agent;
    int currentWaypoint;

    bool initialized = false;

    //DEV ONLY
    [SerializeField] bool debugMode;

    private void Update() {
        if (initialized) {
            if (agent.remainingDistance < agent.stoppingDistance) {
                //Cycle to next waypoint
                currentWaypoint++;
                currentWaypoint %= waypointPositions.Count;

                agent.SetDestination(waypointPositions[currentWaypoint]);
            }
        }
    }
    public void Initialize() {

        //Helpers
        agent = GetComponent<NavMeshAgent>();
        Vector3 destination;

        //Setup waypoints       
        //*LEFT*
        Vector3 targetLPosition = transform.position + (Vector3.left * maxSearchDistance);
        NavMeshHit lHit;
        bool isHitLeft = NavMesh.Raycast(transform.position, targetLPosition, out lHit, 9); // 9 is ground layer

        if (isHitLeft) {
            //Offset from edge
            destination = lHit.position - (Vector3.left * edgeOffset);

            //Ensure still on mesh
            if (NavMesh.SamplePosition(destination, out NavMeshHit checkHit, 1f, 9)) {// 9 is ground layer
                waypointPositions.Add(checkHit.position);
            }
            else {
                Debug.Log($"FAILED TO LOCATE LEFT PATROL WAYPOINT FOR {gameObject.name} FROM POSITION {transform.position}");
                return;
            }
        }
        else {
            //No boundary in range
            Debug.Log($"LEFT CHECK FAILED TO FIND END OF NAVMESH, PLACING AT MAX SEARCH RANGE");
            waypointPositions.Add(targetLPosition);
        }
        //*RIGHT*
        Vector3 targetRPosition = transform.position + (-Vector3.left * maxSearchDistance);
        NavMeshHit rHit;
        bool isHitRight = NavMesh.Raycast(transform.position, targetRPosition, out rHit, 9); // 9 is ground layer

        if (isHitRight) {
            //Offset from edge
            destination = rHit.position - (-Vector3.left * edgeOffset);

            //Ensure still on mesh
            if (NavMesh.SamplePosition(destination, out NavMeshHit checkHit, 1f, 9)) {// 9 is ground layer
                waypointPositions.Add(checkHit.position);
            }
            else {
                Debug.Log($"FAILED TO LOCATE RIGHT PATROL WAYPOINT FOR {gameObject.name} FROM POSITION {transform.position}");
                return;
            }
        }
        else {
            //No boundary in range

            Debug.Log($"RIGHT CHECK FAILED TO FIND END OF NAVMESH, PLACING AT MAX SEARCH RANGE");
            waypointPositions.Add(targetRPosition);
        }

        //DEV ONLY
        if (debugMode) {
            Debug.Log($"Waypoints:\n\t{waypointPositions[0]}\n\t{waypointPositions[1]}");

            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            Transform parent = GameObject.Find("WaypointsDEV").transform;
            //
            GameObject lWaypoint = new GameObject("Left Waypoint", typeof(MeshFilter), typeof(MeshRenderer));
            lWaypoint.GetComponent<MeshFilter>().mesh = gm.debugObjectMesh;
            lWaypoint.GetComponent<MeshRenderer>().material = gm.debugMaterial;
            lWaypoint.transform.position = waypointPositions[0];
            lWaypoint.transform.parent = parent;
            //
            GameObject rWaypoint = new GameObject("Right Waypoint", typeof(MeshFilter), typeof(MeshRenderer));
            rWaypoint.GetComponent<MeshFilter>().mesh = gm.debugObjectMesh;
            rWaypoint.GetComponent<MeshRenderer>().material = gm.debugMaterial;
            rWaypoint.transform.position = waypointPositions[1];
            rWaypoint.transform.parent = parent;
        }


        //Set initial destination
        currentWaypoint = Random.Range(0, 1);
        agent.SetDestination(waypointPositions[currentWaypoint]);

        initialized = true;
    }
}
