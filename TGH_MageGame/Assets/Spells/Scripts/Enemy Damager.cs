using UnityEngine;

public class EnemyDamager : MonoBehaviour
{
    private int damage;
    private float lifeSpan;
    private bool destroyOnImpact;

    private void Start()
    {
        Destroy(gameObject, lifeSpan);
    }

    public void SetAttributes(int newDamage, float newLifeSpan, bool newDestroyOnImpact)
    {
        damage = newDamage;
        lifeSpan = newLifeSpan;
        destroyOnImpact = newDestroyOnImpact;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Mob Enemy") || collision.gameObject.CompareTag("Boss Enemy"))
        {
            collision.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);

            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider collided)
    {

        if (collided.gameObject.CompareTag("Mob Enemy") || collided.gameObject.CompareTag("Boss Enemy"))
        {
            collided.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);

            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
        }
    }
}