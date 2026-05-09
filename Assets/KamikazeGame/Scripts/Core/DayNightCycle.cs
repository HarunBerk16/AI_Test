using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun")]
    public Light sunLight;
    [Range(0f, 24f)] public float timeOfDay = 10f;
    public float dayDurationSeconds = 120f;
    public bool autoAdvance = true;

    [Header("Sun Colors")]
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Ambient")]
    public Gradient ambientSkyColor;
    public Gradient ambientEquatorColor;
    public Gradient ambientGroundColor;

    [Header("Fog")]
    public Gradient fogColor;
    public AnimationCurve fogStart;
    public AnimationCurve fogEnd;

    void Awake()
    {
        if (sunLight == null)
            sunLight = GetComponent<Light>();

        SetupDefaultCurves();
    }

    void Update()
    {
        if (autoAdvance)
            timeOfDay = (timeOfDay + Time.deltaTime * (24f / dayDurationSeconds)) % 24f;

        ApplyTimeOfDay();
    }

    void ApplyTimeOfDay()
    {
        float t = timeOfDay / 24f;

        // Sun rotation: 0h = -90 (midnight below horizon), 12h = straight up, etc.
        float sunAngle = (timeOfDay / 24f) * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        sunLight.color = sunColor.Evaluate(t);
        sunLight.intensity = sunIntensity.Evaluate(t);

        RenderSettings.ambientSkyColor     = ambientSkyColor.Evaluate(t);
        RenderSettings.ambientEquatorColor = ambientEquatorColor.Evaluate(t);
        RenderSettings.ambientGroundColor  = ambientGroundColor.Evaluate(t);

        RenderSettings.fogColor         = fogColor.Evaluate(t);
        RenderSettings.fogStartDistance = fogStart.Evaluate(t);
        RenderSettings.fogEndDistance   = fogEnd.Evaluate(t);

        DynamicGI.UpdateEnvironment();
    }

    void SetupDefaultCurves()
    {
        // Sun color: dawn orange → day white → dusk red → night dark
        sunColor = new Gradient();
        sunColor.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f,0.05f,0.1f),  0.00f), // midnight
                new GradientColorKey(new Color(1.00f,0.50f,0.20f), 0.25f), // dawn
                new GradientColorKey(new Color(1.00f,0.96f,0.88f), 0.45f), // morning
                new GradientColorKey(new Color(1.00f,0.98f,0.95f), 0.50f), // noon
                new GradientColorKey(new Color(1.00f,0.96f,0.88f), 0.55f), // afternoon
                new GradientColorKey(new Color(1.00f,0.45f,0.15f), 0.75f), // sunset
                new GradientColorKey(new Color(0.05f,0.05f,0.1f),  1.00f), // midnight
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        sunIntensity = new AnimationCurve(
            new Keyframe(0.00f, 0.0f),
            new Keyframe(0.22f, 0.0f),
            new Keyframe(0.28f, 0.6f),  // sunrise
            new Keyframe(0.50f, 1.3f),  // noon
            new Keyframe(0.72f, 0.6f),  // sunset
            new Keyframe(0.78f, 0.0f),
            new Keyframe(1.00f, 0.0f)
        );

        // Ambient sky: night dark blue → day light blue
        ambientSkyColor = new Gradient();
        ambientSkyColor.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f,0.05f,0.12f), 0.00f),
                new GradientColorKey(new Color(0.55f,0.40f,0.20f), 0.25f), // dawn warm
                new GradientColorKey(new Color(0.55f,0.70f,0.90f), 0.45f),
                new GradientColorKey(new Color(0.55f,0.72f,0.92f), 0.50f),
                new GradientColorKey(new Color(0.50f,0.65f,0.85f), 0.75f), // sunset
                new GradientColorKey(new Color(0.05f,0.05f,0.12f), 1.00f),
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        ambientEquatorColor = new Gradient();
        ambientEquatorColor.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f,0.05f,0.08f), 0.00f),
                new GradientColorKey(new Color(0.40f,0.55f,0.30f), 0.50f),
                new GradientColorKey(new Color(0.05f,0.05f,0.08f), 1.00f),
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        ambientGroundColor = new Gradient();
        ambientGroundColor.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.02f,0.02f,0.02f), 0.00f),
                new GradientColorKey(new Color(0.12f,0.10f,0.07f), 0.50f),
                new GradientColorKey(new Color(0.02f,0.02f,0.02f), 1.00f),
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        // Fog: clear day → hazy at dusk/dawn → dark at night
        fogColor = new Gradient();
        fogColor.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f,0.05f,0.15f), 0.00f), // night
                new GradientColorKey(new Color(0.90f,0.60f,0.35f), 0.25f), // dawn orange
                new GradientColorKey(new Color(0.75f,0.88f,1.00f), 0.45f), // day
                new GradientColorKey(new Color(0.75f,0.88f,1.00f), 0.55f), // day
                new GradientColorKey(new Color(0.85f,0.55f,0.30f), 0.75f), // dusk
                new GradientColorKey(new Color(0.05f,0.05f,0.15f), 1.00f), // night
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        fogStart = new AnimationCurve(
            new Keyframe(0f, 300f),
            new Keyframe(0.5f, 600f),
            new Keyframe(1f, 300f)
        );

        fogEnd = new AnimationCurve(
            new Keyframe(0f, 800f),
            new Keyframe(0.5f, 1800f),
            new Keyframe(1f, 800f)
        );
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Only preview in editor if curves are already initialized (non-empty)
        if (sunLight != null && Application.isPlaying == false
            && sunIntensity != null && sunIntensity.keys.Length > 0
            && sunColor != null && sunColor.colorKeys.Length > 0)
        {
            ApplyTimeOfDay();
        }
    }
#endif
}
