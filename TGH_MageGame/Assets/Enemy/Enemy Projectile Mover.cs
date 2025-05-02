using UnityEngine;

public class EnemyProjectileMover : MonoBehaviour {
    //**PROPERTIES**
    [Header("Attributes")]
    [SerializeField] float moveSpeed;
    [SerializeField] float lifeSpan;
    [SerializeField] Vector3 target;
    [SerializeField] int damage;
    //
    Rigidbody rb;

    //**UNITY METHODS**
    private void Start() {
        //Cache refernces
        rb = GetComponent<Rigidbody>();

        //Set destruction after lifespan
        if (lifeSpan > 0) {
            Destroy(gameObject, lifeSpan);
        }
    }
    //
    private void Update() {
        //Move towards target
        rb.linearVelocity = transform.forward * moveSpeed;
    }
    //
    private void OnTriggerEnter(Collider other) {
        //Check if hit player
        if (other.CompareTag("Player")) {
            //Deal damage to player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null) {
                playerHealth.RemoveFromHealth(damage);
            }
        }

        if (!other.CompareTag("Enemy Projectile")) {
            //Destroy projectile
            Destroy(gameObject);
        }
    }

    //**UTILITY METHODS**
    public void SetAttributes(float moveSpeedIn, float lifeSpanIn, Vector3 targetIn, int damageIn) {

        //set properties
        moveSpeed = moveSpeedIn;
        lifeSpan = lifeSpanIn;
        target = targetIn;
        damage = damageIn;

        //Look towards target
        transform.LookAt(target);

        //Set destruction after lifespan
        if (lifeSpan > 0) {
            Destroy(gameObject, lifeSpan);
        }
    }
    //DEPRECATED CODE - KEPT FOR COMPATIBILITY
    public void SetTarget(Vector3 direction) {
        Debug.Log("DEPRECATED CODE - Please use 'SetAttributes()' to instantiate a projectile");
        transform.LookAt(direction);
    }

}