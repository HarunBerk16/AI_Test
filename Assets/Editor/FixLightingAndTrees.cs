using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixLightingAndTrees
{
    public static string Execute()
    {
        FixLight();
        RemoveDayNight();
        ApplyTreeShader();
        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        return "Done.";
    }

    static void FixLight()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type != LightType.Directional) continue;

            // Işık kameranın ÖNÜNDEN gelsin: kamera +Z'ye bakıyor,
            // ışık -Z tarafından (kameranın arkasından) gelip sahneyi öne doğru aydınlatsın.
            // (45, 210, 0) → ışık yukarı-sol-arkadan geliyor, cisimler cepheden aydınlanıyor.
            l.transform.rotation = Quaternion.Euler(45f, 210f, 0f);
            l.color = new Color(1.0f, 0.97f, 0.90f);
            l.intensity = 1.25f;
            l.colorTemperature = 6000f;
            l.useColorTemperature = true;
            l.shadows = LightShadows.Soft;
            EditorUtility.SetDirty(l);
            break;
        }
    }

    static void RemoveDayNight()
    {
        var dnc = GameObject.Find("DayNightController");
        if (dnc != null)
        {
            Object.DestroyImmediate(dnc);
            Debug.Log("[Fix] DayNightController removed.");
        }
    }

    static void ApplyTreeShader()
    {
        var shader = Shader.Find("KamikazeGame/LowPolyTree");
        if (shader == null)
        {
            // Shader hasn't compiled yet — try by asset path
            var shaderAsset = AssetDatabase.LoadAssetAtPath<Shader>(
                "Assets/KamikazeGame/Shaders/LowPolyTree.shader");
            if (shaderAsset == null) { Debug.LogWarning("[Fix] Tree shader not found."); return; }
            shader = shaderAsset;
        }

        // Create 3 tree materials with color variation
        var configs = new (string name, Color leaf, float trunkH)[]
        {
            ("TreeMat_A", new Color(0.12f, 0.48f, 0.08f), 3.5f),   // deep green
            ("TreeMat_B", new Color(0.06f, 0.40f, 0.16f), 4.0f),   // forest green
            ("TreeMat_C", new Color(0.26f, 0.62f, 0.06f), 2.5f),   // spring green
        };

        var mats = new Material[configs.Length];
        for (int i = 0; i < configs.Length; i++)
        {
            var c = configs[i];
            var mat = new Material(shader);
            mat.name = c.name;
            mat.SetColor("_TrunkColor",  new Color(0.32f, 0.18f, 0.07f));
            mat.SetColor("_LeafColor",   c.leaf);
            mat.SetFloat("_TrunkHeight", c.trunkH);
            mat.SetFloat("_BlendZone",   2.5f);
            var path = $"Assets/LowPolyMaterials/{c.name}.mat";
            AssetDatabase.CreateAsset(mat, path);
            mats[i] = mat;
        }
        AssetDatabase.SaveAssets();

        // Assign to all tree LOD0/LOD1 objects
        var allMR = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var mr in allMR)
        {
            var name = mr.gameObject.name;
            if (!name.StartsWith("Tree_")) continue;

            Material treeMat;
            if      (name.StartsWith("Tree_01") || name.StartsWith("Tree_04")) treeMat = mats[0];
            else if (name.StartsWith("Tree_02") || name.StartsWith("Tree_05")) treeMat = mats[1];
            else                                                                treeMat = mats[2];

            mr.sharedMaterial = treeMat;
            EditorUtility.SetDirty(mr);
            count++;
        }

        // Also fix bushes — give them leaf-only (no trunk needed, they're low)
        var bushMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Bush.mat");
        if (bushMat != null)
        {
            bushMat.SetColor("_BaseColor", new Color(0.10f, 0.44f, 0.12f));
            EditorUtility.SetDirty(bushMat);
        }

        Debug.Log($"[Fix] Tree shader applied to {count} renderers.");
    }
}
