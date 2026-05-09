using UnityEngine;
using UnityEditor;

public class RestoreLighting
{
    public static string Execute()
    {
        // Restore ambient to Skybox-driven with good trilight values
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.55f, 0.70f, 0.90f);
        RenderSettings.ambientEquatorColor = new Color(0.40f, 0.55f, 0.30f);
        RenderSettings.ambientGroundColor  = new Color(0.12f, 0.10f, 0.07f);
        RenderSettings.ambientIntensity = 1.0f;

        // Restore fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 600f;
        RenderSettings.fogEndDistance = 1800f;
        RenderSettings.fogColor = new Color(0.75f, 0.88f, 1.0f, 1f);

        // Restore sun light
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.color = new Color(1.0f, 0.96f, 0.88f);
                l.intensity = 1.3f;
                l.colorTemperature = 6500f;
                l.useColorTemperature = true;
                l.transform.rotation = Quaternion.Euler(50f, 330f, 0f);
                EditorUtility.SetDirty(l);
            }
        }

        DynamicGI.UpdateEnvironment();

        // Reset scene view to shaded
        var sv = SceneView.lastActiveSceneView;
        if (sv != null)
        {
            sv.cameraMode = SceneView.GetBuiltinCameraMode(DrawCameraMode.Textured);
            sv.sceneLighting = true;
            sv.Repaint();
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        return "Lighting restored and scene saved.";
    }
}
