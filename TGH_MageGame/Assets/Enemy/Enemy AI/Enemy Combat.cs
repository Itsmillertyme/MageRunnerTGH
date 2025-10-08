using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCombat : MonoBehaviour, IBehave {
    //**PROPERTIES**    
    [Header("Attack Settings")]
    [SerializeField] float attackCoolDown;
    [SerializeField] float attackRadius;
    [SerializeField] int damage;
    [SerializeField] AttackType attackType;
    //
    [Header("Component References")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    //
    [Header("Debug - DO NOT INTERACT")]
    [SerializeField] bool initialized = false;
    [SerializeField] bool attackReady = true;
    [SerializeField] bool playerInRange = false;
    //
    Animator animator;
    NavMeshAgent agent;
    GameObject player;
    bool aiDebugMode;
    bool spawningDebugMode;

    //**UNITY METHODS**
    private void Awake() {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
    }
    //
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
                    StartCoroutine(SetupAttack());
                }
            }
            else {
                playerInRange = false;
            }
        }
    }

    //**UTILITY METHODS**    
    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {
        this.aiDebugMode = aiDebugMode;
        this.spawningDebugMode = spawningDebugMode;

        //Flag
        initialized = true;
    }
    //
    void DoMeleeAttack() {
        //test if still in range
        if (playerInRange) {
            //apply damage
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            //Remove health
            if (playerHealth != null) {
                playerHealth.RemoveFromHealth(damage);
            }
        }

        agent.updateRotation = true;

        StartCoroutine(DoCooldown());
    }
    //
    void DoRangedAttack(Vector3 target) {
        //test if still in range
        if (playerInRange) {

            //Spawn projectile
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            projectile.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, target, damage);
        }

        agent.updateRotation = true;

        StartCoroutine(DoCooldown());
    }

    //**COROUTINES**
    IEnumerator DoCooldown() {

        if (aiDebugMode) Debug.Log($"[Enemy AI] {name} attack cooldown begins");

        //Wait for cooldown
        yield return new WaitForSeconds(attackCoolDown);

        if (aiDebugMode) Debug.Log($"[Enemy AI] {name} attack ready");

        //Set flag
        attackReady = true;
    }
    //
    //IEnumerator DoMelee() {
    //    //Trigger animation
    //    animator.SetTrigger("attack");

    //    //Wait for animation
    //    yield return new WaitForSeconds(11 / 30f);

    //    //test if still in range
    //    if (playerInRange) {
    //        //apply damage
    //        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

    //        //Remove health
    //        if (playerHealth != null) {
    //            playerHealth.RemoveFromHealth(damage);
    //        }
    //    }

    //    //Wait for cooldown
    //    yield return new WaitForSeconds(attackCoolDown);

    //    //Set flag
    //    attackReady = true;
    //}
    ////
    //IEnumerator DoRanged() {

    //    //Move to Idle animation 
    //    if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
    //        animator.CrossFade("Idle", 0f);
    //    }

    //    //Get target 
    //    Vector3 targetPosition = player.transform.position + new Vector3(0f, 1f, 0f);

    //    //look at player
    //    agent.updateRotation = false;

    //    //Snap look at the player
    //    Vector3 direction = (targetPosition - transform.position).normalized;
    //    direction.y = 0;
    //    if (direction != Vector3.zero) {
    //        Quaternion lookRotation = Quaternion.LookRotation(direction);
    //        transform.rotation = lookRotation;
    //    }


    //    //Trigger animation
    //    animator.SetTrigger("attack");

    //    //Wait for animation
    //    yield return new WaitForSeconds(11 / 30f);

    //    //test if still in range
    //    if (playerInRange) {

    //        //Spawn projectile
    //        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
    //        projectile.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition, damage);
    //    }

    //    agent.updateRotation = true;

    //    //Wait for cooldown
    //    yield return new WaitForSeconds(attackCoolDown);

    //    //Set flag
    //    attackReady = true;
    //}
    ////
    IEnumerator SetupAttack() {
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

        if (attackType == AttackType.Melee) {
            DoMeleeAttack();
        }
        else if (attackType == AttackType.Ranged) {
            DoRangedAttack(targetPosition);
        }
    }
}

public enum AttackType {
    Melee,
    Ranged,
    Super
}
