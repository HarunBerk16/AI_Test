using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class RestoreLowPoly
{
    public static void Execute()
    {
        // 1. SetupEnvironment'ın eklediği grupları sil
        foreach (var name in new[] { "NatureGroup", "TerrainGroup" })
        {
            var go = GameObject.Find(name);
            if (go != null) { Object.DestroyImmediate(go); Debug.Log($"[RestoreLowPoly] {name} silindi."); }
        }

        // 2. Ground'u düz canlı yeşile döndür
        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Ground_Grass.mat");
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", new Color(0.18f, 0.78f, 0.22f));
                mat.SetFloat("_Smoothness", 0f);
                mat.SetFloat("_Metallic", 0f);
                AssetDatabase.CreateAsset(mat, "Assets/LowPolyMaterials/Ground_Grass.mat");
            }
            // Texture'ları temizle
            mat.SetTexture("_BaseMap", null);
            mat.SetTexture("_BumpMap", null);
            mat.SetColor("_BaseColor", new Color(0.18f, 0.78f, 0.22f));
            mat.SetFloat("_Smoothness", 0f);
            EditorUtility.SetDirty(mat);

            ground.GetComponent<MeshRenderer>().sharedMaterial = mat;
            ground.transform.position   = new Vector3(0f, 0f, 350f);
            ground.transform.localScale = new Vector3(100f, 1f, 160f);
            EditorUtility.SetDirty(ground);
            Debug.Log("[RestoreLowPoly] Ground düz yeşile döndürüldü.");
        }

        // 3. RoadOverlay varsa ölçek/konum kontrol et
        var road = GameObject.Find("RoadOverlay");
        if (road != null)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Road.mat");
            if (mat != null)
            {
                mat.SetTexture("_BaseMap", null);
                mat.SetColor("_BaseColor", new Color(0.72f, 0.65f, 0.46f));
                EditorUtility.SetDirty(mat);
                road.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
            road.transform.position   = new Vector3(0f, 0.01f, 350f);
            road.transform.localScale = new Vector3(5f, 1f, 160f);
            EditorUtility.SetDirty(road);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("[RestoreLowPoly] Tamamlandı.");
    }
}
