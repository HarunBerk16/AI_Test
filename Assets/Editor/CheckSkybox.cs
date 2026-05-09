using UnityEngine;
using UnityEditor;

public class CheckSkybox
{
    public static string Execute()
    {
        var skybox = RenderSettings.skybox;
        if (skybox == null) return "Skybox material is NULL";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Skybox: {skybox.name}  Shader: {skybox.shader.name}");

        foreach (var prop in skybox.shader.name.Contains("Cubemap")
            ? new[] { "_Tex", "_MainTex", "_Cubemap", "_FrontTex" }
            : new[] { "_MainTex" })
        {
            if (skybox.HasProperty(prop))
            {
                var tex = skybox.GetTexture(prop);
                sb.AppendLine($"  {prop}: {(tex != null ? tex.name : "NULL/MISSING")}");
            }
        }

        // List ALL texture properties
        var serialized = new SerializedObject(skybox);
        var prop2 = serialized.GetIterator();
        while (prop2.NextVisible(true))
        {
            if (prop2.propertyType == SerializedPropertyType.ObjectReference
                && prop2.objectReferenceValue is Texture)
            {
                sb.AppendLine($"  Texture prop '{prop2.name}': {prop2.objectReferenceValue.name}");
            }
        }

        // Check all float/color props
        sb.AppendLine($"\nAll shader keywords: {string.Join(", ", skybox.shaderKeywords)}");
        if (skybox.HasProperty("_Exposure")) sb.AppendLine($"Exposure: {skybox.GetFloat("_Exposure")}");

        return sb.ToString();
    }
}
