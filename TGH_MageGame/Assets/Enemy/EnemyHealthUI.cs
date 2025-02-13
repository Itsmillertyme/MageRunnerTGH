using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] Scrollbar healthUI;
    EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        healthUI.size = (float)health.CurrentHealth / health.MaxHealth;
    }
}
