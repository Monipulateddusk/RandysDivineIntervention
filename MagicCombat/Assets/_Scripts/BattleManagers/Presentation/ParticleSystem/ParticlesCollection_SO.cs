using UnityEngine;

[CreateAssetMenu(menuName="Particle Database", fileName="Particle Data")]
public class ParticlesCollection_SO : ScriptableObject
{
    [Header("Particle Prefabs")]
    public ParticleSystemController BurnParticlePrefab;
    public ParticleSystemController PoisonParticlePrefab;
    public ParticleSystemController ShockWaveParticlePrefab;
}
