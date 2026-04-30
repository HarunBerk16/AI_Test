using UnityEngine;

/// <summary>
/// Uçak çarptığında patlamayı yönetir.
/// - Patlama yarıçapı GameData.ExplosionRadius'dan gelir (warhead upgrade)
/// - Yarıçap içindeki tüm BuildingSection'ları yıkar
/// - Görsel efekt (basit ışık patlaması) oynar
/// </summary>
public class ExplosionManager : MonoBehaviour
{
    [Header("Patlama Efekti")]
    public GameObject explosionVFXPrefab;   // Atanırsa instantiate edilir
    public float explosionLightDuration = 0.3f;

    void OnEnable()
    {
        GameEvents.OnPlaneImpact += HandleImpact;
    }

    void OnDisable()
    {
        GameEvents.OnPlaneImpact -= HandleImpact;
    }

    void HandleImpact(Vector3 impactPoint)
    {
        float radius = GameData.ExplosionRadius;

        Debug.Log($"PATLAMA! Merkez: {impactPoint} | Yaricap: {radius}m");

        // Görsel efekt
        SpawnExplosionEffect(impactPoint, radius);

        // Yarıçap içindeki binaları bul ve hasar ver
        Collider[] hits = Physics.OverlapSphere(impactPoint, radius);
        foreach (Collider col in hits)
        {
            TargetBuilding building = col.GetComponentInParent<TargetBuilding>();
            if (building != null)
            {
                building.OnExplosion(impactPoint, radius);
            }
        }
    }

    void SpawnExplosionEffect(Vector3 center, float radius)
    {
        // Prefab varsa kullan
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, center, Quaternion.identity);
            return;
        }

        // Prefab yoksa basit ışık efekti
        GameObject lightObj = new GameObject("ExplosionLight");
        lightObj.transform.position = center;

        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.5f, 0.1f);
        light.intensity = 10f;
        light.range = radius * 3f;

        // Kısa süre sonra sil
        Destroy(lightObj, explosionLightDuration);
    }

    // Gizmos: Editor'de patlama yarıçapını göster
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, GameData.ExplosionRadius);
    }
}
