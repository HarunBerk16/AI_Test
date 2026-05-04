using UnityEngine;

public static class UpgradeData
{
    public const int MaxWarheadLevel   = 5;
    public const int MaxHullLevel      = 2; // 3 gövde tipi: 0, 1, 2
    public const int MaxStabilityLevel = 5;

    static int GetCost(int currentLevel, int maxLevel, int baseCost, float multiplier = 1.8f)
    {
        if (currentLevel >= maxLevel) return -1;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, currentLevel));
    }

    // ── Warhead ─────────────────────────────────────────
    public static int   WarheadCost(int level)   => GetCost(level, MaxWarheadLevel, 50);
    public static float WarheadRadius(int level) => 2.5f + level * 2f;

    // ── Gövde (Hull) ─────────────────────────────────────
    // Sadece 2 yükseltme: Küçük→Boru→Shahed
    public static int   HullCost(int level)  => GetCost(level, MaxHullLevel, 150);
    public static float HullSpeed(int level) => level switch { 0 => 22f, 1 => 32f, _ => 44f };
    public static string HullName(int level) => level switch
    {
        0 => "Küçük Uçak",
        1 => "Boru Gövde",
        _ => "Shahed"
    };
    public static string HullNextName(int level) => level switch
    {
        0 => "Boru Gövde",
        1 => "Shahed",
        _ => "—"
    };

    // ── Stabilite (eski Kanat) ───────────────────────────
    public static int   StabilityCost(int level)      => GetCost(level, MaxStabilityLevel, 35);
    public static float StabilityTurnSpeed(int level) => 80f + level * 20f;
}
