using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        PlaneController plane = FindObjectOfType<PlaneController>();
        if (plane != null)
        {
            plane.SetSpeed(UpgradeData.HullSpeed(GameData.HullLevel));
            Debug.Log($"Ucak ayarlandi — Hiz: {GameData.PlaneSpeed}, Patlama: {GameData.ExplosionRadius}m, Govde: {UpgradeData.HullName(GameData.HullLevel)}");
        }
    }
}
