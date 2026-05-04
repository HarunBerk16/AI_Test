using UnityEngine;

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

    // Gövde tipi: 0=Küçük Uçak, 1=Boru Gövde, 2=Shahed
    public static int HullLevel
    {
        get => PlayerPrefs.GetInt("HullLevel", 0);
        set => PlayerPrefs.SetInt("HullLevel", value);
    }

    // Stabilite (eski Kanat): manevra kabiliyeti
    public static int StabilityLevel
    {
        get => PlayerPrefs.GetInt("StabilityLevel", 0);
        set => PlayerPrefs.SetInt("StabilityLevel", value);
    }

    // Hull'a göre azami warhead seviyesi: küçük→2, tüp→4, shahed→5
    public static int MaxWarheadForHull => HullLevel switch
    {
        0 => 2,
        1 => 4,
        _ => UpgradeData.MaxWarheadLevel
    };

    public static float ExplosionRadius => 2.5f + WarheadLevel * 2f;
    public static float BlastForce      => 50f  + WarheadLevel * 80f;
    public static float PlaneSpeed      => UpgradeData.HullSpeed(HullLevel);
    public static float StabilityScale  => 1f   + StabilityLevel * 0.3f;

    public static void Save() => PlayerPrefs.Save();
}
