using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Abyssal Fang")]

public class AbyssalFang : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private float castAltHandCooldownTime;

    public float CastAltHandCooldownTime => castAltHandCooldownTime;
}