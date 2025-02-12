using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Infernal Embrace")]

public class InfernalEmbrace : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private AudioClip continousCastingSFX;

    public override void Cast(Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
    }
}
