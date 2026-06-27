using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    [Header("Particle System Variables")]
    [SerializeField] private Color PrimaryColour, SecondaryColour;


    [Header("Particle System Referances")]
    [SerializeField] private ParticleSystem PrimaryColourParticleSystem, SecondaryColourParticleSystem;

    public bool test;

    private void OnValidate()
    {
        if (this.PrimaryColourParticleSystem == null || this.SecondaryColourParticleSystem == null) { return; }

        var primarySystem = this.PrimaryColourParticleSystem.main;
        primarySystem.startColor = this.PrimaryColour;

        var secondarySystem = this.SecondaryColourParticleSystem.main;
        secondarySystem.startColor = this.SecondaryColour;

        if (this.test)
        {
            this.test = false;

            this.PrimaryColourParticleSystem.Play();

        }

    }
}
