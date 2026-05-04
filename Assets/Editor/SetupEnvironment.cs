using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupEnvironment
{
    [MenuItem("KamikazeGame/Setup Environment")]
    public static void Run()
    {
        SetSkybox();
        SetLighting();

        // Çevre (ağaç/kaya/zemin) low-poly sistemine devredildi.
        // Environment yoksa kur, varsa dokunma.
        if (GameObject.Find("Environment") == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/LowPolyMaterials"))
                AssetDatabase.CreateFolder("Assets", "LowPolyMaterials");
            SetupLowPolyEnvironment.Execute();
        }
        else
        {
            Debug.Log("[SetupEnvironment] Low-poly Environment zaten mevcut, atlandı.");
        }

        var gm = GameObject.Find("GameManager");
        if (gm != null) EditorUtility.SetDirty(gm);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SetupEnvironment] Tamamlandı.");
    }

    static void SetSkybox()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/BOXOPHOBIC/Skybox Cubemap Extended/Demo/Materials/Skybox Cubemap Extended Day.mat");
        if (mat == null) { Debug.LogWarning("[SetupEnvironment] Skybox mat bulunamadı."); return; }
        RenderSettings.skybox = mat;
        DynamicGI.UpdateEnvironment();
        Debug.Log("[SetupEnvironment] Skybox: " + mat.name);
    }

    static void SetExplosionVFX()
    {
        string[] guids = AssetDatabase.FindAssets("BigExplosion t:Prefab");
        if (guids.Length == 0) { Debug.LogWarning("[SetupEnvironment] BigExplosion prefab bulunamadı."); return; }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));

        var em = Object.FindFirstObjectByType<ExplosionManager>();
        if (em == null) { Debug.LogWarning("[SetupEnvironment] ExplosionManager yok."); return; }

        var so = new SerializedObject(em);
        so.FindProperty("explosionVFXPrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(em);
        Debug.Log("[SetupEnvironment] Explosion VFX: " + prefab.name);
    }

    static void SetLighting()
    {
        var light = Object.FindFirstObjectByType<Light>();
        if (light == null) return;
        light.color          = new Color(1.0f, 0.95f, 0.85f);
        light.intensity      = 1.15f;
        light.shadowStrength = 0.6f;
        EditorUtility.SetDirty(light);
        Debug.Log("[SetupEnvironment] Işık ayarlandı.");
    }
}
