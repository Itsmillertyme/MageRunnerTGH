using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    //[Header("Debugging")]
    //[SerializeField] private float lifeSpan;
    //[SerializeField] private float moveSpeed;
    //[SerializeField] private int damage;
    //[SerializeField] PlayerStats playerStats;
    //private Vector3 targetPosition;

    //// 
    //private Vector3 moveDirection;

    private void Start()
    {
        Debug.LogError("ProjectileMover Script is deprecated from our project.");
        //    moveDirection = (targetPosition - transform.position).normalized;
        //    Destroy(gameObject, lifeSpan);
    }

    //private void Update()
    //{
    //    Move();
    //    //Snap z to 0
    //    //transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    //}

    //private void Move()
    //{
    //    // MOVE TOWARD TARGET
    //    transform.position += moveSpeed * Time.deltaTime * moveDirection;

    //    // ROTATE OBJECT TOWARDS DIRECTION TO MOVE
    //    if (moveDirection != Vector3.zero)  // Ensure there is a valid direction
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

    //        // ADD 90 ON Y-AXIS TO FIX PROJECTILE ROTATION
    //        transform.rotation = targetRotation * Quaternion.Euler(0, 90, 0);
    //    }
    //}

    //public void SetAttributes(int damage, float lifeSpan, float moveSpeed, Vector3 size, Vector3 targetPosition)
    //{
    //    this.damage = damage;
    //    this.lifeSpan = lifeSpan;
    //    this.moveSpeed = moveSpeed;
    //    transform.localScale = size;
    //    this.targetPosition = targetPosition;
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        collision.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);
    //        Destroy(gameObject);
    //    }

    //    else if (collision.gameObject.CompareTag("Player"))
    //    {
    //        collision.gameObject.GetComponent<PlayerHealth>().RemoveFromHealth(damage);
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    //private void OnTriggerEnter(Collider collided)
    //{

    //    if (collided.gameObject.CompareTag("Enemy"))
    //    {
    //        collided.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);

    //        Destroy(gameObject, lifeSpan);
    //    }

    //}
}