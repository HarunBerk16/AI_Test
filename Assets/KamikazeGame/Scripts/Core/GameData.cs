using UnityEngine;

/// <summary>
/// Tüm oyun verilerini tutan merkezi veri sınıfı.
/// PlayerPrefs ile kaydedilir.
/// </summary>
public static class GameData
{
    // Para
    public static int Coins
    {
        get => PlayerPrefs.GetInt("Coins", 0);
        set => PlayerPrefs.SetInt("Coins", value);
    }

    // Aktif level (1'den başlar)
    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt("CurrentLevel", 1);
        set => PlayerPrefs.SetInt("CurrentLevel", value);
    }

    // Upgrade seviyeleri (0 = başlangıç)
    public static int WarheadLevel
    {
        get => PlayerPrefs.GetInt("WarheadLevel", 0);
        set => PlayerPrefs.SetInt("WarheadLevel", value);
    }

    public static int WingLevel
    {
        get => PlayerPrefs.GetInt("WingLevel", 0);
        set => PlayerPrefs.SetInt("WingLevel", value);
    }

    public static int FuelLevel
    {
        get => PlayerPrefs.GetInt("FuelLevel", 0);
        set => PlayerPrefs.SetInt("FuelLevel", value);
    }

    // Upgrade'e göre değerler
    public static float ExplosionRadius => 6f + WarheadLevel * 3f;   // Patlama yarıçapı
    public static float PlaneSpeed      => 20f + FuelLevel * 5f;      // Uçuş hızı
    public static float WingScale       => 1f + WingLevel * 0.3f;     // Kanat boyutu (görsel)

    public static void Save() => PlayerPrefs.Save();
}
