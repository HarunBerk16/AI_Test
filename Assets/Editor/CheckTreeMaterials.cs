using UnityEngine;
using UnityEditor;
using System.Text;

public class CheckTreeMaterials
{
    public static string Execute()
    {
        var sb = new StringBuilder();
        string[] treePaths = {
            "Environment/Tree_01/Tree_01_LOD0",
            "Environment/Tree_02/Tree_02_LOD0",
            "Environment/Tree_03/Tree_03_LOD0",
            "Environment/Tree_04/Tree_04_LOD0",
            "Environment/Tree_05/Tree_05_LOD0",
        };

        foreach (var path in treePaths)
        {
            var go = GameObject.Find(path);
            if (go == null) { sb.AppendLine($"{path}: NOT FOUND"); continue; }

            var mr = go.GetComponent<MeshRenderer>();
            var mf = go.GetComponent<MeshFilter>();
            if (mr == null) { sb.AppendLine($"{path}: No MeshRenderer"); continue; }

            sb.AppendLine($"\n{go.name}:");
            sb.AppendLine($"  Sub-mesh count (materials): {mr.sharedMaterials.Length}");
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
            {
                var m = mr.sharedMaterials[i];
                sb.AppendLine($"  [{i}] {(m != null ? m.name : "NULL")}");
            }
            if (mf != null && mf.sharedMesh != null)
                sb.AppendLine($"  Mesh subMeshCount: {mf.sharedMesh.subMeshCount}");
        }
        return sb.ToString();
    }
}
