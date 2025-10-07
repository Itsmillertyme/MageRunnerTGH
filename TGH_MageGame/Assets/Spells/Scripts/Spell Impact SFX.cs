using UnityEngine;

public class SpellImpactSFX : MonoBehaviour
{
    private AudioSource soundEffect;

    private void Awake()
    {
        soundEffect = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!soundEffect.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }
    public void BeginEffect(Spell spell)
    {
        soundEffect.clip = spell.HitSFX;
        soundEffect.volume = spell.HitSFXVolume;
        soundEffect.pitch = spell.HitSFXPitch + UtilityTools.RandomVarianceFloat();
        soundEffect.Play();
    }
}