using TurnBased.Presentation;
using UnityEngine;

public class SummoningCircleVisualHandler : MonoBehaviour
{
    [Header("Inspector Variables")]
    [SerializeField] private Material SummoningCircleMaterialPrefab;

    [SerializeField] private Transform SummoningCircleTransform;
    [SerializeField] private MeshRenderer summoningCirclePlaneMeshRenderer;
    [SerializeField] private Light glowLight;

    [SerializeField] private SummoningCircleImbutentData summoningCircleImbutentData;
    [SerializeField] private ParticleSystem instanciatedParticleSystem;

    [SerializeField] private bool testVisual;

    private void OnValidate()
    {
        if (this.testVisual)
        {
            this.testVisual = false;

            InstanciateParticleSystem();

            InitaliseSummoningCircleVisual(summoningCircleImbutentData);
            _ = PlaySummoningCircleAnimation();
        }       
    }


    public async System.Threading.Tasks.Task RunSummoningCircleVisual(SummoningCircleImbutentData data)
    {
        this.summoningCircleImbutentData = data;
        DestroyParticleSystem();
        InstanciateParticleSystem();

        InitaliseSummoningCircleVisual(data);
        await PlaySummoningCircleAnimation();
    }

    public void InitaliseSummoningCircleVisual(SummoningCircleImbutentData data)
    {
        this.summoningCircleImbutentData = data;

        StartParticleSystemPlaying();

        if (this.summoningCirclePlaneMeshRenderer == null || this.glowLight == null || this.summoningCircleImbutentData == null) { return; }
  
        this.summoningCirclePlaneMeshRenderer.material = SummoningCircleMaterialPrefab;
        this.glowLight.intensity = this.summoningCircleImbutentData.lightGlowIntensity;
        this.glowLight.color = this.summoningCircleImbutentData.lightColour;
        this.SummoningCircleTransform.transform.localRotation = Quaternion.Euler(0, this.summoningCircleImbutentData.rotateCircleAngle, 0);
        this.SummoningCircleTransform.transform.localScale = new Vector3(1, 1, 1);

        var summoningCircleMaterialInstance = summoningCirclePlaneMeshRenderer.material;

        summoningCircleMaterialInstance.SetTexture("_SummoningCircleTexture2D", this.summoningCircleImbutentData.summoningCircleTexture2D);
        summoningCircleMaterialInstance.SetColor("_Color", this.summoningCircleImbutentData.materialColour);

        summoningCircleMaterialInstance.SetFloat("_Metallic", this.summoningCircleImbutentData.materialMetallic);
        summoningCircleMaterialInstance.SetFloat("_Smoothness", this.summoningCircleImbutentData.materialSmoothness);

        summoningCircleMaterialInstance.SetFloat("_Exposure", this.summoningCircleImbutentData.materialEmissionExposure);
        summoningCircleMaterialInstance.SetFloat("_EmissionIntensity", this.summoningCircleImbutentData.materialEmissionIntensity);
  
    }

    private void InstanciateParticleSystem()
    {
        if (this.instanciatedParticleSystem != null && this.summoningCircleImbutentData == null) { return; }

        GameObject gO;
        Transform childTransform = this.transform.Find("ParticleSystem(Clone)");
        if (childTransform == null) 
        {
            gO = GameObject.Instantiate(this.summoningCircleImbutentData._particleSystem.gameObject, this.transform);
            gO.name = "ParticleSystem(Clone)";
        }
        else
        {
            gO = childTransform.gameObject;
        }

        if (gO != null && gO.TryGetComponent(out ParticleSystem pS))
        {
            this.instanciatedParticleSystem = pS;

            StopParticleSystemPlaying();
        }
    }

    private void DestroyParticleSystem()
    {
        if (this.instanciatedParticleSystem == null) { return; }
        
        Destroy(this.instanciatedParticleSystem.gameObject);
        this.instanciatedParticleSystem = null;
        
    }

    public void ChangeMaterialColour(UnityEngine.Color newColor, UnityEngine.Color newAccentColor)
    {
        var summoningCircleMaterialInstance = this.summoningCirclePlaneMeshRenderer.material;
        summoningCircleMaterialInstance.SetColor("_Color", newColor);
        summoningCircleMaterialInstance.SetColor("_ColorEmission", newAccentColor);
    }

    private async System.Threading.Tasks.Task PlaySummoningCircleAnimation()
    {
        if (this.instanciatedParticleSystem == null || this.summoningCircleImbutentData == null) { return; }

        StopParticleSystemPlaying();

        await GrowAndRotateSummoningCircle();
        await GlowSummoningCircle();
    }

