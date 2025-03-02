using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Heaven's Lament")]

public class HeavensLament : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private GameObject vfxPrefab;
    public override void Cast(Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(Damage, LifeSpan, MoveSpeed, ProjectileSize, direction);
        //GameObject newVFX = Instantiate(vfxPrefab, position, Quaternion.identity);
        //float yRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        //Quaternion orientation = Quaternion.Euler(0, yRotation, 0);
        //GameObject newVFX2 = Instantiate(vfxPrefab, position, orientation);
    }
}
