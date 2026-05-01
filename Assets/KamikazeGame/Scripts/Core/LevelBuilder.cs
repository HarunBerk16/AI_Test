using UnityEngine;

/// <summary>
/// Menu ve Flying fazı başlarken seviyeyi yeniden inşa eder.
/// Level 1: 7 katlı 1 bina — kısmi hasar mümkün (2.5m başlangıç yarıçapıyla).
/// Level 2: 3 bina, daha uzak ve daha yüksek.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    void Awake() => BuildLevel();

    void OnEnable()  => GameStateManager.OnPhaseChanged += HandlePhaseChange;
    void OnDisable() => GameStateManager.OnPhaseChanged -= HandlePhaseChange;

    void HandlePhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Flying || phase == GamePhase.Menu)
            BuildLevel();
    }

    void BuildLevel()
    {
        var existing = FindObjectsByType<TargetBuilding>(FindObjectsSortMode.None);
        foreach (var b in existing)
            Destroy(b.gameObject);

        switch (GameData.CurrentLevel)
        {
            case 2:  BuildLevel2(); break;
            default: BuildLevel1(); break;
        }
    }

    void BuildLevel1()
    {
        SpawnBuilding(new Vector3(0, 0, 40), 7, 3f, 5f, "Hedef Bina", 200);
    }

    void BuildLevel2()
    {
        SpawnBuilding(new Vector3(-10, 0, 60), 6,  3f, 4f, "Sol Bina",  150);
        SpawnBuilding(new Vector3( 10, 0, 70), 8,  3f, 5f, "Sag Bina",  200);
        SpawnBuilding(new Vector3(  0, 0, 80), 12, 3f, 4f, "Ana Kule",  350);
    }

    void SpawnBuilding(Vector3 pos, int floors, float floorHeight, float width, string buildingName, int reward)
    {
        GameObject building = new GameObject(buildingName);
        building.transform.position = pos;

        var tb = building.AddComponent<TargetBuilding>();
        tb.buildingName   = buildingName;
        tb.coinRewardFull = reward;

        Color[] palette = {
            new Color(0.55f, 0.55f, 0.65f),
            new Color(0.45f, 0.45f, 0.60f),
            new Color(0.40f, 0.40f, 0.55f),
            new Color(0.35f, 0.35f, 0.50f),
            new Color(0.30f, 0.30f, 0.45f),
            new Color(0.25f, 0.25f, 0.40f),
            new Color(0.20f, 0.20f, 0.35f),
        };

        for (int i = 0; i < floors; i++)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"Kat_{i + 1}";
            floor.transform.SetParent(building.transform);
            floor.transform.localPosition = new Vector3(0, i * floorHeight + floorHeight * 0.5f, 0);
            floor.transform.localScale    = new Vector3(width, floorHeight, width);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = palette[Mathf.Clamp(i, 0, palette.Length - 1)];
            floor.GetComponent<Renderer>().material = mat;

            floor.AddComponent<BuildingSection>();
        }
    }
}
