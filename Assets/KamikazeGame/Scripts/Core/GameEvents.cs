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

    // Upgrade yapıldı
    public static Action OnUpgradePurchased;
}
