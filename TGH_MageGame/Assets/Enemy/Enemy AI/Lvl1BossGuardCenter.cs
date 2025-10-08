using UnityEngine;
using UnityEngine.AI;

public class Lvl1BossGuardCenter : MonoBehaviour, IBehave {

    //**PROPERTIES**    
    [SerializeField] float extraTurnSpeed;
    [SerializeField] float playerSearchRadius;

    [SerializeField] Vector3 guardPosition;

    NavMeshAgent agent;
    Animator animator;
    GameObject player;

    bool initialized = false;
    bool playerInRange = false;

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

        if (initialized) {
            //Look for player
            if (Vector3.Distance(player.transform.position, transform.position) < playerSearchRadius) {
                playerInRange = true;
            }
            else {
                playerInRange = false;
            }

            //Set destination
            if (playerInRange) {
                //Chase
                if (agent.destination != player.transform.position) {
                    agent.SetDestination(player.transform.position);
                }
            }
            else {
                //Move go to guard position
                if (agent.destination != guardPosition) {
                    agent.SetDestination(guardPosition);
                }
            }
        }
    }


    //**UTILITY METHODS**

    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {
        //Helpers
        Vector3 guardPosition = new Vector3(roomDataIn.PathNode.transform.position.x, roomDataIn.BottomRightObject.transform.position.y + 0.5f, -2.5f);

        //find lower platform
        RaycastHit rayHit;
        if (Physics.Raycast(guardPosition, Vector3.down, out rayHit, 20f)) {
            guardPosition.y = rayHit.point.y;
        }

        //Sample Navmesh
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(guardPosition, out navMeshHit, 1.0f, NavMesh.AllAreas)) {
            guardPosition = navMeshHit.position;
        }

        //Set property
        this.guardPosition = guardPosition;

        //Flag
        initialized = true;
    }
}