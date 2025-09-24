using Unity.VisualScripting;
using UnityEngine;

public class EnemyDamager : MonoBehaviour
{
    private int damage;
    private float lifeSpan;
    private bool destroyOnEnemyImpact;
    private bool destroyOnPlatformImpact;
    private int bonusStatDamage;
    private bool addDamageOverTime;

    private void Start()
    {
        Destroy(gameObject, lifeSpan);
    }

    private void OnTriggerEnter(Collider collided)
    {
        // ADD SOUND EFFECT ON AN COLLISION
        AddSFXObject(collided);

        if (collided.gameObject.CompareTag("Mob Enemy"))
        {
            collided.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);

            if (addDamageOverTime && NoDamageOverTimeAlreadyExists(collided.gameObject))
            {
                collided.gameObject.AddComponent<EnemyTakeDamageOverTime>();
            }

            if (destroyOnEnemyImpact)
            {
                Destroy(gameObject);
            }
        }
        else if (collided.gameObject.CompareTag("Boss Enemy"))
        {
            collided.gameObject.GetComponent<BossHealth>().RemoveFromHealth(damage);

            if (addDamageOverTime && NoDamageOverTimeAlreadyExists(collided.gameObject))
            {
                collided.gameObject.AddComponent<EnemyTakeDamageOverTime>();
            }

            if (destroyOnEnemyImpact)
            {
                Destroy(gameObject);
            }
        }
        else if (collided.gameObject.CompareTag("Platform") && !destroyOnPlatformImpact)
        {
            // Do nothing, projectile will not be destroyed on platform impact
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAttributes(int newDamage, float newLifeSpan, bool newDestroyOnEnemyImpact, bool newDestroyOnPlatformImpact, bool newDamageOverTime)
    {
        damage = newDamage + bonusStatDamage;
        lifeSpan = newLifeSpan;
        destroyOnEnemyImpact = newDestroyOnEnemyImpact;
        destroyOnPlatformImpact = newDestroyOnPlatformImpact;
        addDamageOverTime = newDamageOverTime;
    }

    private bool NoDamageOverTimeAlreadyExists(GameObject enemy)
    {
        if (enemy.GetComponent<EnemyTakeDamageOverTime>() == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void AddSFXObject(Collider collided)
    {
        AudioSource audioSource = collided.AddComponent<AudioSource>();
        //audioSource.
    }
}