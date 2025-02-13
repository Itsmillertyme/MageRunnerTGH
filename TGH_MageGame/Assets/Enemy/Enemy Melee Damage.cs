using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMeleeDamage : MonoBehaviour
{
    [SerializeField] private int meleeDamage; 
    private void OnTriggerEnter(Collider collider)
    {
        collider.gameObject.GetComponent<PlayerHealth>().RemoveFromHealth(meleeDamage);
    }
}
