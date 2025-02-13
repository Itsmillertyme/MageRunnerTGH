using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Abyssal Fang")]

public class AbyssalFang : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private float castAltHandCooldownTime;

    public override void Cast(Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
        CastAltHand(position, direction);
    }

    IEnumerator CastAltHand(Vector3 position, Vector3 direction)
    {
        yield return new WaitForSeconds(castAltHandCooldownTime);

        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
    }
}