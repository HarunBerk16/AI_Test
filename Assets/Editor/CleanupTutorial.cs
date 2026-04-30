using UnityEditor;
using UnityEngine;
using System.IO;

public class CleanupTutorial
{
    public static void Execute()
    {
        string path = "Assets/TutorialInfo";
        if (AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
            Debug.Log("TutorialInfo klasoru silindi.");
        }
        else
        {
            Debug.Log("TutorialInfo bulunamadi, temiz.");
        }
    }
}
