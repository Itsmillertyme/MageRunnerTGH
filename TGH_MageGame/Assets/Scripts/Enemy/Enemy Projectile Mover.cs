using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileMover : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float lifeSpan;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeSpan);
    }

    private void Update()
    {
        rb.linearVelocity = transform.forward * moveSpeed;
    }

    public void SetTarget(Vector3 direction)
    {
        transform.LookAt(direction);
    }
}