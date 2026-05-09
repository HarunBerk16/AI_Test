using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupDayNight
{
    public static string Execute()
    {
        // Find or create a DayNight controller GameObject
        var existing = GameObject.Find("DayNightController");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var go = new GameObject("DayNightController");
        var dnc = go.AddComponent<DayNightCycle>();

        // Link the directional light
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                dnc.sunLight = l;
                break;
            }
        }

        dnc.timeOfDay = 10f;
        dnc.dayDurationSeconds = 180f; // 3 minutes = full day cycle
        dnc.autoAdvance = true;

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();

        return $"DayNightController added. Sun linked: {(dnc.sunLight != null ? dnc.sunLight.name : "NOT FOUND")}";
    }
}
