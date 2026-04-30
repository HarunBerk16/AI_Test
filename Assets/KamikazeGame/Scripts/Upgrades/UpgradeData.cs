using UnityEngine;

/// <summary>
/// Her upgrade türünün fiyat/seviye tablosu.
/// Seviye arttıkça fiyat üstel büyür.
/// </summary>
public static class UpgradeData
{
    public const int MaxLevel = 5;

    // Fiyat hesabı: baseCost * (multiplier ^ level)
    public static int GetCost(int currentLevel, int baseCost, float multiplier = 1.8f)
    {
        if (currentLevel >= MaxLevel) return -1; // Max seviye
        return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, currentLevel));
    }

    // Warhead: patlama yarıçapı
    public static int WarheadCost(int level)  => GetCost(level, baseCost: 50);
    public static float WarheadRadius(int level) => 3f + level * 2f;

    // Yakıt: hız
    public static int FuelCost(int level)     => GetCost(level, baseCost: 40);
    public static float FuelSpeed(int level)  => 20f + level * 5f;

    // Kanat: manevra kabiliyeti (turnSpeed)
    public static int WingCost(int level)     => GetCost(level, baseCost: 35);
    public static float WingTurnSpeed(int level) => 80f + level * 20f;
}
