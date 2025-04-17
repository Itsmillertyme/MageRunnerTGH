using UnityEngine;

public class EnemyDamager : MonoBehaviour {
    private int damage;
    private float lifeSpan;

    private void Start() {
        Destroy(gameObject, lifeSpan);
    }

    public void SetAttributes(int newDamage, float newLifeSpan) {
        damage = newDamage;
        lifeSpan = newLifeSpan;
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Mob Enemy") || collision.gameObject.CompareTag("Boss Enemy")) {
            collision.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);
            Destroy(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collided) {

        if (collided.gameObject.CompareTag("Mob Enemy") || collided.gameObject.CompareTag("Boss Enemy")) {
            collided.gameObject.GetComponent<EnemyHealth>().RemoveFromHealth(damage);
            Destroy(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}