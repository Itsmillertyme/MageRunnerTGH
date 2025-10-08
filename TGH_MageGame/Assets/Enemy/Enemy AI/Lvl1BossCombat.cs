using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Lvl1BossCombat : MonoBehaviour, IBehave {
    //**PROPERTIES**    
    [Header("Attack Settings")]
    [SerializeField] float meleeAttackCoolDown;
    [SerializeField] float rangedAttackCoolDown;
    [SerializeField] float superAttackCoolDown;
    [SerializeField] float meleeAttackRadius;
    [SerializeField] float rangedAttackRadius;
    [SerializeField] int meleeDamage;
    [SerializeField] int rangedDamage;
    [SerializeField] float superAttackChance;

    //
    [Header("Component References")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] GameObject weaponParticles;
    //
    [Header("Debug - DO NOT INTERACT")]
    [SerializeField] bool initialized = false;
    [SerializeField] bool attackReady = true;
    [SerializeField] bool superAttackReady = true;
    [SerializeField] bool playerInMeleeRange = false;
    [SerializeField] bool playerInRangedRange = false;
    //
    Animator animator;
    NavMeshAgent agent;
    GameObject player;

    //**UNITY METHODS**
    private void Awake() {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
        weaponParticles.SetActive(false);
    }
    //
    private void Update() {

        //Check if initialized
        if (initialized && !GetComponent<BossHealth>().IsDead) {
            //Look for player
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

            //Set range flags based on distance
            if (distanceToPlayer < meleeAttackRadius) {
                playerInMeleeRange = true;
            }
            else {
                playerInMeleeRange = false;
            }

            if (distanceToPlayer < rangedAttackRadius) {
                playerInRangedRange = true;
            }
            else {
                playerInRangedRange = false;
            }

            //do attacks
            if (attackReady) {
                //Prioritizes melee attacks if in range
                if (playerInMeleeRange) {
                    StartCoroutine(SetupAttack(AttackType.Melee));
                }
                else if (playerInRangedRange) {

                    //Roll for super attack
                    if (superAttackReady && Random.value < superAttackChance) {
                        superAttackReady = false;
                        StartCoroutine(SetupAttack(AttackType.Super));
                    }
                    //Normal ranged attack
                    else {
                        StartCoroutine(SetupAttack(AttackType.Ranged));
                    }
                }
            }

        }
    }

    //**UTILITY METHODS**
    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {
        //Flag
        initialized = true;
    }
    //
    void DoMeleeAttack() {
        //test if still in range
        if (playerInMeleeRange) {
            //apply damage
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            //Remove health
            if (playerHealth != null) {
                playerHealth.RemoveFromHealth(meleeDamage);
            }
        }

        agent.updateRotation = true;

        StartCoroutine(DoCooldown(meleeAttackCoolDown));
    }
    //
    void DoRangedAttack(Vector3 target) {

        //Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, target, rangedDamage);

        agent.updateRotation = true;

        StartCoroutine(DoCooldown(rangedAttackCoolDown));
    }

    //**COROUTINES**
    IEnumerator DoCooldown(float cooldown) {
        //Wait for cooldown
        yield return new WaitForSeconds(cooldown);
        //Set flag
        attackReady = true;
    }
    //
    IEnumerator SetupAttack(AttackType attackType) {
        attackReady = false;

        //Move to Idle animation 
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
            animator.CrossFade("Idle", 0f);
        }

        //Get target 
        Vector3 targetPosition = player.transform.position + new Vector3(0f, 2f, 0f);

        //look at player
        agent.updateRotation = false;

        //Snap look at the player
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }

        if (attackType == AttackType.Melee) {
            //Trigger animation
            animator.SetTrigger("attack");

            //Wait for animation
            yield return new WaitForSeconds(13 / 30f);

            //Do attack
            DoMeleeAttack();
        }
        else if (attackType == AttackType.Ranged) {
            //Trigger animation
            animator.SetTrigger("attack");

            //Wait for animation
            yield return new WaitForSeconds(13 / 30f);

            //Do attack
            DoRangedAttack(targetPosition);
        }
        else if (attackType == AttackType.Super) {
            StartCoroutine(DoSuperAttack());
        }
    }
    //
    IEnumerator DoSuperAttack() {

        //Get target 
        Vector3 targetPosition = player.transform.position + new Vector3(0f, 2f, 0f);

        //turn on particles
        weaponParticles.SetActive(true);

        //look at player
        agent.updateRotation = false;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }

        //Trigger super attack animation
        animator.SetTrigger("superAttack");

        //Fire first volley - 1 projectile
        //Wait for animation
        yield return new WaitForSeconds(13 / 30f);
        //Spawn projectile
        GameObject projectile1 = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile1.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition, rangedDamage);

        //Get target 
        targetPosition = player.transform.position + new Vector3(0f, 2f, 0f);

        //Fire second volley - 2 projectiles
        //Wait for animation
        yield return new WaitForSeconds(27 / 30f);
        //Spawn projectile
        GameObject projectile2 = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile2.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition + new Vector3(0, 0.5f, 0), rangedDamage);
        GameObject projectile3 = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile3.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition + new Vector3(0, -0.5f, 0), rangedDamage);

        //Get target 
        targetPosition = player.transform.position + new Vector3(0f, 2f, 0f);

        //Fire final volley - 3 projectiles
        //Wait for animation
        yield return new WaitForSeconds(35 / 30f);
        //Spawn projectile
        GameObject projectile4 = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile4.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition + new Vector3(0, 1, 0), rangedDamage);
        GameObject projectile5 = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile5.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition, rangedDamage);
        GameObject projectile6 = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        projectile6.GetComponent<EnemyProjectileMover>().SetAttributes(10f, 8f, targetPosition + new Vector3(0, -1, 0), rangedDamage);


        //turn on particles
        weaponParticles.SetActive(false);

        agent.updateRotation = true;

        //Cooldown regular attacks
        StartCoroutine(DoCooldown(rangedAttackCoolDown));

        //Cooldown super attacks
        yield return new WaitForSeconds(superAttackCoolDown);
        //Set flag
        superAttackReady = true;
    }


}

