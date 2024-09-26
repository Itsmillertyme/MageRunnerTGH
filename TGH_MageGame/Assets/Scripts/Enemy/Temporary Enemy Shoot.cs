using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryEnemyShoot : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float directionChangeInterval = 1f;
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] GameObject prefab;
    private float timeSinceLastChange = 0f;

    private Vector3 moveDirection;
    private Vector3 targetPosition;
    private Vector3 moveTarget;
    private Coroutine shootDelay;

    private void Start()
    {
        // Initialize the first movement direction
        ChangeDirection();
        if (shootDelay == null)
        {
            shootDelay = StartCoroutine(Shoot());
        }
    }

    private void Update()
    {
        timeSinceLastChange += Time.deltaTime;

        // Change direction at set intervals
        if (timeSinceLastChange >= directionChangeInterval)
        {
            ChangeDirection();
            timeSinceLastChange = 0f;
        }

        // Smoothly move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

   
    }

    private void ChangeDirection()
    {
        // Randomly choose a direction (forward, backward)
        int direction = Random.Range(1, 3);

        switch (direction)
        {
            case 1:
                moveDirection = Vector3.left;
                break;
            case 2:
                moveDirection = Vector3.right;
                break;
        }

        // Calculate the new target position based on the movement direction
        moveTarget = transform.position + (moveDirection * moveDistance);
    }

    public IEnumerator Shoot()
    {
        while (true)
        {
            targetPosition = GameObject.FindWithTag("Player").transform.position;
            GameObject projectile = Instantiate(prefab, transform.position, transform.rotation);
            projectile.GetComponent<ProjectileMover>().SetAttributes(5, 2, 10, targetPosition);
            int waitTime = Random.Range(1, 7);

            yield return new WaitForSeconds(waitTime);
        }
    }
}