using UnityEngine;
[CreateAssetMenu(menuName = "Summoning Circle Imbutent Data", fileName = "Summoning Circle Data")]
public class SummoningCircleImbutentData : ScriptableObject
{
    public ParticleSystem _particleSystem;

    public Texture2D summoningCircleTexture2D;
    public Color materialColour;
    public Color materialEmissionColour;
    public Color lightColour;

    [Range(0, 1)]   public float materialMetallic = 0;
    [Range(0, 1)]   public float materialSmoothness = 0.5f;
    [Range(0, 1)]   public float materialEmissionIntensity = 0;
    [Range(0, 1)]   public float materialEmissionExposure = 0.1f;

    [Range(0, 360)] public float rotateCircleAngle = 45.0f;
    [Range(0, 5)]   public float growCircleDuration = 1.0f;
    [Range(0, 5)]   public float glowLightDuration = 0.5f;
    [Range(0, 10)]  public float lightGlowIntensity = 5f;

    [Range(0, 5)]   public float circleMoveDuration = 1.0f;
}
