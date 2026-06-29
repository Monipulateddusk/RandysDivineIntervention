using UnityEngine;

[CreateAssetMenu(menuName="Particle Database", fileName="Particle Data")]
public class ParticlesCollection_SO : ScriptableObject
{
    [Header("Particle Prefabs")]
    public ParticleSystemController ImplositionParticlePrefab;
    public ParticleSystemController ShockWaveParticlePrefab;
}
