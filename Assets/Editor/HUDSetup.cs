using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDSetup
{
    public static void Execute()
    {
        // Panel Settings asset bul
        var panelSettings = AssetDatabase.FindAssets("t:PanelSettings");

        // UIDocument objesi
        GameObject hudObj = new GameObject("HUD");
        UIDocument doc = hudObj.AddComponent<UIDocument>();

        // UXML yükle
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/KamikazeGame/UI/HUD.uxml");

        if (uxml != null)
            doc.visualTreeAsset = uxml;
        else
            Debug.LogWarning("HUD.uxml bulunamadi!");

        // Panel Settings ata (varsa)
        if (panelSettings.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(panelSettings[0]);
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            doc.panelSettings = ps;
        }
        else
        {
            Debug.LogWarning("PanelSettings bulunamadi. Window > UI Toolkit > Panel Settings ile olustur.");
        }

        // HUDController ekle
        hudObj.AddComponent<HUDController>();

        Debug.Log("HUD olusturuldu!");
    }
}
