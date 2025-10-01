using UnityEngine;

public class ThunderlordsCascadeProjectileMovement : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparksFX;
    [SerializeField] private ParticleSystem dustCloudFX;
    [SerializeField] private ParticleSystem dustDebrisFX;

    public ParticleSystem SparksFX => sparksFX;
    public ParticleSystem DustCloudFX => dustCloudFX;
    public ParticleSystem DustDebrisFX => dustDebrisFX;
}