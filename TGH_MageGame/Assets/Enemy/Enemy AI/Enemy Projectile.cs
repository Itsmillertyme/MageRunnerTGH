using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyProjectile : MonoBehaviour, IBehave {
    //**PROPERTIES**    
    [SerializeField] float attackCoolDown;
    [SerializeField] float attackRadius;
    [SerializeField] int damage;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;

    Animator animator;
    NavMeshAgent agent;
    GameObject player;

    [SerializeField] bool initialized = false;
    [SerializeField] bool attackReady = true;
    [SerializeField] bool playerInRange = false;

    //**UNITY METHODS**
    private void Awake() {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
    }

    private void Update() {

        //Check if initialized
        if (initialized) {
            //Look for player
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            if (distanceToPlayer < attackRadius) {
                //Set flag
                playerInRange = true;

                //Is attack ready
                if (attackReady) {
                    attackReady = false;
                    StartCoroutine(DoRanged());
                }
            }
            else {
                playerInRange = false;
            }
        }
    }


    //**UTILITY METHODS**

    //**COROUTINES**
    IEnumerator DoRanged() {

        //Move to Idle animation 
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            animator.CrossFade("Idle", 0f);
        }

        //Get target 
        Vector3 targetPosition = player.transform.position + new Vector3(0f, 1f, 0f);

        //look at player
        agent.updateRotation = false;

        //Snap look at the player
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }


        //Trigger animation
        animator.SetTrigger("attack");

        //Wait for animation
        yield return new WaitForSeconds(11 / 30f);

        //test if still in range
        if (playerInRange) {

            //Spawn projectile
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition, damage);
        }

        agent.updateRotation = true;

        //Wait for cooldown
        yield return new WaitForSeconds(attackCoolDown);

        //Set flag
        attackReady = true;
    }

    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {
        throw new System.NotImplementedException();
    }
}
