using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public enum BehaviorType
    {
        Idle,
        Patrol,
        Combat,
    }

    [Header("Idle Enemy Attributes")]
    [SerializeField] float pursuitRange;
    [SerializeField] float shootRange;
    [SerializeField] float shootFrequency;

    [Header("Patrolling Enemy Attributes")]
    [SerializeField] float patrolRange;
    [SerializeField] float meleeRange;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform shotSpawn;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Animator punchAnim;
    
    [SerializeField] private BehaviorType enemyTask; // MEMORY OF ORIGINAL BEHAVIOR
    [SerializeField] private List<Transform> patrolWaypoints;

    #region DRIVEN
    private int waypointGoalIndex;
    private float distanceFromObjective;
    [SerializeField] private float distanceFromPlayer;
    private bool isPatrollingInReverse;
    private bool isAbleToShoot = true;
    private bool isPatrolling = true;
    [SerializeField] private BehaviorType behavior; // BEHAVIOR THAT IS CONTROLLED
    private Vector3 moveTarget;
    private NavMeshAgent navAgent;
    #endregion END DRIVEN

    #region GAME LOOP
    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        behavior = enemyTask;
    }

    private void Update()
    {
        CheckDistanceFromPlayer();
        ProcessBehavior();
    }
    #endregion END GAME LOOP

    private void ProcessBehavior()
    {
        switch (behavior)
        {
            case BehaviorType.Idle:
                PerformIdle();
                break;
            case BehaviorType.Patrol:
                PerformPatrol();
                break;
            case BehaviorType.Combat:
                PerformCombat();
                break;
        }
    }

    private void ChangeBehavior(BehaviorType newBehavior)
    {
        behavior = newBehavior;
    }

    private void PerformIdle()
    {
        Idle();
    }

    private void PerformPatrol()
    {
        Patrol();
    }

    private void PerformCombat()
    {
        Combat();
    }

    private void Idle()
    {
        punchAnim.SetTrigger("Idle");

        // RESET VELOCITY FOR INSTANT STOPPING (FOR TRANSITIONS)
        navAgent.velocity = Vector3.zero;

        if (distanceFromPlayer <= pursuitRange)
        {
            ChangeBehavior(BehaviorType.Combat);
        }
    }

    private void Patrol()
    {
        if (patrolWaypoints.Count > 0)
        {
            isPatrolling = true;
            distanceFromObjective = Vector3.Distance(transform.position, patrolWaypoints[waypointGoalIndex].position);
        }
        else
        {
            isPatrolling = false;
            punchAnim.SetTrigger("Idle");
        }

        if (isPatrolling)
        {
            if (distanceFromObjective < 2f) // IF AT WAYPOINT
            {
                // LOGIC FOR INVERTING PATROL DIRECTION
                if (waypointGoalIndex == patrolWaypoints.Count - 1) // IF AT END OF LIST
                {
                    isPatrollingInReverse = true;
                }
                else if (waypointGoalIndex == 0) // IF AT START OF LIST
                {
                    isPatrollingInReverse = false;
                }

                // PATROL POINT TO POINT IN DIRECTION ENEMY IS MOVING
                if (!isPatrollingInReverse) // IF IS MOVING THROUGH THE LIST IN NORMAL DIRECTION
                {
                    waypointGoalIndex++;
                }
                else // HAS ALREADY WENT THROUGH LIST AND IS NOW GOING IN REVERSE THROUGH LIST
                {
                    waypointGoalIndex--;
                }
            }

            moveTarget = patrolWaypoints[waypointGoalIndex].position;
            navAgent.SetDestination(moveTarget);
        }

        if (distanceFromPlayer <= pursuitRange)
        {
            ChangeBehavior(BehaviorType.Combat);
        }
    }

    private void Combat()
    {
        #region// LOGIC FOR ENEMIES THAT PATROL
        if (enemyTask == BehaviorType.Patrol && distanceFromPlayer < meleeRange) // IF IN MELEE RANGE
        {
            //MoveTowardsPlayer();
            Melee();
        }
        else if (enemyTask == BehaviorType.Patrol && distanceFromPlayer <= pursuitRange) // IF IN COMBAT BUT NOT IN MELEE RANGE
        {
            MoveTowardsPlayer();
        }
        else if (enemyTask == BehaviorType.Patrol && distanceFromPlayer > patrolRange) // IF OUT OF COMBAT RANGE
        {
            ChangeBehavior(BehaviorType.Patrol);
        }
        #endregion

        #region // LOGIC FOR ENEMIES THAT ARE IDLE
        if (enemyTask == BehaviorType.Idle && distanceFromPlayer < shootRange) // IF IN SHOOTING RANGE
        {
            MoveTowardsPlayer();
            Shoot();
        }
        else if (enemyTask == BehaviorType.Idle && distanceFromPlayer <= pursuitRange) // IF IN COMBAT BUT NOT IN SHOOTING RANGE
        {
            MoveTowardsPlayer();
        }
        else if (enemyTask == BehaviorType.Idle && distanceFromPlayer > pursuitRange) // IF OUT OF COMBAT RANGE
        {
            ChangeBehavior(BehaviorType.Idle);
        }
        #endregion
    }

    private void Shoot()
    {
        if (isAbleToShoot)
        {
            StartCoroutine(ShootCooldown());
        }
    }

    private IEnumerator ShootCooldown()
    {
        GameObject projectile = Instantiate(prefab, shotSpawn.position, Quaternion.identity);
        projectile.GetComponent<EnemyProjectileMover>().SetTarget(player.localPosition);
        isAbleToShoot = false;
        yield return new WaitForSeconds(shootFrequency);
        isAbleToShoot = true;
    }

    private void Melee()
    {
        punchAnim.SetTrigger("Melee");
    }

    private void CheckDistanceFromPlayer()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, player.position);
    }

    private void MoveTowardsPlayer()
    {
        navAgent.SetDestination(player.position);
        punchAnim.SetTrigger("Walking");
    }
}