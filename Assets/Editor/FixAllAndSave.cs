using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class FixAllAndSave
{
    public static void Execute()
    {
        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(
            "Assets/KamikazeGame/UI/GamePanelSettings.asset");

        if (ps == null)
        {
            Debug.LogError("GamePanelSettings.asset bulunamadi!");
            return;
        }

        // --- SampleScene: HUD PanelSettings ---
        GameObject hudObj = GameObject.Find("HUD");
        if (hudObj != null)
        {
            UIDocument doc = hudObj.GetComponent<UIDocument>();
            if (doc != null)
            {
                doc.panelSettings = ps;
                EditorUtility.SetDirty(hudObj);
                Debug.Log("HUD PanelSettings atandi.");
            }
        }

        // --- SampleScene: Rigidbody ---
        GameObject plane = GameObject.Find("Plane");
        if (plane != null)
        {
            Rigidbody rb = plane.GetComponent<Rigidbody>();
            if (rb == null) rb = plane.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            EditorUtility.SetDirty(plane);
            Debug.Log("Rigidbody kontrol edildi.");
        }

        // SampleScene kaydet
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("SampleScene kaydedildi.");

        // --- UpgradeScene: PanelSettings ---
        var upgradeScene = EditorSceneManager.OpenScene(
            "Assets/KamikazeGame/Scenes/UpgradeScene.unity",
            OpenSceneMode.Additive);

        GameObject upgradeMgr = GameObject.Find("UpgradeManager");
        if (upgradeMgr != null)
        {
            UIDocument doc = upgradeMgr.GetComponent<UIDocument>();
            if (doc != null)
            {
                doc.panelSettings = ps;
                EditorUtility.SetDirty(upgradeMgr);
                Debug.Log("UpgradeScene PanelSettings atandi.");
            }
        }

        EditorSceneManager.SaveScene(upgradeScene);
        EditorSceneManager.CloseScene(upgradeScene, true);
        Debug.Log("UpgradeScene kaydedildi.");

        // --- MainMenu: PanelSettings ---
        var mainMenuScene = EditorSceneManager.OpenScene(
            "Assets/KamikazeGame/Scenes/MainMenu.unity",
            OpenSceneMode.Additive);

        GameObject mainMenu = GameObject.Find("MainMenu");
        if (mainMenu != null)
        {
            UIDocument doc = mainMenu.GetComponent<UIDocument>();
            if (doc != null)
            {
                doc.panelSettings = ps;
                EditorUtility.SetDirty(mainMenu);
                Debug.Log("MainMenu PanelSettings atandi.");
            }
        }

        EditorSceneManager.SaveScene(mainMenuScene);
        EditorSceneManager.CloseScene(mainMenuScene, true);
        Debug.Log("MainMenu kaydedildi.");

        Debug.Log("=== Tum sahneler duzeltildi ve kaydedildi ===");
    }
}
