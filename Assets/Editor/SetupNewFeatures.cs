using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class SetupNewFeatures
{
    public static void Execute()
    {
        // --- 1. Eski binayı sil (artık LevelBuilder hallediyor) ---
        GameObject oldBuilding = GameObject.Find("TargetBuilding_01");
        if (oldBuilding != null)
        {
            GameObject.DestroyImmediate(oldBuilding);
            Debug.Log("Eski bina silindi.");
        }

        // --- 2. Zemin ekle ---
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, 0, 0);
            ground.transform.localScale = new Vector3(20, 1, 100); // geniş ve uzun
            ground.tag = "Ground";

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.35f, 0.15f); // yeşil zemin
            ground.GetComponent<Renderer>().material = mat;

            Debug.Log("Zemin eklendi.");
        }

        // --- 3. GameManager'a LevelBuilder ekle ---
        GameObject gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            if (gm.GetComponent<LevelBuilder>() == null)
            {
                gm.AddComponent<LevelBuilder>();
                Debug.Log("LevelBuilder eklendi.");
            }

            // MissionController ekle
            if (gm.GetComponent<MissionController>() == null)
            {
                var mc = gm.AddComponent<MissionController>();

                // Plane referansı
                GameObject plane = GameObject.Find("Plane");
                if (plane != null) mc.plane = plane.transform;

                // CameraFollow referansı
                var cam = Camera.main;
                if (cam != null)
                {
                    var cf = cam.GetComponent<CameraFollow>();
                    if (cf != null) mc.cameraFollow = cf;
                }

                Debug.Log("MissionController eklendi.");
            }
        }

        // --- 4. Sahneyi kaydet ---
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("SampleScene kaydedildi.");
    }
}
