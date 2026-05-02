using UnityEngine;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour
{
    [Header("Patlama Efekti")]
    public GameObject explosionVFXPrefab;
    public float explosionLightDuration = 0.3f;

    private bool _missionFired;

    void OnEnable()
    {
        GameEvents.OnPlaneImpact += HandleImpact;
        GameStateManager.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable()
    {
        GameEvents.OnPlaneImpact -= HandleImpact;
        GameStateManager.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Flying) _missionFired = false;
    }

    void HandleImpact(Vector3 impactPoint)
    {
        if (_missionFired) return;

        float radius     = GameData.ExplosionRadius;
        float blastForce = GameData.BlastForce;

        SpawnExplosionEffect(impactPoint, radius);

        // force zone yarıçapında tüm binaları bul (2× hasar yarıçapı)
        Collider[] hits    = Physics.OverlapSphere(impactPoint, radius * 2f);
        var affected       = new HashSet<TargetBuilding>();
        foreach (Collider col in hits)
        {
            var b = col.GetComponentInParent<TargetBuilding>();
            if (b != null) affected.Add(b);
        }

        foreach (var building in affected)
            building.OnExplosion(impactPoint, radius, blastForce);

        // Sahnedeki TÜM binalara göre toplam yıkım hesapla
        var allBuildings = FindObjectsByType<TargetBuilding>(FindObjectsSortMode.None);
        int totalSections = 0, totalDestroyed = 0;
        float rawCoins = 0f;

        foreach (var b in allBuildings)
        {
            int t = b.GetTotalSections();
            int d = b.GetDestroyedCount();
            totalSections  += t;
            totalDestroyed += d;
            if (t > 0) rawCoins += b.coinRewardFull * ((float)d / t);
        }

        float percent = totalSections > 0 ? (float)totalDestroyed / totalSections : 0f;
        int   earned  = Mathf.RoundToInt(rawCoins);

        GameData.Coins += earned;
        GameData.Save();

        _missionFired = true;
        GameEvents.OnMissionComplete?.Invoke(percent, earned);

        Debug.Log($"PATLAMA: %{percent * 100:F0} yikim | {earned} coin | BlastForce:{blastForce}");
    }

    // ──────────── VFX ────────────

    void SpawnExplosionEffect(Vector3 center, float radius)
    {
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, center, Quaternion.identity);
            return;
        }

        var root = new GameObject("ExplosionVFX");
        root.transform.position = center;

        SpawnFire  (root.transform, radius);
        SpawnSparks(root.transform, radius);
        SpawnSmoke (root.transform, radius);
        SpawnLight (root.transform, radius);

        Destroy(root, 6f);
    }

    static Shader ParticleShader()
    {
        var s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (s == null) s = Shader.Find("Particles/Standard Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        return s;
    }

    void SpawnFire(Transform parent, float radius)
    {
        var go = new GameObject("Fire");
        go.transform.SetParent(parent, false);

        var ps  = go.AddComponent<ParticleSystem>();
        var ren = go.GetComponent<ParticleSystemRenderer>();
        ren.material = new Material(ParticleShader());

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.3f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(2f, 7f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.4f, Mathf.Max(1f, radius * 0.6f));
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1.0f, 0.35f, 0.0f),
                                   new Color(1.0f, 0.80f, 0.1f));
        main.gravityModifier = -0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(30 + radius * 4)) });

        var sh = ps.shape;
        sh.enabled    = true;
        sh.shapeType  = ParticleSystemShapeType.Sphere;
        sh.radius     = Mathf.Max(0.3f, radius * 0.35f);

        ps.Play();
    }

    void SpawnSparks(Transform parent, float radius)
    {
        var go = new GameObject("Sparks");
        go.transform.SetParent(parent, false);

        var ps  = go.AddComponent<ParticleSystem>();
        var ren = go.GetComponent<ParticleSystemRenderer>();
        ren.material     = new Material(ParticleShader());
        ren.renderMode   = ParticleSystemRenderMode.Stretch;
        ren.velocityScale = 0.1f;
        ren.lengthScale   = 1.5f;

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.2f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 1.0f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(6f + radius, 18f + radius * 2f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1.0f, 0.95f, 0.5f),
                                   Color.white);
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(50 + radius * 6)) });

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = 0.1f;

        ps.Play();
    }

    void SpawnSmoke(Transform parent, float radius)
    {
        var go = new GameObject("Smoke");
        go.transform.SetParent(parent, false);

        var ps  = go.AddComponent<ParticleSystem>();
        var ren = go.GetComponent<ParticleSystemRenderer>();
        ren.material = new Material(ParticleShader());

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.5f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(2.0f, 4.0f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.5f, 2.5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(Mathf.Max(0.5f, radius * 0.5f),
                                                               Mathf.Max(1.5f, radius * 1.2f));
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(0.12f, 0.12f, 0.12f, 0.75f),
                                   new Color(0.35f, 0.30f, 0.25f, 0.45f));
        main.gravityModifier = -0.08f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(20 + radius * 3)) });

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = Mathf.Max(0.3f, radius * 0.4f);

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        var curve = new AnimationCurve(new Keyframe(0f, 0.4f), new Keyframe(1f, 1f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.2f, 0.15f, 0.1f), 0f),
                    new GradientColorKey(new Color(0.4f, 0.4f, 0.4f), 1f) },
            new[] { new GradientAlphaKey(0.7f, 0f),
                    new GradientAlphaKey(0f,   1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        ps.Play();
    }

    void SpawnLight(Transform parent, float radius)
    {
        var go  = new GameObject("Flash");
        go.transform.SetParent(parent, false);
        var lt  = go.AddComponent<Light>();
        lt.type      = LightType.Point;
        lt.color     = new Color(1f, 0.55f, 0.1f);
        lt.intensity = 12f;
        lt.range     = radius * 5f;
        Destroy(go, explosionLightDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, GameData.ExplosionRadius);
    }
}
