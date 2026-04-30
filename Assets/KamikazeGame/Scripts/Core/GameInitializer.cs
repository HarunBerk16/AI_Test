using UnityEngine;

/// <summary>
/// SampleScene başladığında upgrade değerlerini uçağa uygular.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        // Uçağı bul
        PlaneController plane = FindObjectOfType<PlaneController>();
        if (plane != null)
        {
            plane.speed     = UpgradeData.FuelSpeed(GameData.FuelLevel);
            plane.turnSpeed = UpgradeData.WingTurnSpeed(GameData.WingLevel);
            Debug.Log($"Ucak ayarlandi — Hiz: {plane.speed}, Manevra: {plane.turnSpeed}, Patlama: {GameData.ExplosionRadius}m");
        }
    }
}
