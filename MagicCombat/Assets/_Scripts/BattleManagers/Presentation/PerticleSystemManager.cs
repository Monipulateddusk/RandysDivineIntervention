namespace TurnBased.Presentation
{
    public class PerticleSystemManager
    {
        private ParticlesCollection_SO particlesData;
        public void Awake(ParticlesCollection_SO data)
        {
            this.particlesData = data;
        }

        public void OnDestroy()
        {

        }


    }
}