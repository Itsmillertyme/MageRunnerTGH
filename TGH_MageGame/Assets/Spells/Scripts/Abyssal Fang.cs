using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Spells/Abyssal Fang")]

public class AbyssalFang : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private float castAltHandCooldownTime;

    public float CastAltHandCooldownTime => castAltHandCooldownTime;

    public override void Cast(Vector3 position, Vector3 direction)
    { 
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
    }
}