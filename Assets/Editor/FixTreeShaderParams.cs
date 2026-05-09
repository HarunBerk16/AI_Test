using UnityEngine;
using UnityEditor;

public class FixTreeShaderParams
{
    public static string Execute()
    {
        // TreeMat_A: yuvarlak yapraklı (Tree_01, Tree_04) — belirgin gövde
        AdjustMat("Assets/LowPolyMaterials/TreeMat_A.mat",
            trunkColor: new Color(0.30f, 0.16f, 0.06f),
            leafColor:  new Color(0.12f, 0.50f, 0.08f),
            trunkH: 4.5f, blendZ: 3.0f);

        // TreeMat_B: çam/koni (Tree_02, Tree_05) — yaprak hemen tabandan başlar
        AdjustMat("Assets/LowPolyMaterials/TreeMat_B.mat",
            trunkColor: new Color(0.28f, 0.14f, 0.05f),
            leafColor:  new Color(0.10f, 0.42f, 0.10f),
            trunkH: 0.6f, blendZ: 1.5f);

        // TreeMat_C: karışık (Tree_03) — orta gövde
        AdjustMat("Assets/LowPolyMaterials/TreeMat_C.mat",
            trunkColor: new Color(0.30f, 0.16f, 0.06f),
            leafColor:  new Color(0.26f, 0.62f, 0.06f),
            trunkH: 3.0f, blendZ: 2.5f);

        AssetDatabase.SaveAssets();
        return "Tree shader params updated.";
    }

    static void AdjustMat(string path, Color trunkColor, Color leafColor, float trunkH, float blendZ)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { Debug.LogWarning($"Not found: {path}"); return; }
        mat.SetColor("_TrunkColor",  trunkColor);
        mat.SetColor("_LeafColor",   leafColor);
        mat.SetFloat("_TrunkHeight", trunkH);
        mat.SetFloat("_BlendZone",   blendZ);
        EditorUtility.SetDirty(mat);
    }
}
