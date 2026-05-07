using UnityEngine;
using UnityEditor;

public class FixMaterialsURP
{
    public static void Execute()
    {
        var urpLit = Shader.Find("Universal Render Pipeline/Lit");
        var urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpLit == null)
        {
            Debug.LogError("[FixMaterials] URP/Lit shader bulunamadı!");
            return;
        }

        string[] searchFolders = new[]
        {
            "Assets/SimpleNaturePack",
            "Assets/Almgp_grassyTerrain",
        };

        int converted = 0;
        foreach (var folder in searchFolders)
        {
            var guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                var shaderName = mat.shader != null ? mat.shader.name : "";
                if (shaderName == "Universal Render Pipeline/Lit" ||
                    shaderName == "Universal Render Pipeline/Unlit") continue;

                // Mevcut ana dokuyu kaydet
                Texture mainTex = null;
                if (mat.HasProperty("_MainTex")) mainTex = mat.GetTexture("_MainTex");
                if (mainTex == null && mat.HasProperty("_BaseColorMap")) mainTex = mat.GetTexture("_BaseColorMap");

                Color baseColor = Color.white;
                if (mat.HasProperty("_Color")) baseColor = mat.GetColor("_Color");
                else if (mat.HasProperty("_BaseColor")) baseColor = mat.GetColor("_BaseColor");

                mat.shader = urpLit;

                // Dokuyu URP property ismine aktar
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                    mat.SetTexture("_MainTex", mainTex);
                }
                mat.SetColor("_BaseColor", baseColor);

                // Matte görünüm için metallic/smoothness sıfırla
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.3f);

                EditorUtility.SetDirty(mat);
                converted++;
                Debug.Log($"[FixMaterials] Dönüştürüldü: {mat.name} ({shaderName} → URP/Lit)");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[FixMaterials] Toplam {converted} materyal URP'ye dönüştürüldü.");
    }
}
