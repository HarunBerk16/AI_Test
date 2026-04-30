using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FixHUD
{
    public static void Execute()
    {
        GameObject hudObj = GameObject.Find("HUD");
        if (hudObj == null) { Debug.LogError("HUD objesi bulunamadi!"); return; }

        UIDocument doc = hudObj.GetComponent<UIDocument>();
        if (doc == null) { Debug.LogError("UIDocument bulunamadi!"); return; }

        // PanelSettings ata
        PanelSettings ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(
            "Assets/KamikazeGame/UI/GamePanelSettings.asset");

        if (ps != null)
        {
            doc.panelSettings = ps;
            EditorUtility.SetDirty(doc);
            Debug.Log("PanelSettings atandi!");
        }
        else
        {
            Debug.LogError("GamePanelSettings.asset bulunamadi!");
        }
    }
}
