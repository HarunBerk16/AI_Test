using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Hedef bina.
/// Binada birden fazla "bölüm" (katlar, kuleler) var.
/// Her bölüm bağımsız olarak yıkılabilir.
/// Toplam yıkım yüzdesi hesaplanır → coin kazanılır.
/// </summary>
public class TargetBuilding : MonoBehaviour
{
    [Header("Bina Ayarları")]
    public string buildingName = "Hedef Bina";
    public int coinRewardFull = 100;    // Tam yıkımda kazanılacak coin

    // Bölümler (child objelerde BuildingSection bileşeni olmalı)
    private List<BuildingSection> _sections = new List<BuildingSection>();
    private int _totalSections;
    private bool _resultShown = false;

    void Start()
    {
        // Tüm child section'ları bul
        GetComponentsInChildren<BuildingSection>(_sections);
        _totalSections = _sections.Count;

        if (_totalSections == 0)
            Debug.LogWarning($"{buildingName}: BuildingSection bileşeni bulunamadi!");
    }

    /// <summary>
    /// Patlama olduğunda GameManager buraya bildirir.
    /// </summary>
    public void OnExplosion(Vector3 explosionCenter, float explosionRadius)
    {
        foreach (var section in _sections)
        {
            if (section.IsDestroyed) continue;

            // Collider'ın en yakın noktasını bul (merkez değil, yüzey)
            Collider col = section.GetComponent<Collider>();
            if (col != null)
            {
                Vector3 closest = col.ClosestPoint(explosionCenter);
                float dist = Vector3.Distance(closest, explosionCenter);
                if (dist <= explosionRadius)
                    section.Destroy();
            }
            else
            {
                // Collider yoksa merkez mesafesiyle kontrol et
                float dist = Vector3.Distance(section.transform.position, explosionCenter);
                if (dist <= explosionRadius)
                    section.Destroy();
            }
        }

        ShowResult();
    }

    void ShowResult()
    {
        if (_resultShown) return;
        _resultShown = true;

        int destroyedCount = 0;
        foreach (var s in _sections)
            if (s.IsDestroyed) destroyedCount++;

        float percent = _totalSections > 0 ? (float)destroyedCount / _totalSections : 0f;
        int earned = Mathf.RoundToInt(coinRewardFull * percent);

        Debug.Log($"Yikim: %{percent * 100:F0} | Kazanilan: {earned} coin");

        // Para ekle
        GameData.Coins += earned;
        GameData.Save();

        // UI'ya bildir
        GameEvents.OnMissionComplete?.Invoke(percent, earned);
    }

    public float GetDestructionPercent()
    {
        if (_totalSections == 0) return 0f;
        int destroyedCount = 0;
        foreach (var s in _sections)
            if (s.IsDestroyed) destroyedCount++;
        return (float)destroyedCount / _totalSections;
    }
}
