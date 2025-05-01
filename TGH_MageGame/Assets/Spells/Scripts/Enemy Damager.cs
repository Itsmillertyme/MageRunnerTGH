using UnityEngine;

public class EnemyDamager : MonoBehaviour {
    private int damage;
    private float lifeSpan;
    private bool destroyOnEnemyImpact;
    private bool destroyOnPlatformImpact;

    private void Start() {
        Destroy(gameObject, lifeSpan);
    }

    public void SetAttributes(int newDamage, float newLifeSpan, bool newDestroyOnEnemyImpact = true, bool newDestroyOnPlatformImpact = true) {

        //STAT INTEGRATION
        int bonusStatDamage = (int) GameObject.Find("Player").GetComponent<PlayerController>().PlayerStats["Damage"].StatValue;

        damage = newDamage + bonusStatDamage;
        lifeSpan = newLifeSpan;
        destroyOnEnemyImpact = newDestroyOnEnemyImpact;
        destroyOnPlatformImpact = newDestroyOnPlatformImpact;
    }

    private void OnCollisionEnter(Collision collision) {

        if (collision.gameObject.CompareTag("Mob Enemy")) {
            collision.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);

            if (destroyOnEnemyImpact) {
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Boss Enemy")) {
            collision.gameObject.GetComponent<BossHealth>().RemoveFromHealth(damage);

            if (destroyOnEnemyImpact) {
                Destroy(gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Platform") && !destroyOnPlatformImpact) {
            // Do nothing, projectile will not be destroyed on platform impact
        }
        else {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collided) {

        if (collided.gameObject.CompareTag("Mob Enemy")) {
            collided.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);

            if (destroyOnEnemyImpact) {
                Destroy(gameObject);
            }
        }
        else if (collided.gameObject.CompareTag("Boss Enemy")) {
            collided.gameObject.GetComponent<BossHealth>().RemoveFromHealth(damage);

            if (destroyOnEnemyImpact) {
                Destroy(gameObject);
            }
        }
        else if (collided.gameObject.CompareTag("Platform") && !destroyOnPlatformImpact) {
            // Do nothing, projectile will not be destroyed on platform impact
        }
        else {
            Destroy(gameObject);
        }
    }
}