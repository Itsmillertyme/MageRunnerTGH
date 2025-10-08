using System.Collections;
using UnityEngine;

public class EnemyMelee : MonoBehaviour, IBehave {

    //**PROPERTIES**    
    [SerializeField] float attackCoolDown;
    [SerializeField] float attackRadius;
    [SerializeField] int damage;

    Animator animator;
    GameObject player;

    [SerializeField] bool initialized = false;
    [SerializeField] bool attackReady = true;
    [SerializeField] bool playerInRange = false;

    //**UNITY METHODS**
    private void Awake() {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    private void Update() {

        //Check if initialized
        if (initialized) {
            //Look for player
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

            Debug.Log(distanceToPlayer);
            if (distanceToPlayer < attackRadius) {
                //Set flag
                playerInRange = true;

                //Is attack ready
                if (attackReady) {
                    attackReady = false;
                    StartCoroutine(DoMelee());
                }
            }
            else {
                playerInRange = false;
            }
        }
    }


    //**UTILITY METHODS**

    //**COROUTINES**
    IEnumerator DoMelee() {
        //Trigger animation
        animator.SetTrigger("attack");

        //Wait for animation
        yield return new WaitForSeconds(11 / 30f);

        //test if still in range
        if (playerInRange) {
            //apply damage
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

            //Remove health
            if (playerHealth != null) {
                playerHealth.RemoveFromHealth(damage);
            }
        }

        //Wait for cooldown
        yield return new WaitForSeconds(attackCoolDown);

        //Set flag
        attackReady = true;
    }

    public void Initialize(RoomData roomDataIn, bool spawningDebugMode = false, bool aiDebugMode = false) {
        throw new System.NotImplementedException();
    }
}
