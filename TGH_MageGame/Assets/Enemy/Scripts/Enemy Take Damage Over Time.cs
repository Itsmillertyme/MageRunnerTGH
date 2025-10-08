using System.Collections;
using UnityEngine;

public class EnemyTakeDamageOverTime : MonoBehaviour
{
    [Header("Health Regen")]
    [SerializeField] private int damageTickAmount = 10;
    [SerializeField] private float damageFrequency = 1;
    [SerializeField] private float damageDuration = 5;

    private EnemyHealth enemyHealth;
    private Coroutine damageOverTime;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        if (damageOverTime == null)
        {
            damageOverTime = StartCoroutine(DamageOverTime());
        }
    }

    public IEnumerator DamageOverTime()
    {
        float elapsedTime = 0f;

        while (elapsedTime < damageDuration)
        {
            enemyHealth.RemoveFromHealth(damageTickAmount);
            elapsedTime += damageFrequency;
            yield return new WaitForSeconds(damageFrequency);
        }

        Destroy(this);
    }
}