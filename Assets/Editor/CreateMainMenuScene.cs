using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateMainMenuScene
{
    public static void Execute()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = "MainMenu";

        // UIDocument objesi
        GameObject menuObj = new GameObject("MainMenu");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(menuObj, scene);

        UIDocument doc = menuObj.AddComponent<UIDocument>();

        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/KamikazeGame/UI/MainMenu.uxml");
        if (uxml != null) doc.visualTreeAsset = uxml;

        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(
            "Assets/KamikazeGame/UI/GamePanelSettings.asset");
        if (ps != null) doc.panelSettings = ps;

        menuObj.AddComponent<MainMenuController>();

        // Kamera
        GameObject camObj = new GameObject("Main Camera");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camObj, scene);
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.1f);
        camObj.tag = "MainCamera";

        // Kaydet
        EditorSceneManager.SaveScene(scene, "Assets/KamikazeGame/Scenes/MainMenu.unity");
        EditorSceneManager.CloseScene(scene, true);

        // Build Settings: MainMenu en başa
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/KamikazeGame/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
            new EditorBuildSettingsScene("Assets/KamikazeGame/Scenes/UpgradeScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;

        Debug.Log("MainMenu sahnesi olusturuldu! Build sirasi: MainMenu(0) > SampleScene(1) > UpgradeScene(2)");
    }
}
