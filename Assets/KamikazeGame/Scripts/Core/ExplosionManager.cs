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
            var vfx = Instantiate(explosionVFXPrefab, center, Quaternion.identity);
            // Particle sistemleri loop'ta olabilir — belirli süre sonra zorla sil
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.loop       = false;
                main.stopAction = ParticleSystemStopAction.None;
            }
            Destroy(vfx, 6f);
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
        // Ana ateş topu — büyük, parlak sarı→turuncu→kırmızı
        var go = new GameObject("Fireball");
        go.transform.SetParent(parent, false);

        var ps  = go.AddComponent<ParticleSystem>();
        var ren = go.GetComponent<ParticleSystemRenderer>();
        ren.material = new Material(ParticleShader());

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(radius * 2f, radius * 4f);
        main.startSize       = new ParticleSystem.MinMaxCurve(radius * 1.8f, radius * 4f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1.0f, 1.0f, 0.0f),
                                   new Color(1.0f, 0.15f, 0.0f));
        main.gravityModifier = -0.4f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(18 + radius * 2)) });

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = 0.15f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 1f, 0f),    0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0f), 0.35f),
                    new GradientColorKey(new Color(0.8f, 0.05f, 0f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.9f, 0.4f),
                    new GradientAlphaKey(0f,  1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        var curve = new AnimationCurve(new Keyframe(0f, 0.2f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0.4f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ps.Play();
    }

    void SpawnSparks(Transform parent, float radius)
    {
        // Enkaz parçaları — turuncu/kırmızı bloklar dışa fırlıyor
        var go = new GameObject("Debris");
        go.transform.SetParent(parent, false);

        var ps  = go.AddComponent<ParticleSystem>();
        var ren = go.GetComponent<ParticleSystemRenderer>();
        ren.material = new Material(ParticleShader());

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.1f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.9f, 1.8f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(radius * 2.5f, radius * 6f);
        main.startSize       = new ParticleSystem.MinMaxCurve(radius * 0.35f, radius * 0.9f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1.0f, 0.55f, 0.0f),
                                   new Color(0.7f, 0.05f, 0.0f));
        main.gravityModifier = 2.0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.RoundToInt(35 + radius * 5)) });

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius    = Mathf.Max(0.2f, radius * 0.25f);

        ps.Play();
    }

    void SpawnSmoke(Transform parent, float radius)
    {
        // Şok dalgası halkası — düzlemsel turuncu/sarı patlama halkası
        var go = new GameObject("Shockwave");
        go.transform.SetParent(parent, false);

        var ps  = go.AddComponent<ParticleSystem>();
        var ren = go.GetComponent<ParticleSystemRenderer>();
        ren.material = new Material(ParticleShader());

        var main = ps.main;
        main.loop            = false;
        main.duration        = 0.05f;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(radius * 4f, radius * 8f);
        main.startSize       = new ParticleSystem.MinMaxCurve(radius * 1f, radius * 2f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
                                   new Color(1.0f, 0.9f, 0.2f, 0.95f),
                                   new Color(1.0f, 0.45f, 0.0f, 0.7f));
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)50) });

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius    = 0.05f;
        sh.arc       = 360f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 1f, 0.3f), 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0f),  1f) },
            new[] { new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        ps.Play();
    }

    void SpawnLight(Transform parent, float radius)
    {
        var go  = new GameObject("Flash");
        go.transform.SetParent(parent, false);
        var lt  = go.AddComponent<Light>();
        lt.type      = LightType.Point;
        lt.color     = new Color(1f, 0.7f, 0.1f);
        lt.intensity = 50f;
        lt.range     = radius * 12f;
        Destroy(go, explosionLightDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, GameData.ExplosionRadius);
    }
}
