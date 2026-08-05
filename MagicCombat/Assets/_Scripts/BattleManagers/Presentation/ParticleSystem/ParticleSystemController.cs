using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    public void PlayParticleSystem()
    {
        if (this.gameObject.TryGetComponent(out ParticleSystem pS))
        {
            pS.Play();
        }
    }

    public void PlayAllParticleSystems()
    {
        if (this.gameObject.TryGetComponent(out ParticleSystem particleSystem))
        {
            particleSystem.Play();
        }

        foreach (Transform child in this.transform)
        {
            if (child.gameObject.TryGetComponent(out particleSystem))
            {
                particleSystem.Play();  
            }
        }
    }

    public void StopAllParticleSystems()
    {
        if (this.gameObject.TryGetComponent(out ParticleSystem particleSystem))
        {
            particleSystem.Stop();
            particleSystem.Clear();
        }

        foreach (Transform child in this.transform)
        {
            if (child.gameObject.TryGetComponent(out particleSystem))
            {
                particleSystem.Stop();
                particleSystem.Clear();
            }
        }
    }
}
