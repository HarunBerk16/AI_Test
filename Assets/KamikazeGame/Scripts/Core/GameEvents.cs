using System;

/// <summary>
/// Oyun genelinde event sistemi.
/// Script'ler birbirini doğrudan çağırmak yerine event kullanır.
/// C++ analoji: function pointer / callback sistemi.
/// </summary>
public static class GameEvents
{
    // Görev tamamlandı: (yıkımYüzdesi, kazanilanCoin)
    public static Action<float, int> OnMissionComplete;

    // Uçak hedefe çarptı
    public static Action<UnityEngine.Vector3> OnPlaneImpact;

    // Uçak yere çarptı (coin yok)
    public static Action OnPlaneCrash;

    // Uçak hedefi geçti (kaçırdı)
    public static Action OnTargetMissed;

    // Upgrade yapıldı
    public static Action OnUpgradePurchased;

    // Pause toggle
    public static Action<bool> OnPauseChanged;
}
