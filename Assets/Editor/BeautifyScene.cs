using UnityEngine;
using UnityEditor;
using System.IO;

public class BeautifyScene
{
    public static void Execute()
    {
        // 1. Skybox
        var skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/BOXOPHOBIC/Skybox Cubemap Extended/Demo/Materials/Skybox Cubemap Extended Day.mat");
        if (skyboxMat != null)
        {
            RenderSettings.skybox = skyboxMat;
            DynamicGI.UpdateEnvironment();
            Debug.Log("[Beautify] Skybox ayarlandı.");
        }
        else Debug.LogWarning("[Beautify] Skybox materyali bulunamadı!");

        // 2. Ground'a çimen materyali uygula
        var groundGo = GameObject.Find("Ground");
        if (groundGo != null)
        {
            var grassMat = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Almgp_grassyTerrain/Models/MESHes/Materials/diffuse_x1_y1.mat");
            if (grassMat != null)
            {
                var mr = groundGo.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = grassMat;
                Debug.Log("[Beautify] Ground materyali uygulandı.");
            }
        }

        // 3. Doğa objelerini yerleştir
        string[] treePaths = new string[]
        {
            "Assets/SimpleNaturePack/Prefabs/Tree_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_03.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_04.prefab",
            "Assets/SimpleNaturePack/Prefabs/Tree_05.prefab",
        };
        string[] rockPaths = new string[]
        {
            "Assets/SimpleNaturePack/Prefabs/Rock_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Rock_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Rock_03.prefab",
        };
        string[] bushPaths = new string[]
        {
            "Assets/SimpleNaturePack/Prefabs/Bush_01.prefab",
            "Assets/SimpleNaturePack/Prefabs/Bush_02.prefab",
            "Assets/SimpleNaturePack/Prefabs/Bush_03.prefab",
        };

        // Container obje
        var container = new GameObject("Environment");
        Undo.RegisterCreatedObjectUndo(container, "Create Environment");

        var rng = new System.Random(42);

        // Ağaç pozisyonları - geniş alana dağıt, uçağın yolu ortada (0,0) olduğundan kenarları doldur
        Vector2[] treeZones = new Vector2[]
        {
            new Vector2(-80, -80), new Vector2(80, -80), new Vector2(-80, 80), new Vector2(80, 80),
            new Vector2(-120, 0), new Vector2(120, 0), new Vector2(0, -120), new Vector2(0, 120),
            new Vector2(-150, -50), new Vector2(150, 50), new Vector2(-50, 150), new Vector2(50, -150),
            new Vector2(-100, 100), new Vector2(100, -100), new Vector2(-200, -30), new Vector2(200, 30),
            new Vector2(30, 200), new Vector2(-30, -200), new Vector2(170, 170), new Vector2(-170, -170),
            new Vector2(-60, -160), new Vector2(60, 160), new Vector2(-180, 90), new Vector2(180, -90),
        };

        foreach (var zone in treeZones)
        {
            float ox = (float)(rng.NextDouble() * 20 - 10);
            float oz = (float)(rng.NextDouble() * 20 - 10);
            var pos = new Vector3(zone.x + ox, 0f, zone.y + oz);

            string treePath = treePaths[rng.Next(treePaths.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(treePath);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, (float)(rng.NextDouble() * 360), 0);
            float s = (float)(0.8 + rng.NextDouble() * 0.6);
            go.transform.localScale = Vector3.one * s;
            go.transform.SetParent(container.transform);
        }

        // Kaya pozisyonları
        Vector2[] rockZones = new Vector2[]
        {
            new Vector2(-40, -90), new Vector2(40, 90), new Vector2(-90, 40), new Vector2(90, -40),
            new Vector2(-130, -130), new Vector2(130, 130), new Vector2(60, -60), new Vector2(-60, 60),
            new Vector2(110, 50), new Vector2(-110, -50),
        };

        foreach (var zone in rockZones)
        {
            float ox = (float)(rng.NextDouble() * 15 - 7.5);
            float oz = (float)(rng.NextDouble() * 15 - 7.5);
            var pos = new Vector3(zone.x + ox, 0f, zone.y + oz);

            string rockPath = rockPaths[rng.Next(rockPaths.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rockPath);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, (float)(rng.NextDouble() * 360), 0);
            float s = (float)(0.6 + rng.NextDouble() * 1.0);
            go.transform.localScale = Vector3.one * s;
            go.transform.SetParent(container.transform);
        }

        // Çalı pozisyonları
        Vector2[] bushZones = new Vector2[]
        {
            new Vector2(-35, 50), new Vector2(35, -50), new Vector2(-70, -35), new Vector2(70, 35),
            new Vector2(-50, -140), new Vector2(50, 140), new Vector2(-140, 50), new Vector2(140, -50),
            new Vector2(20, -80), new Vector2(-20, 80), new Vector2(-100, -10), new Vector2(100, 10),
        };

        foreach (var zone in bushZones)
        {
            float ox = (float)(rng.NextDouble() * 12 - 6);
            float oz = (float)(rng.NextDouble() * 12 - 6);
            var pos = new Vector3(zone.x + ox, 0f, zone.y + oz);

            string bushPath = bushPaths[rng.Next(bushPaths.Length)];
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bushPath);
            if (prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0, (float)(rng.NextDouble() * 360), 0);
            float s = (float)(0.7 + rng.NextDouble() * 0.8);
            go.transform.localScale = Vector3.one * s;
            go.transform.SetParent(container.transform);
        }

        // 4. Directional Light'ı güneş gibi ayarla
        var lights = Object.FindObjectsOfType<Light>();
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.color = new Color(1f, 0.95f, 0.8f);
                l.intensity = 1.2f;
                l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                break;
            }
        }

        EditorUtility.SetDirty(RenderSettings.skybox);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Beautify] Sahne güzelleştirme tamamlandı!");
    }
}
