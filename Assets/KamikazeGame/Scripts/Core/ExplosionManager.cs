using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Patlama yönetimi.
/// Her binaya OnExplosion yalnızca bir kez çağrılır (çoklu collider'dan gelen tekrarları filtreler).
/// </summary>
public class ExplosionManager : MonoBehaviour
{
    [Header("Patlama Efekti")]
    public GameObject explosionVFXPrefab;
    public float explosionLightDuration = 0.3f;

    void OnEnable()  => GameEvents.OnPlaneImpact += HandleImpact;
    void OnDisable() => GameEvents.OnPlaneImpact -= HandleImpact;

    void HandleImpact(Vector3 impactPoint)
    {
        float radius = GameData.ExplosionRadius;

        Debug.Log($"PATLAMA! Merkez: {impactPoint} | Yaricap: {radius}m");

        SpawnExplosionEffect(impactPoint, radius);

        // Aynı binayı birden fazla çağırmamak için HashSet kullan
        Collider[] hits = Physics.OverlapSphere(impactPoint, radius);
        var affected = new HashSet<TargetBuilding>();
        foreach (Collider col in hits)
        {
            var building = col.GetComponentInParent<TargetBuilding>();
            if (building != null) affected.Add(building);
        }

        foreach (var building in affected)
            building.OnExplosion(impactPoint, radius);
    }

    void SpawnExplosionEffect(Vector3 center, float radius)
    {
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, center, Quaternion.identity);
            return;
        }

        GameObject lightObj = new GameObject("ExplosionLight");
        lightObj.transform.position = center;

        Light light = lightObj.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = new Color(1f, 0.5f, 0.1f);
        light.intensity = 10f;
        light.range     = radius * 3f;

        Destroy(lightObj, explosionLightDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, GameData.ExplosionRadius);
    }
}
