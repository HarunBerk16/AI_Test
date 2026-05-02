using UnityEngine;
using System.Collections.Generic;

public class TargetBuilding : MonoBehaviour
{
    [Header("Bina Ayarları")]
    public string buildingName  = "Hedef Bina";
    public int    coinRewardFull = 100;

    private List<BuildingSection> _sections = new List<BuildingSection>();
    private int _totalSections;

    void Start()
    {
        GetComponentsInChildren<BuildingSection>(_sections);
        _totalSections = _sections.Count;
        if (_totalSections == 0)
            Debug.LogWarning($"{buildingName}: BuildingSection bulunamadi!");
    }

    // Patlama hasarı ve fizik kuvveti uygular. Sonuç hesabı ExplosionManager'da.
    public void OnExplosion(Vector3 explosionCenter, float explosionRadius, float blastForce)
    {
        float forceRadius = explosionRadius * 2f;

        foreach (var section in _sections)
        {
            if (section.IsDestroyed) continue;

            Collider col = section.GetComponent<Collider>();
            Vector3 closest = col != null
                ? col.ClosestPoint(explosionCenter)
                : section.transform.position;
            float dist = Vector3.Distance(closest, explosionCenter);

            if (dist <= explosionRadius)
                section.Destroy(explosionCenter, blastForce);
            else if (dist <= forceRadius)
            {
                float falloff = 1f - (dist - explosionRadius) / explosionRadius;
                section.ApplyForce(explosionCenter, blastForce * falloff * 0.4f);
            }
        }
    }

    public int GetDestroyedCount()
    {
        int n = 0;
        foreach (var s in _sections)
            if (s.IsDestroyed) n++;
        return n;
    }

    public int   GetTotalSections()    => _totalSections;
    public float GetDestructionPercent() =>
        _totalSections > 0 ? (float)GetDestroyedCount() / _totalSections : 0f;
}
