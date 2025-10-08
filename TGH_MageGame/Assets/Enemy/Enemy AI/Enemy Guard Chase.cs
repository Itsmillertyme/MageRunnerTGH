using UnityEngine;
using UnityEngine.AI;

public class EnemyGuardChase : MonoBehaviour, IBehave {

    //**PROPERTIES**    
    [SerializeField] float extraTurnSpeed;
    [SerializeField] float playerSearchRadius;
    [SerializeField] GameObject debugOrb;
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

            float stopDistance = 3f;
            //Set destination
            if (playerInRange) {
                float distToPlayer = Vector3.Distance(agent.transform.position, player.transform.position);

                if (distToPlayer > stopDistance) {
                    // Chase until within stopDistance
                    if (agent.destination != player.transform.position) {
                        agent.SetDestination(player.transform.position);
                    }
                }
                else {
                    agent.SetDestination(transform.position);
                }
            }
            else {
                // Return to guard position
                if (agent.destination != guardPosition) {
                    agent.SetDestination(guardPosition);
                }
            }

        }
    }


    //**UTILITY METHODS**    
    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {
        //Add POIs retrieval from GameManager in future, get navmesh point closest to center of the room        

        //Helpers
        Vector3 guardPosition = new Vector3(roomDataIn.PathNode.position.x, roomDataIn.PathNode.position.y, -2.5f);

        //Sample Navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(guardPosition, out hit, 10f, NavMesh.AllAreas)) {
            guardPosition = hit.position;
        }

        //DEV ONLY
        if (spawningDebugMode) {
            GameObject debugWaypoint = Instantiate(debugOrb, guardPosition, Quaternion.identity, transform.parent);
            debugWaypoint.name = $"{name}: Guard Position";
        }

        //Set property
        this.guardPosition = guardPosition;

        //Flag
        initialized = true;
    }
}
