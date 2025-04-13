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
        player = GameObject.FindWithTag("Player");

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

                    targetWaypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    targetWaypoint.name = $"Agent target";
                    targetWaypoint.transform.position = waypointPositions[currentWaypoint];
                    targetWaypoint.GetComponent<MeshRenderer>().material.color = Color.red;
                    targetWaypoint.transform.parent = GameObject.Find("DEV").transform;
                }
                agent.SetDestination(waypointPositions[currentWaypoint]);
            }
        }

        //Fast turning
        Vector3 lookRot = agent.steeringTarget - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRot), extraTurnSpeed * Time.deltaTime);
    }
    //
    private void OnAnimatorMove() {
        transform.position = agent.nextPosition;
    }


    //**UTILITY METHODS**    
    public void Initialize(PathNode roomIn, bool debugMode = false) {

        //Debug.Log("Initilizing agent");
        waypointPositions = new List<Vector3>();
        agent = GetComponent<NavMeshAgent>();

        //Establish bounds
        float roomLeftBound = roomIn.transform.position.x + roomIn.RoomDimensions.y / 2;
        float roomRightBound = roomIn.transform.position.x - roomIn.RoomDimensions.y / 2;
        float roomUpBound = roomIn.transform.position.y + roomIn.RoomDimensions.x / 2;
        float roomDownBound = roomIn.transform.position.y - roomIn.RoomDimensions.x / 2;

        //Generate points within room
        Vector3 p1 = new Vector3(Random.Range(roomRightBound, roomIn.transform.position.x), Random.Range(roomIn.transform.position.y, roomUpBound), 1.5f); //Quad 1 (TR from Cam)
        Vector3 p2 = new Vector3(Random.Range(roomRightBound, roomIn.transform.position.x), Random.Range(roomDownBound, roomIn.transform.position.y), 1.5f); //Quad 2 (BR from Cam)
        Vector3 p3 = new Vector3(Random.Range(roomIn.transform.position.x, roomLeftBound), Random.Range(roomDownBound, roomIn.transform.position.y), 1.5f); //Quad 3 (BL from Cam)
        Vector3 p4 = new Vector3(Random.Range(roomIn.transform.position.x, roomLeftBound), Random.Range(roomIn.transform.position.y, roomUpBound), 1.5f); //Quad 4 (TL from Cam)

        //Sample
        NavMeshHit hit;
        if (NavMesh.SamplePosition(p1, out hit, 10, NavMesh.AllAreas)) {
            p1 = hit.position;
        }
        hit = new NavMeshHit();
        if (NavMesh.SamplePosition(p2, out hit, 10, NavMesh.AllAreas)) {
            p2 = hit.position;
        }
        hit = new NavMeshHit();
        if (NavMesh.SamplePosition(p3, out hit, 10, NavMesh.AllAreas)) {
            p3 = hit.position;
        }
        hit = new NavMeshHit();
        if (NavMesh.SamplePosition(p4, out hit, 10, NavMesh.AllAreas)) {
            p4 = hit.position;
        }

        //Add all points to list
        waypointPositions.Add(p1);
        waypointPositions.Add(p2);
        waypointPositions.Add(p3);
        waypointPositions.Add(p4);

        //DEV ONLY
        if (debugMode) {
            Transform parent = GameObject.Find("DEV").transform;
            //Play mode execution
            if (Application.isPlaying) {
                GameObject doQ1 = Instantiate(debugOrb, p1, Quaternion.identity, parent);
                Color q1Color = doQ1.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ1.GetComponent<MeshRenderer>().material.color = new Color(0f, 0.5f, 1f);
                GameObject doQ2 = Instantiate(debugOrb, p2, Quaternion.identity, parent);
                Color q2Color = doQ2.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ2.GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, .33f);
                GameObject doQ3 = Instantiate(debugOrb, p3, Quaternion.identity, parent);
                Color q3Color = doQ3.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ3.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0f);
                GameObject doQ4 = Instantiate(debugOrb, p4, Quaternion.identity, parent);
                Color q4Color = doQ4.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ4.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0f);
            }
            //Editor script execution
            else {
                GameObject doQ1 = Instantiate(debugOrb, p1, Quaternion.identity, parent);
                Color q1Color = doQ1.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ1.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(0f, 0.5f, 1f);
                GameObject doQ2 = Instantiate(debugOrb, p2, Quaternion.identity, parent);
                Color q2Color = doQ2.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ2.GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, .33f);
                GameObject doQ3 = Instantiate(debugOrb, p3, Quaternion.identity, parent);
                Color q3Color = doQ3.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ3.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1f, 1f, 0f);
                GameObject doQ4 = Instantiate(debugOrb, p4, Quaternion.identity, parent);
                Color q4Color = doQ4.GetComponent<MeshRenderer>().sharedMaterial.color;
                doQ4.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1f, 0.5f, 0f);
            }
        }

        //Set initial destination
        currentWaypoint = Random.Range(0, waypointPositions.Count);
        if (Application.isPlaying) {
            agent.SetDestination(waypointPositions[currentWaypoint]);
            targetWaypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            targetWaypoint.name = $"Agent target";
            targetWaypoint.transform.position = waypointPositions[currentWaypoint];
            targetWaypoint.GetComponent<MeshRenderer>().material.color = Color.red;
            targetWaypoint.transform.parent = GameObject.Find("DEV").transform;
        }

        //Flag
        initialized = true;
    }

}