    /// <summary>
    /// Set the circle to a size of 0 and slowly rotate and grow the circle over time. 
    /// </summary>
    /// <returns></returns>
    private async System.Threading.Tasks.Task GrowAndRotateSummoningCircle()
    {
        if (this.SummoningCircleTransform == null || this.summoningCircleImbutentData == null || this.summoningCirclePlaneMeshRenderer == null || this.glowLight == null) { return; }

        this.glowLight.intensity = 0;
        this.SummoningCircleTransform.transform.localRotation = Quaternion.identity;
        this.SummoningCircleTransform.transform.localScale = Vector3.zero;

        var summoningCircleMaterialInstance = this.summoningCirclePlaneMeshRenderer.material;
        summoningCircleMaterialInstance.SetFloat("_Exposure", 0);
        summoningCircleMaterialInstance.SetFloat("_EmissionIntensity", 0);


        float startTime = Time.time;
        while (Time.time < startTime + this.summoningCircleImbutentData.growCircleDuration)
        {
            float t = (Time.time - startTime) / this.summoningCircleImbutentData.growCircleDuration;

            this.SummoningCircleTransform.transform.localRotation = Quaternion.Euler(0, Mathf.SmoothStep(0, this.summoningCircleImbutentData.rotateCircleAngle, t), 0);
            this.SummoningCircleTransform.transform.localScale = new Vector3(Mathf.SmoothStep(0, 1, t), Mathf.SmoothStep(0, 1, t), Mathf.SmoothStep(0, 1, t));

            await System.Threading.Tasks.Task.Yield();
        }
        this.SummoningCircleTransform.transform.localRotation = Quaternion.Euler(0, this.summoningCircleImbutentData.rotateCircleAngle, 0);
        this.SummoningCircleTransform.transform.localScale = new Vector3(1,1,1);

    }

    /// <summary>
    /// Over time increase the emmision of the shader graph to make the circle glow with the light. Also, once we get here, enable the particle system.
    /// </summary>
    /// <returns></returns>
    private async System.Threading.Tasks.Task GlowSummoningCircle()
    {
        if (this.summoningCirclePlaneMeshRenderer == null || this.glowLight == null || this.summoningCircleImbutentData == null) { return; }

        var summoningCircleMaterialInstance = this.summoningCirclePlaneMeshRenderer.material;
        summoningCircleMaterialInstance.SetColor("_ColorEmission", this.summoningCircleImbutentData.materialEmissionColour);

        StartParticleSystemPlaying();


        float startTime = Time.time;
        while (Time.time < startTime + this.summoningCircleImbutentData.glowLightDuration)
        {
            float t = (Time.time - startTime) / this.summoningCircleImbutentData.glowLightDuration;

            summoningCircleMaterialInstance.SetFloat("_Exposure", Mathf.SmoothStep(0, this.summoningCircleImbutentData.materialEmissionExposure, t));
            summoningCircleMaterialInstance.SetFloat("_EmissionIntensity", Mathf.SmoothStep(0, this.summoningCircleImbutentData.materialEmissionIntensity, t));

            this.glowLight.intensity = Mathf.SmoothStep(0, this.summoningCircleImbutentData.lightGlowIntensity, t);
            this.glowLight.color = new Color(Mathf.SmoothStep(0, this.summoningCircleImbutentData.lightColour.r, t), Mathf.SmoothStep(0, this.summoningCircleImbutentData.lightColour.g, t), Mathf.SmoothStep(0, this.summoningCircleImbutentData.lightColour.b, t));

            await System.Threading.Tasks.Task.Yield();
        }

        summoningCircleMaterialInstance.SetFloat("_Exposure", this.summoningCircleImbutentData.materialEmissionExposure);
        summoningCircleMaterialInstance.SetFloat("_EmissionIntensity", this.summoningCircleImbutentData.materialEmissionIntensity);
        this.glowLight.intensity = this.summoningCircleImbutentData.lightGlowIntensity;
        this.glowLight.color = this.summoningCircleImbutentData.lightColour;
    }

    public async System.Threading.Tasks.Task MoveSummoningCircleToPosition(Vector3 endPosition)
    {
        if (this.summoningCircleImbutentData == null) return;  

        Vector3 startPosition = this.gameObject.transform.position;

        float startTime = Time.time;
        while (Time.time < startTime + this.summoningCircleImbutentData.circleMoveDuration)
        {
            float t = (Time.time - startTime) / this.summoningCircleImbutentData.circleMoveDuration;

            Vector3 position = new(Mathf.SmoothStep(startPosition.x, endPosition.x, t), Mathf.SmoothStep(startPosition.y, endPosition.y, t), Mathf.SmoothStep(startPosition.z, endPosition.z, t));

            this.gameObject.transform.SetPositionAndRotation(position, this.transform.rotation);
            await System.Threading.Tasks.Task.Yield();
        }
        this.gameObject.transform.SetPositionAndRotation(endPosition, this.transform.rotation);
    }

    public Color GetPrimaryColour() => this.summoningCircleImbutentData.materialColour;
    public Color GetSecondaryColour() => this.summoningCircleImbutentData.materialEmissionColour;


    public bool TryGetTotalDuration(out float totalDuration)
    {
        totalDuration = 0;
        if (this.summoningCircleImbutentData == null) {  return false; }

        totalDuration += this.summoningCircleImbutentData.growCircleDuration;
        totalDuration += this.summoningCircleImbutentData.glowLightDuration;

        return true;
    }

    private void StopParticleSystemPlaying()
    {
        if (this.instanciatedParticleSystem != null && this.instanciatedParticleSystem.TryGetComponent(out ParticleSystemController particleSystemManager))
        {
            particleSystemManager.StopAllParticleSystems();
        }
    }
    private void StartParticleSystemPlaying()
    {
        if (this.instanciatedParticleSystem != null && this.instanciatedParticleSystem.TryGetComponent(out ParticleSystemController particleSystemManager))
        {
            particleSystemManager.PlayAllParticleSystems();
        }
    }
}
