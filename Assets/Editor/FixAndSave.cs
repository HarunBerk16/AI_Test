using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FixAndSave
{
    public static void Execute()
    {
        // Rigidbody ekle
        GameObject plane = GameObject.Find("Plane");
        if (plane != null)
        {
            Rigidbody rb = plane.GetComponent<Rigidbody>();
            if (rb == null) rb = plane.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
            EditorUtility.SetDirty(plane);
            Debug.Log("Rigidbody eklendi.");
        }

        // TargetBuilding component kontrolü
        GameObject building = GameObject.Find("TargetBuilding_01");
        if (building != null)
        {
            if (building.GetComponent<TargetBuilding>() == null)
            {
                building.AddComponent<TargetBuilding>();
                EditorUtility.SetDirty(building);
                Debug.Log("TargetBuilding bileşeni eklendi.");
            }
        }

        // Sahneyi kaydet
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
            "Assets/Scenes/SampleScene.unity");

        Debug.Log("SampleScene kaydedildi!");
    }
}
