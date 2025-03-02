using UnityEngine;

public abstract class SpellSkillUpgrade : ScriptableObject
{
    public abstract void Apply(Spell spell);
}