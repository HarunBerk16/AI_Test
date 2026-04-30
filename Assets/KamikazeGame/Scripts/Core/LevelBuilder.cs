using UnityEngine;

/// <summary>
/// Sahne başladığında aktif levele göre hedefi inşa eder.
/// Level 1: 1 bina, 3 kat
/// Level 2: 2 bina, toplam 8 kat, daha uzak
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    void Awake()
    {
        // Mevcut test binasını sil, levele göre yeniden kur
        var existing = FindObjectsByType<TargetBuilding>(FindObjectsSortMode.None);
        foreach (var b in existing)
            Destroy(b.gameObject);

        int level = GameData.CurrentLevel;

        if (level == 1)
            BuildLevel1();
        else if (level == 2)
            BuildLevel2();
        else
            BuildLevel1(); // fallback
    }

    void BuildLevel1()
    {
        // 1 bina, 3 kat, z=30
        SpawnBuilding(new Vector3(0, 0, 30), 3, 2.5f, 5f, "Hedef Bina", 100);
    }

    void BuildLevel2()
    {
        // Sol bina: 5 kat, z=50
        SpawnBuilding(new Vector3(-8, 0, 50), 5, 2.5f, 4f, "Sol Bina", 150);
        // Sağ bina: 4 kat, z=55
        SpawnBuilding(new Vector3(8, 0, 55), 4, 3f, 5f, "Sag Bina", 100);
        // Orta kule: 7 kat, z=60
        SpawnBuilding(new Vector3(0, 0, 60), 7, 2.5f, 3f, "Ana Kule", 250);
    }

    void SpawnBuilding(Vector3 pos, int floors, float floorHeight, float width, string buildingName, int reward)
    {
        GameObject building = new GameObject(buildingName);
        building.transform.position = pos;

        var tb = building.AddComponent<TargetBuilding>();
        tb.buildingName = buildingName;
        tb.coinRewardFull = reward;

        Color[] palette = {
            new Color(0.55f, 0.55f, 0.65f),
            new Color(0.45f, 0.45f, 0.6f),
            new Color(0.4f, 0.4f, 0.55f),
            new Color(0.35f, 0.35f, 0.5f),
            new Color(0.3f, 0.3f, 0.45f),
            new Color(0.25f, 0.25f, 0.4f),
            new Color(0.2f, 0.2f, 0.35f),
        };

        for (int i = 0; i < floors; i++)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"Kat_{i + 1}";
            floor.transform.SetParent(building.transform);
            floor.transform.localPosition = new Vector3(0, i * floorHeight + floorHeight * 0.5f, 0);
            floor.transform.localScale = new Vector3(width, floorHeight, width);

            var r = floor.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = palette[Mathf.Clamp(i, 0, palette.Length - 1)];
            r.material = mat;

            floor.AddComponent<BuildingSection>();
        }
    }
}
