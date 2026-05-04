using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixExplosion
{
    public static void Execute()
    {
        var em = Object.FindFirstObjectByType<ExplosionManager>();
        if (em == null) { Debug.LogError("[FixExplosion] ExplosionManager bulunamadı!"); return; }

        var so = new SerializedObject(em);
        so.FindProperty("explosionVFXPrefab").objectReferenceValue = null;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(em);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixExplosion] explosionVFXPrefab temizlendi — prosedürel sistem aktif.");
    }
}
