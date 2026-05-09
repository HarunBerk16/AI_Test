using UnityEngine;
using UnityEditor;
using System.Text;

public class AnalyzeVisuals
{
    public static string Execute()
    {
        var sb = new StringBuilder();

        // Fog settings
        sb.AppendLine("=== FOG ===");
        sb.AppendLine($"Fog Enabled: {RenderSettings.fog}");
        sb.AppendLine($"Fog Mode: {RenderSettings.fogMode}");
        sb.AppendLine($"Fog Color: {RenderSettings.fogColor}");
        sb.AppendLine($"Fog Density: {RenderSettings.fogDensity}");
        sb.AppendLine($"Fog Start: {RenderSettings.fogStartDistance}");
        sb.AppendLine($"Fog End: {RenderSettings.fogEndDistance}");

        // Skybox
        sb.AppendLine("\n=== SKYBOX ===");
        sb.AppendLine($"Skybox Material: {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "NULL")}");
        sb.AppendLine($"Ambient Mode: {RenderSettings.ambientMode}");
        sb.AppendLine($"Ambient Sky Color: {RenderSettings.ambientSkyColor}");
        sb.AppendLine($"Ambient Ground Color: {RenderSettings.ambientGroundColor}");
        sb.AppendLine($"Ambient Light: {RenderSettings.ambientLight}");
        sb.AppendLine($"Ambient Intensity: {RenderSettings.ambientIntensity}");

        // Materials
        string[] matPaths = {
            "Assets/LowPolyMaterials/Ground_Grass.mat",
            "Assets/LowPolyMaterials/Leaf_B.mat"
        };

        sb.AppendLine("\n=== MATERIALS ===");
        foreach (var path in matPaths)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                sb.AppendLine($"\nMaterial: {mat.name}");
                sb.AppendLine($"  Shader: {mat.shader.name}");
                if (mat.HasProperty("_BaseColor"))
                    sb.AppendLine($"  BaseColor: {mat.GetColor("_BaseColor")}");
                if (mat.HasProperty("_Color"))
                    sb.AppendLine($"  Color: {mat.GetColor("_Color")}");
                if (mat.HasProperty("_Smoothness"))
                    sb.AppendLine($"  Smoothness: {mat.GetFloat("_Smoothness")}");
            }
        }

        // All materials in LowPolyMaterials folder
        sb.AppendLine("\n=== ALL LOWPOLY MATERIALS ===");
        var guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/LowPolyMaterials" });
        foreach (var guid in guids)
        {
            var matPath = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat != null)
            {
                string color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor").ToString() : "N/A";
                sb.AppendLine($"  {mat.name}: {color}");
            }
        }

        // Camera far clip
        sb.AppendLine("\n=== CAMERA ===");
        var cam = Camera.main;
        if (cam != null)
        {
            sb.AppendLine($"Far Clip Plane: {cam.farClipPlane}");
            sb.AppendLine($"Near Clip Plane: {cam.nearClipPlane}");
            sb.AppendLine($"Clear Flags: {cam.clearFlags}");
            sb.AppendLine($"Background Color: {cam.backgroundColor}");
        }

        return sb.ToString();
    }
}
