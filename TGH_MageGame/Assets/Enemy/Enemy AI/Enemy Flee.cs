using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFlee : MonoBehaviour, IBehave {
    //**PROPERTIES**    
    [SerializeField] float extraTurnSpeed;
    [SerializeField] float playerDetectionRadius;
    [SerializeField] float playerForgetRadius;

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
    public void Initialize(PathNode roomIn, bool debugMode = false) {

        //Helper
        float navMeshSampleRadius = 6f;

        //Find corner points
        Vector3 cBottomLeft = new Vector3(roomIn.RoomTopLeftCorner.y - 3, roomIn.RoomTopLeftCorner.x + 3, 1.5f);
        Vector3 cTopLeft = new Vector3(roomIn.RoomTopLeftCorner.y - 3, roomIn.RoomTopLeftCorner.x + roomIn.RoomDimensions.x - 3, 1.5f);
        Vector3 cTopRight = new Vector3(roomIn.RoomTopLeftCorner.y - roomIn.RoomDimensions.y + 3, roomIn.RoomTopLeftCorner.x + roomIn.RoomDimensions.x - 3, 1.5f);
        Vector3 cBottomRight = new Vector3(roomIn.RoomTopLeftCorner.y - roomIn.RoomDimensions.y + 3, roomIn.RoomTopLeftCorner.x + 3, 1.5f);

        //Sample
        NavMeshHit hit;
        if (NavMesh.SamplePosition(cBottomLeft, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
            cBottomLeft = hit.position;
        }
        if (NavMesh.SamplePosition(cTopLeft, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
            cTopLeft = hit.position;
        }
        if (NavMesh.SamplePosition(cTopRight, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
            cTopRight = hit.position;
        }
        if (NavMesh.SamplePosition(cBottomRight, out hit, navMeshSampleRadius, NavMesh.AllAreas)) {
            cBottomRight = hit.position;
        }

        //GameObject debug1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //debug1.transform.position = cBottomLeft;
        //GameObject debug2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //debug2.transform.position = cTopLeft;
        //GameObject debug3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //debug3.transform.position = cTopRight;
        //GameObject debug4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //debug4.transform.position = cBottomRight;

        cornerPoints.Add(cBottomLeft);
        cornerPoints.Add(cTopLeft);
        cornerPoints.Add(cTopRight);
        cornerPoints.Add(cBottomRight);

        //Flag
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

    public void Initialize(RoomData roomDataIn, bool debugMode = false) {
        throw new System.NotImplementedException();
    }
}
