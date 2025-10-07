using UnityEngine;

public class EnemyDamager : MonoBehaviour
{
    [SerializeField] private GameObject impactSFXPrefab;
    private int damage;
    private float lifeSpan;
    private bool destroyOnEnemyImpact;
    private bool destroyOnPlatformImpact;
    private bool addDamageOverTime;
    private Spell spell;

    private void Start()
    {
        Destroy(gameObject, lifeSpan);
    }

    private void OnTriggerEnter(Collider collided)
    {
        // ADD SOUND EFFECT ON AN COLLISION
        AddSFXObject();

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

    public void SetAttributes(Spell newSpell)
    {
        spell = newSpell;
        damage = spell.Damage;
        lifeSpan = spell.LifeSpan;
        destroyOnEnemyImpact = spell.DestroyOnEnemyImpact;
        destroyOnPlatformImpact = spell.DestroyOnEnvironmentImpact;
        addDamageOverTime = spell.DamageOverTime;
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

    private void AddSFXObject()
    {
        GameObject projectile = Instantiate(spell.HitSFXPrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<SpellImpactSFX>().BeginEffect(spell);
    }

    public void SetPrefab(GameObject runtimeObject)
    {
        impactSFXPrefab = runtimeObject;
    }
}