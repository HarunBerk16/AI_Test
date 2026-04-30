using UnityEditor;
using UnityEngine;

public class FixCollision
{
    public static void Execute()
    {
        // 1. Plane'e Kinematic Rigidbody ekle
        // (Kinematic = fizik motoru hareket ettirmez, biz kodla hareket ettiriyoruz)
        // (Ama trigger detection çalışması için Rigidbody şart)
        GameObject plane = GameObject.Find("Plane");
        if (plane != null)
        {
            Rigidbody rb = plane.GetComponent<Rigidbody>();
            if (rb == null) rb = plane.AddComponent<Rigidbody>();
            rb.isKinematic = true;    // Fizik motoru müdahale etmesin
            rb.useGravity = false;    // Yerçekimi olmasın
            Debug.Log("Plane: Kinematic Rigidbody eklendi.");
        }

        // 2. TargetBuilding_01 parent'ına TargetBuilding scripti var mı kontrol et
        GameObject building = GameObject.Find("TargetBuilding_01");
        if (building != null)
        {
            TargetBuilding tb = building.GetComponent<TargetBuilding>();
            if (tb == null)
            {
                building.AddComponent<TargetBuilding>();
                Debug.Log("TargetBuilding_01: TargetBuilding bileşeni eklendi.");
            }
            else
            {
                Debug.Log("TargetBuilding_01: TargetBuilding zaten mevcut.");
            }
        }

        Debug.Log("Collision düzeltmesi tamamlandi.");
    }
}
