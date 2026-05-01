using UnityEngine;

/// <summary>
/// Tüm oyun verilerini tutan merkezi veri sınıfı.
/// PlayerPrefs ile kaydedilir.
/// </summary>
public static class GameData
{
    public static int Coins
    {
        get => PlayerPrefs.GetInt("Coins", 0);
        set => PlayerPrefs.SetInt("Coins", value);
    }

    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt("CurrentLevel", 1);
        set => PlayerPrefs.SetInt("CurrentLevel", value);
    }

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

    // Başlangıçta 2.5m yarıçap → 7 katlı binada kısmi hasar mümkün.
    // Max (level 5): 12.5m → tüm binayı kapsar.
    public static float ExplosionRadius => 2.5f + WarheadLevel * 2f;
    public static float PlaneSpeed      => 20f  + FuelLevel    * 5f;
    public static float WingScale       => 1f   + WingLevel    * 0.3f;

    public static void Save() => PlayerPrefs.Save();
}
