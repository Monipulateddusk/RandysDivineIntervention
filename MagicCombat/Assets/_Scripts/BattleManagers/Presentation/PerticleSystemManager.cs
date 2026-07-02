namespace TurnBased.Presentation
{
    public class PerticleSystemManager
    {
        private StatusPresentationManager _StatusPresentationManager;
        private ParticlesCollection_SO _ParticlesData;



        public void Awake(ParticlesCollection_SO data)
        {
            this._ParticlesData = data;

            this._StatusPresentationManager = new();
            this._StatusPresentationManager.Awake(data);
        }



        public void OnDestroy()
        {
            this._StatusPresentationManager.OnDestroy();
            this._StatusPresentationManager = null;
        }




    }
}