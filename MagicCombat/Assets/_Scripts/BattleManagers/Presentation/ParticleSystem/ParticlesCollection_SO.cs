using UnityEngine;

[CreateAssetMenu(menuName="Particle Database", fileName="Particle Data")]
public class ParticlesCollection_SO : ScriptableObject
{
    [Header("Particle Prefabs")]
    public ParticleSystemController ApplyBurnParticleSystem;
    public ParticleSystemController BurnParticlePrefab;
    public ParticleSystemController PoisonParticlePrefab;
    public ParticleSystemController ShockWaveParticlePrefab;
    public ParticleSystemController CollisionParticlePrefab;
}
