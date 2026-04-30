using UnityEditor;
using UnityEngine;

public class AddGroundTag
{
    public static void Execute()
    {
        // TagManager üzerinden "Ground" tag'ini ekle
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));

        SerializedProperty tags = tagManager.FindProperty("tags");

        // Zaten var mı kontrol et
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == "Ground")
            {
                Debug.Log("Ground tag zaten mevcut.");
                AssignGroundTag();
                return;
            }
        }

        // Ekle
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Ground";
        tagManager.ApplyModifiedProperties();
        Debug.Log("Ground tag eklendi.");

        AssignGroundTag();
    }

    static void AssignGroundTag()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            ground.tag = "Ground";
            EditorUtility.SetDirty(ground);
            Debug.Log("Ground objesi tag'landi.");
        }
    }
}
