using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Abyssal Fang")]

public class AbyssalFang : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private float castAltHandCastDelayTime = 0.3f;

    public float CastAltHandCooldownTime => castAltHandCastDelayTime;

    public void SetCastAltHandCastDelayTime(float newValue) => castAltHandCastDelayTime = newValue;
}