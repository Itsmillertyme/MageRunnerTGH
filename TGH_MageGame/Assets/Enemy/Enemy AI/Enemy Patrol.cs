using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour, IBehave {
    //**PROPERTIES**
    [SerializeField] float extraTurnSpeed;
    [SerializeField] float playerSearchRadius;

    [SerializeField] List<Vector3> waypointPositions = new List<Vector3>();
    [SerializeField] int currentWaypoint;

    NavMeshAgent agent;
    Animator animator;
    GameObject targetWaypoint;
    GameObject player;

    bool initialized = false;

    //DEV ONLY
    [SerializeField] GameObject debugOrb;

    //**UNITY METHODS**
    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>().gameObject;

        //**ANIM TESTING
        //agent.updatePosition = false;
    }
    //
    private void Update() {
        //Update Animations
        if (agent.velocity.magnitude > 0.1f && !agent.isStopped) {
            animator.SetBool("isWalking", true);
        }
        else {
            animator.SetBool("isWalking", false);
        }

        //Detect player
        if (Vector3.Distance(player.transform.position, transform.position) < playerSearchRadius) {
            agent.SetDestination(transform.position);

            Vector3 direction = (player.transform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 180f); //180 degrees/s
        }
        else {
            if (initialized && waypointPositions.Count > 0) {

                if (Vector3.Distance(waypointPositions[currentWaypoint], transform.position) < agent.stoppingDistance) {

                    Destroy(targetWaypoint);

                    //Cycle to next waypoint
                    currentWaypoint++;
                    currentWaypoint %= waypointPositions.Count;

                    GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
                    if (gm.DebugEnemySpawning) {
                        targetWaypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        targetWaypoint.name = $"Agent target";
                        targetWaypoint.transform.position = waypointPositions[currentWaypoint];
                        targetWaypoint.GetComponent<MeshRenderer>().material.color = Color.red;
                        targetWaypoint.transform.parent = GameObject.Find("DEV").transform;
                    }
                }
                agent.SetDestination(waypointPositions[currentWaypoint]);
            }
        }

        //Fast turning
        Vector3 lookRot = agent.steeringTarget - transform.position;

        if (lookRot != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRot), extraTurnSpeed * Time.deltaTime);
    }
    //
    private void OnAnimatorMove() {
        transform.position = agent.nextPosition;
    }


    //**UTILITY METHODS**    
    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {

        if (spawningDebugMode) Debug.Log("[Enemy Spawning] Initilizing agent from RoomData");
        waypointPositions = new List<Vector3>();
        agent = GetComponent<NavMeshAgent>();

        float roomLeftBound = roomDataIn.TopLeftObject.position.x;
        float roomRightBound = roomDataIn.BottomRightObject.position.x;
        float roomUpBound = roomDataIn.TopLeftObject.position.y;
        float roomDownBound = roomDataIn.BottomRightObject.position.y;
        Vector2 roomCenter = new Vector2((roomLeftBound + roomRightBound) / 2, (roomUpBound + roomDownBound) / 2);

        //Generate points within room
        Vector3 p1 = new Vector3(Random.Range(roomCenter.x, roomRightBound), Random.Range(roomCenter.y, roomUpBound), -2.5f); //Quad 1 (TR from Cam)
        Vector3 p2 = new Vector3(Random.Range(roomCenter.x, roomRightBound), Random.Range(roomDownBound, roomCenter.y), -2.5f); //Quad 2 (BR from Cam)
        Vector3 p3 = new Vector3(Random.Range(roomLeftBound, roomCenter.x), Random.Range(roomDownBound, roomCenter.y), -2.5f); //Quad 3 (BL from Cam)
        Vector3 p4 = new Vector3(Random.Range(roomLeftBound, roomCenter.x), Random.Range(roomCenter.y, roomUpBound), -2.5f); //Quad 4 (TL from Cam)

        //Sample
        NavMeshHit hit;
        if (NavMesh.SamplePosition(p1, out hit, 20, NavMesh.AllAreas)) {
            p1 = hit.position;
        }
        hit = new NavMeshHit();
        if (NavMesh.SamplePosition(p2, out hit, 20, NavMesh.AllAreas)) {
            p2 = hit.position;
        }
        hit = new NavMeshHit();
        if (NavMesh.SamplePosition(p3, out hit, 20, NavMesh.AllAreas)) {
            p3 = hit.position;
        }
        hit = new NavMeshHit();
        if (NavMesh.SamplePosition(p4, out hit, 20, NavMesh.AllAreas)) {
            p4 = hit.position;
        }

        //Add all points to list
        waypointPositions.Add(p1);
        waypointPositions.Add(p2);
        waypointPositions.Add(p3);
        waypointPositions.Add(p4);


        for (int i = waypointPositions.Count - 1; i >= 0; i--) {
            NavMeshPath path = new NavMeshPath();
            if (Application.isPlaying) {
                if (!agent.CalculatePath(waypointPositions[i], path)) {
                    waypointPositions.RemoveAt(i);
                }
            }
            else {
                if (!NavMesh.CalculatePath(agent.transform.position, waypointPositions[i], NavMesh.AllAreas, path)) {
                    waypointPositions.RemoveAt(i);
                }
            }
        }



        //DEV ONLY
        if (spawningDebugMode) {
            GameObject parentObj = new GameObject($"{name}: Waypoints");
            for (int i = 0; i < waypointPositions.Count; i++) {
                GameObject debugWaypoint = Instantiate(debugOrb, waypointPositions[i], Quaternion.identity, parentObj.transform);
                debugWaypoint.name = $"{name}: Waypoint {i + 1}";
            }
            parentObj.transform.parent = transform.parent;
        }

        //Set initial destination
        currentWaypoint = Random.Range(0, waypointPositions.Count);
        if (Application.isPlaying) agent.SetDestination(waypointPositions[currentWaypoint]);
        if (spawningDebugMode) Debug.Log($"[Enemy Spawning] Waypoint {currentWaypoint} set as initial waypoint at {waypointPositions[currentWaypoint]}");

        //Flag
        initialized = true;
    }

}
