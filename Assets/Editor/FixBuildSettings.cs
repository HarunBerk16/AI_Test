using UnityEditor;
using UnityEngine;

public class FixBuildSettings
{
    public static void Execute()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
            new EditorBuildSettingsScene("Assets/KamikazeGame/Scenes/UpgradeScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build Settings guncellendi: SampleScene (0), UpgradeScene (1)");
    }
}
