using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Heaven's Lament")]

public class HeavensLament : Spell
{
    public override void Cast(Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
    }
}
