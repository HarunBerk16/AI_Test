using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ImproveVisuals
{
    public static string Execute()
    {
        FixFog();
        FixMaterials();
        FixLighting();
        FixCamera();
        FixSkybox();

        EditorSceneManager.MarkAllScenesDirty();
        return "Visuals improved successfully!";
    }

    static void FixFog()
    {
        // Push fog far out so trees/environment are fully visible
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 600f;
        RenderSettings.fogEndDistance = 1800f;
        // Warmer, lighter fog color matching a clear day sky horizon
        RenderSettings.fogColor = new Color(0.75f, 0.88f, 1.0f, 1f);
    }

    static void FixMaterials()
    {
        // Ground: richer, less neon grass
        SetMatColor("Assets/LowPolyMaterials/Ground_Grass.mat", new Color(0.13f, 0.52f, 0.12f));

        // Leaves: natural variation, less neon/cyan
        SetMatColor("Assets/LowPolyMaterials/Leaf_A.mat", new Color(0.18f, 0.55f, 0.10f));   // mid green
        SetMatColor("Assets/LowPolyMaterials/Leaf_B.mat", new Color(0.06f, 0.42f, 0.18f));   // deep forest green
        SetMatColor("Assets/LowPolyMaterials/Leaf_C.mat", new Color(0.28f, 0.68f, 0.06f));   // spring yellow-green
        SetMatColor("Assets/LowPolyMaterials/Bush.mat",   new Color(0.10f, 0.46f, 0.14f));   // natural bush

        // Trunk: warm brown
        SetMatColor("Assets/LowPolyMaterials/Trunk.mat", new Color(0.35f, 0.20f, 0.08f));

        // Rocks: warmer grey
        SetMatColor("Assets/LowPolyMaterials/Rock.mat",    new Color(0.58f, 0.54f, 0.50f));
        SetMatColor("Assets/LowPolyMaterials/RockAlt.mat", new Color(0.50f, 0.55f, 0.48f));

        // Road: slightly more saturated earth tone
        SetMatColor("Assets/LowPolyMaterials/Road.mat", new Color(0.70f, 0.60f, 0.42f));
    }

    static void SetMatColor(string path, Color color)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) return;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color", color);
        EditorUtility.SetDirty(mat);
    }

    static void FixLighting()
    {
        // Brighter, warmer ambient to make colors pop
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.55f, 0.70f, 0.90f);  // sky blue
        RenderSettings.ambientEquatorColor = new Color(0.40f, 0.55f, 0.30f);  // horizon green/warm
        RenderSettings.ambientGroundColor  = new Color(0.12f, 0.10f, 0.07f);  // dark earth
        RenderSettings.ambientIntensity = 1.0f;

        // Directional light: daylight color temp, stronger
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.color = new Color(1.0f, 0.96f, 0.88f);  // warm white daylight
                l.intensity = 1.3f;
                l.colorTemperature = 6500f;
                l.useColorTemperature = true;
                EditorUtility.SetDirty(l);
            }
        }

        // Increase shadow distance for better quality shadows at distance
        QualitySettings.shadowDistance = 500f;
    }

    static void FixCamera()
    {
        // Push camera far clip much further
        var cam = Camera.main;
        if (cam != null)
        {
            cam.farClipPlane = 3000f;
            EditorUtility.SetDirty(cam);
        }
    }

    static void FixSkybox()
    {
        // Ensure skybox mode is set correctly
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

        // Try to update the BOXOPHOBIC skybox material if present
        var skybox = RenderSettings.skybox;
        if (skybox != null)
        {
            Debug.Log($"[VisualFix] Skybox: {skybox.name}, Shader: {skybox.shader.name}");
            // Increase exposure if available
            if (skybox.HasProperty("_Exposure"))
                skybox.SetFloat("_Exposure", 1.2f);
            if (skybox.HasProperty("_AtmosphereThickness"))
                skybox.SetFloat("_AtmosphereThickness", 1.0f);
            EditorUtility.SetDirty(skybox);
        }

        DynamicGI.UpdateEnvironment();
    }
}
