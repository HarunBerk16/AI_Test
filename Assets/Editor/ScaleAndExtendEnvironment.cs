using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class ScaleAndExtendEnvironment
{
    public static void Execute()
    {
        var env = GameObject.Find("Environment");
        if (env == null) { Debug.LogError("[ScaleEnv] Environment bulunamadı!"); return; }

        var rng = new System.Random(42);
        int scaled = 0;

        foreach (Transform child in env.transform)
        {
            string n = child.name.ToLower();
            bool isTree = n.Contains("tree");
            bool isRock = n.Contains("rock");
            bool isBush = n.Contains("bush");

            if (isTree)
            {
                // Gerçekçi ağaç boyu: 3.5-5.5 ölçek (orijinal ~5.5 birim yükseklik, hedef 18-25 birim)
                float s = (float)(3.5 + rng.NextDouble() * 2.0);
                child.localScale = Vector3.one * s;
                scaled++;
            }
            else if (isRock)
            {
                float s = (float)(0.8 + rng.NextDouble() * 1.8);
                child.localScale = Vector3.one * s;
            }
            else if (isBush)
            {
                float s = (float)(0.6 + rng.NextDouble() * 1.0);
                child.localScale = Vector3.one * s;
            }
        }

        // ── Koridor ve zemin uzatma ──────────────────────────────────
        // Yeni koridor: Z=-50 → Z=750 (tüm hedefleri kapsar)
        var road = GameObject.Find("RoadOverlay");
        if (road != null)
        {
            road.transform.position   = new Vector3(0f, 0.01f, 350f);
            road.transform.localScale = new Vector3(5f, 1f, 160f); // 50 geniş, 1600 uzun
            Debug.Log("[ScaleEnv] RoadOverlay uzatıldı.");
        }

        var ground = GameObject.Find("Ground");
        if (ground != null)
        {
            ground.transform.position   = new Vector3(0f, 0f, 350f);
            ground.transform.localScale = new Vector3(100f, 1f, 160f); // 1000x1600
            Debug.Log("[ScaleEnv] Zemin uzatıldı.");
        }

        // ── Yeni ağaçlar ekle (Z=600-750 aralığına, henüz boş) ──────
        string[] treePrefabs = new[]
        {
            "Assets/SimpleNaturePack/Prefabs/Tree_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_03.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_04.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_05.prefab",
        };

        var matLeafA = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Leaf_A.mat");
        var matLeafB = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Leaf_B.mat");
        var matLeafC = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Leaf_C.mat");
        var matTrunk = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Trunk.mat");
        var matRock  = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Rock.mat");
        var matBush  = AssetDatabase.LoadAssetAtPath<Material>("Assets/LowPolyMaterials/Bush.mat");

        // Ek ağaçlar Z=600-750 arası (önceki script Z=600'de bitiriyordu)
        AddTrees(env.transform, treePrefabs, matLeafA, matLeafB, matLeafC, matTrunk, 600f, 760f, 80, rng);

        // Hedef civarına daha sık ağaç (tüm hedeflerin etrafı)
        float[] targetZs = { 250f, 380f, 520f, 680f };
        foreach (float tz in targetZs)
            AddTrees(env.transform, treePrefabs, matLeafA, matLeafB, matLeafC, matTrunk, tz - 40f, tz + 40f, 30, rng);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[ScaleEnv] {scaled} ağaç ölçeklendi, ek ağaçlar eklendi, zemin uzatıldı.");
    }

    static void AddTrees(Transform parent, string[] prefabs,
        Material leafA, Material leafB, Material leafC, Material trunk,
        float zFrom, float zTo, int count, System.Random rng)
    {
        var leafMats = new[] { leafA, leafB, leafC };
        for (int i = 0; i < count; i++)
        {
            float z    = (float)(zFrom + rng.NextDouble() * (zTo - zFrom));
            float side = rng.NextDouble() > 0.5 ? 1f : -1f;
            float x    = side * (float)(35f + rng.NextDouble() * 165f);
            float yaw  = (float)(rng.NextDouble() * 360f);
            float sc   = (float)(3.5 + rng.NextDouble() * 2.0);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[rng.Next(prefabs.Length)]);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(parent);
            go.transform.position   = new Vector3(x, 0f, z);
            go.transform.rotation   = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = Vector3.one * sc;

            var leaf = leafMats[rng.Next(leafMats.Length)];
            foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = new Material[mr.sharedMaterials.Length];
                string nm = mr.gameObject.name.ToLower();
                bool isTrunk = nm.Contains("trunk") || nm.Contains("stem") || nm.Contains("bark");
                for (int m = 0; m < mats.Length; m++) mats[m] = isTrunk ? trunk : leaf;
                mr.sharedMaterials = mats;
            }

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = true;
            rb.mass        = 50f;

            if (go.GetComponent<EnvironmentObstacle>() == null)
                go.AddComponent<EnvironmentObstacle>();
        }
    }
}
