using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ProjectSetupFinal
{
    public static void Execute()
    {
        // 1. Build Settings'i doğru sıraya al
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/KamikazeGame/Scenes/MainMenu.unity",    true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity",              true),
            new EditorBuildSettingsScene("Assets/KamikazeGame/Scenes/UpgradeScene.unity",true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build Settings: MainMenu(0) SampleScene(1) UpgradeScene(2)");

        // 2. Play Mode her zaman MainMenu'den başlasın
        var mainMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(
            "Assets/KamikazeGame/Scenes/MainMenu.unity");
        if (mainMenu != null)
        {
            EditorSceneManager.playModeStartScene = mainMenu;
            Debug.Log("Play Mode start scene: MainMenu");
        }
        else
        {
            Debug.LogError("MainMenu.unity bulunamadi!");
        }
    }
}
