using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Spells/Abyssal Fang")]

public class AbyssalFang : Spell {
    [Header("Unique Spell Attributes")]
    [SerializeField] private float castAltHandCooldownTime;

    [SerializeField] UnityEvent spellCasted;

    public override void Cast(Vector3 position, Vector3 direction) {

        Debug.Log("Calling cast");
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
        spellCasted.Invoke();
        CastAltHand(position, direction);
    }

    IEnumerator CastAltHand(Vector3 position, Vector3 direction) {
        Debug.Log("Calling alt cast");
        yield return new WaitForSeconds(castAltHandCooldownTime);

        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
    }
}