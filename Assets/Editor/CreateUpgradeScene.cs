using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateUpgradeScene
{
    public static void Execute()
    {
        // Yeni sahne oluştur
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene.name = "UpgradeScene";

        // UpgradeManager objesi
        GameObject mgrObj = new GameObject("UpgradeManager");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(mgrObj, scene);

        UIDocument doc = mgrObj.AddComponent<UIDocument>();

        // UXML ata
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/KamikazeGame/UI/UpgradeScreen.uxml");
        if (uxml != null) doc.visualTreeAsset = uxml;

        // PanelSettings ata
        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(
            "Assets/KamikazeGame/UI/GamePanelSettings.asset");
        if (ps != null) doc.panelSettings = ps;

        mgrObj.AddComponent<UpgradeManager>();

        // Kamera ekle (UI için)
        GameObject camObj = new GameObject("Main Camera");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camObj, scene);
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        camObj.tag = "MainCamera";

        // Kaydet
        EditorSceneManager.SaveScene(scene, "Assets/KamikazeGame/Scenes/UpgradeScene.unity");
        EditorSceneManager.CloseScene(scene, true);

        // Build Settings'e ekle
        var scenes = EditorBuildSettings.scenes;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++) newScenes[i] = scenes[i];
        newScenes[scenes.Length] = new EditorBuildSettingsScene(
            "Assets/KamikazeGame/Scenes/UpgradeScene.unity", true);
        EditorBuildSettings.scenes = newScenes;

        Debug.Log("UpgradeScene olusturuldu ve Build Settings'e eklendi!");
    }
}
