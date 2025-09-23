using UnityEngine;

// WIP ON A REWRITE USING COROUTINES INSTEAD OF UPDATE

public class AbyssalFangProjectileMovement : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private float moveSpeed;
    private Vector3 targetPosition;

    private GameManager gameManager;
    private Vector3 moveDirection;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        moveDirection = (targetPosition - transform.position).normalized;
    }

    private void Update()
    {
        Move();
        //Snap z to 0
        transform.position = new Vector3(transform.position.x, transform.position.y, gameManager.CrosshairPositionIn3DSpace.z);
    }

    private void Move()
    {
        // MOVE TOWARD TARGET
        transform.position += moveSpeed * Time.deltaTime * moveDirection;

        // ROTATE OBJECT TOWARDS DIRECTION TO MOVE
        if (moveDirection != Vector3.zero)  // Ensure there is a valid direction
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // FIX PROJECTILE ROTATION
            transform.rotation = targetRotation * Quaternion.Euler(90, 0, 0);
        }
    }

    public void SetAttributes(float moveSpeed, Vector3 size, Vector3 targetPosition)
    {
        this.moveSpeed = moveSpeed;
        transform.localScale = size;
        this.targetPosition = targetPosition;
    }
}
