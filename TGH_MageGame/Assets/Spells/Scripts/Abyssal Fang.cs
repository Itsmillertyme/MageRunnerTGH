using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Abyssal Fang")]

public class AbyssalFang : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private float castAltHandCooldownTime = 0.3f;

    public float CastAltHandCooldownTime => castAltHandCooldownTime;
}