using UnityEditor;
using UnityEngine;

public class CheckBuildScenes
{
    public static void Execute()
    {
        var scenes = EditorBuildSettings.scenes;
        Debug.Log($"Build Settings'teki sahne sayisi: {scenes.Length}");
        for (int i = 0; i < scenes.Length; i++)
            Debug.Log($"  [{i}] enabled={scenes[i].enabled} path={scenes[i].path}");
    }
}
