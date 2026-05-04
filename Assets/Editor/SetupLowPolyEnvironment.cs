using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SetupLowPolyEnvironment
{
    // ─── Canlı low-poly renkler ───────────────────────────────────────
    static readonly Color ColGrass      = new Color(0.18f, 0.78f, 0.22f);   // parlak çimen yeşili
    static readonly Color ColRoad       = new Color(0.72f, 0.65f, 0.46f);   // kum/toprak bej
    static readonly Color ColLeafA      = new Color(0.10f, 0.82f, 0.20f);   // canlı yaprak
    static readonly Color ColLeafB      = new Color(0.05f, 0.68f, 0.38f);   // koyu yaprak
    static readonly Color ColLeafC      = new Color(0.30f, 0.90f, 0.10f);   // limon yeşil
    static readonly Color ColTrunk      = new Color(0.42f, 0.26f, 0.10f);   // kahverengi gövde
    static readonly Color ColRock       = new Color(0.62f, 0.60f, 0.58f);   // gri kaya
    static readonly Color ColRockAlt    = new Color(0.52f, 0.65f, 0.55f);   // yeşilimsi kaya
    static readonly Color ColBush       = new Color(0.10f, 0.85f, 0.28f);   // canlı çalı

    public static void Execute()
    {
        // 0. Materyal klasörünü önce oluştur
        if (!AssetDatabase.IsValidFolder("Assets/LowPolyMaterials"))
            AssetDatabase.CreateFolder("Assets", "LowPolyMaterials");

        // 1. Eski Environment ve RoadOverlay sil
        var oldEnv = GameObject.Find("Environment");
        if (oldEnv != null) { Undo.DestroyObjectImmediate(oldEnv); Debug.Log("[LowPoly] Eski Environment silindi."); }
        var oldRoad = GameObject.Find("RoadOverlay");
        if (oldRoad != null) Undo.DestroyObjectImmediate(oldRoad);

        // 2. Zemini yeniden tasarla
        SetupGround();

        // 3. Yol koridoru overlay
        SetupRoadOverlay();

        // 4. Çevre objelerini yerleştir
        SetupNature();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[LowPoly] Kurulum tamamlandı!");
    }

    // ─── Zemin ────────────────────────────────────────────────────────
    static void SetupGround()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null) return;

        var mat = MakeMat("Ground_Grass", ColGrass);

        var mr = ground.GetComponent<MeshRenderer>();
        if (mr != null) mr.sharedMaterial = mat;

        // Boyutu yol boyunca uzat
        ground.transform.localScale = new Vector3(80f, 1f, 120f); // 800x1200 birim
        ground.transform.position   = new Vector3(0f, 0f, 250f);
        Debug.Log("[LowPoly] Zemin güncellendi.");
    }

    // ─── Yol overlay ──────────────────────────────────────────────────
    static void SetupRoadOverlay()
    {
        var roadMat = MakeMat("Road", ColRoad);

        var road = GameObject.CreatePrimitive(PrimitiveType.Plane);
        road.name = "RoadOverlay";
        road.transform.position   = new Vector3(0f, 0.01f, 250f);
        road.transform.localScale = new Vector3(5f, 1f, 120f);  // 50 birim geniş, 1200 birim uzun
        road.GetComponent<MeshRenderer>().sharedMaterial = roadMat;
        Object.DestroyImmediate(road.GetComponent<Collider>());

        Undo.RegisterCreatedObjectUndo(road, "Create Road");
        Debug.Log("[LowPoly] Yol koridoru oluşturuldu.");
    }

    // ─── Doğa objelerini yerleştir ────────────────────────────────────
    static void SetupNature()
    {
        // Ağaç yaprak materyalleri (3 renk varyantı)
        var matLeafA = MakeMat("Leaf_A", ColLeafA);
        var matLeafB = MakeMat("Leaf_B", ColLeafB);
        var matLeafC = MakeMat("Leaf_C", ColLeafC);
        var matTrunk = MakeMat("Trunk",  ColTrunk);
        var matRock   = MakeMat("Rock",   ColRock);
        var matRockAlt= MakeMat("RockAlt",ColRockAlt);
        var matBush   = MakeMat("Bush",   ColBush);

        string[] treePrefabs = new[]
        {
            "Assets/SimpleNaturePack/Prefabs/Tree_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_03.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_04.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_05.prefab",
        };
        string[] rockPrefabs = new[]
        {
            "Assets/SimpleNaturePack/Prefabs/Rock_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Rock_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Rock_03.prefab",
            "Assets/SimpleNaturePack/Prefabs/Rock_04.prefab",
            "Assets/SimpleNaturePack/Prefabs/Rock_05.prefab",
        };
        string[] bushPrefabs = new[]
        {
            "Assets/SimpleNaturePack/Prefabs/Bush_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Bush_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Bush_03.prefab",
        };

        var container = new GameObject("Environment");
        Undo.RegisterCreatedObjectUndo(container, "Create LowPoly Environment");

        var rng = new System.Random(1337);

        // ── Koridor kenarlarına ağaç (her iki yan, Z: -40 → 600) ─────
        float zStart = -40f;
        float zEnd   = 600f;
        float corridorHalf = 28f;   // yol yarı genişliği (50/2 ≈ 25 + biraz boşluk)
        float sideMin = 35f;        // koridor kenarından minimum uzaklık
        float sideMax = 200f;       // koridor kenarından maksimum uzaklık

        // Ağaçlar: sık ve çift taraflı
        int treeCount = 200;
        for (int i = 0; i < treeCount; i++)
        {
            float z    = (float)(zStart + rng.NextDouble() * (zEnd - zStart));
            float side = rng.NextDouble() > 0.5 ? 1f : -1f;
            float x    = side * (sideMin + (float)(rng.NextDouble() * (sideMax - sideMin)));
            float yaw  = (float)(rng.NextDouble() * 360f);
            float sc   = (float)(0.7 + rng.NextDouble() * 0.9f);

            var path   = treePrefabs[rng.Next(treePrefabs.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(container.transform);
            go.transform.position    = new Vector3(x, 0f, z);
            go.transform.rotation    = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale  = Vector3.one * sc;

            // Canlı renk: yaprak materyalini rastgele ata
            ApplyLowPolyMaterials(go, matLeafA, matLeafB, matLeafC, matTrunk, rng);

            AddObstaclePhysics(go);
        }

        // Kayalar: her iki yan + koridor içinde küçük kayalar
        int rockSideCount = 120;
        for (int i = 0; i < rockSideCount; i++)
        {
            float z    = (float)(zStart + rng.NextDouble() * (zEnd - zStart));
            float side = rng.NextDouble() > 0.5 ? 1f : -1f;
            float x    = side * (sideMin + (float)(rng.NextDouble() * (sideMax - sideMin)));
            float yaw  = (float)(rng.NextDouble() * 360f);
            float sc   = (float)(0.4 + rng.NextDouble() * 1.2f);

            var path   = rockPrefabs[rng.Next(rockPrefabs.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(container.transform);
            go.transform.position   = new Vector3(x, 0f, z);
            go.transform.rotation   = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = Vector3.one * sc;

            ApplyColorToAllRenderers(go, rng.NextDouble() > 0.5 ? matRock : matRockAlt);
            AddObstaclePhysics(go);
        }

        // Koridor içine küçük kayalar ve çalılar (yer hizasında, engel)
        int inRoadRocks = 30;
        for (int i = 0; i < inRoadRocks; i++)
        {
            float z    = (float)(zStart + 30f + rng.NextDouble() * (zEnd - zStart - 60f));
            float x    = (float)((rng.NextDouble() * 2 - 1) * (corridorHalf - 5f));
            float yaw  = (float)(rng.NextDouble() * 360f);
            float sc   = (float)(0.2 + rng.NextDouble() * 0.5f); // küçük

            var path   = rockPrefabs[rng.Next(rockPrefabs.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(container.transform);
            go.transform.position   = new Vector3(x, 0f, z);
            go.transform.rotation   = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = Vector3.one * sc;

            ApplyColorToAllRenderers(go, matRock);
            AddObstaclePhysics(go);
        }

        // Koridor kenarına çalılar (koridor hattında, hem içi hem dışı yakın)
        int bushCount = 80;
        for (int i = 0; i < bushCount; i++)
        {
            float z    = (float)(zStart + rng.NextDouble() * (zEnd - zStart));
            float side = rng.NextDouble() > 0.5 ? 1f : -1f;
            // Bir kısmı koridor içinde, bir kısmı hemen dışında
            float dist = rng.NextDouble() > 0.4
                ? (float)(sideMin + rng.NextDouble() * 80f)
                : (float)(rng.NextDouble() * (corridorHalf - 5f));
            float x    = side * dist;
            float yaw  = (float)(rng.NextDouble() * 360f);
            float sc   = (float)(0.4 + rng.NextDouble() * 0.7f);

            var path   = bushPrefabs[rng.Next(bushPrefabs.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(container.transform);
            go.transform.position   = new Vector3(x, 0f, z);
            go.transform.rotation   = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = Vector3.one * sc;

            ApplyColorToAllRenderers(go, matBush);
            AddObstaclePhysics(go);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[LowPoly] {treeCount} ağaç, {rockSideCount} kaya, {inRoadRocks} yol içi kaya, {bushCount} çalı yerleştirildi.");
    }

    // ─── Yardımcı: Materyal oluştur ───────────────────────────────────
    static Material MakeMat(string name, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0f);
        mat.SetFloat("_Metallic", 0f);
        AssetDatabase.CreateAsset(mat, $"Assets/LowPolyMaterials/{name}.mat");
        return mat;
    }

    // ─── Ağaca yaprak + gövde materyali uygula ────────────────────────
    static void ApplyLowPolyMaterials(GameObject go, Material leafA, Material leafB, Material leafC, Material trunk, System.Random rng)
    {
        var leafMats = new[] { leafA, leafB, leafC };
        var leaf = leafMats[rng.Next(leafMats.Length)];

        foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
        {
            string n = mr.gameObject.name.ToLower();
            bool isTrunk = n.Contains("trunk") || n.Contains("stem") || n.Contains("bark") || n.Contains("branch");
            var mats = new Material[mr.sharedMaterials.Length];
            for (int m = 0; m < mats.Length; m++)
                mats[m] = isTrunk ? trunk : leaf;
            mr.sharedMaterials = mats;
        }
    }

    // ─── Tüm renderer'lara aynı materyali uygula ──────────────────────
    static void ApplyColorToAllRenderers(GameObject go, Material mat)
    {
        foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
        {
            var mats = new Material[mr.sharedMaterials.Length];
            for (int m = 0; m < mats.Length; m++) mats[m] = mat;
            mr.sharedMaterials = mats;
        }
    }

    // ─── Rigidbody + Collider + EnvironmentObstacle ekle ─────────────
    static void AddObstaclePhysics(GameObject go)
    {
        // Root Rigidbody
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = true;
        rb.mass        = 50f;
        rb.linearDamping  = 0.5f;
        rb.angularDamping = 0.5f;

        // EnvironmentObstacle marker
        if (go.GetComponent<EnvironmentObstacle>() == null)
            go.AddComponent<EnvironmentObstacle>();

        // Eğer root'ta collider yoksa basit bir box ekle
        bool hasCollider = go.GetComponentInChildren<Collider>() != null;
        if (!hasCollider)
            go.AddComponent<BoxCollider>();
    }
}
