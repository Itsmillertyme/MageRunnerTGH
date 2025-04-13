using UnityEngine;

// WIP ON A REWRITE USING COROUTINES INSTEAD OF UPDATE

public class AbyssalFangProjectileMovement : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private float moveSpeed;
    [SerializeField] PlayerStats playerStats;
    private Vector3 targetPosition;

    // 
    private Vector3 moveDirection;

    private void Start()
    {
        moveDirection = (targetPosition - transform.position).normalized;
    }

    private void Update()
    {
        Move();
        //Snap z to 0
        //transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    private void Move()
    {
        // MOVE TOWARD TARGET
        transform.position += moveSpeed * Time.deltaTime * moveDirection;

        // ROTATE OBJECT TOWARDS DIRECTION TO MOVE
        if (moveDirection != Vector3.zero)  // Ensure there is a valid direction
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // ADD 90 ON Y-AXIS TO FIX PROJECTILE ROTATION
            transform.rotation = targetRotation * Quaternion.Euler(0, 90, 0);
        }
    }

    public void SetAttributes(float moveSpeed, Vector3 size, Vector3 targetPosition)
    {
        this.moveSpeed = moveSpeed;
        transform.localScale = size;
        this.targetPosition = targetPosition;
    }
}
