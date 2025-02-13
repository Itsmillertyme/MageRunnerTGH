using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Winter's Wrath")]

public class WintersWrath : Spell
{
    public override void Cast(Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
    }
}