using UnityEngine;
using System;

/// <summary>
/// 4 level, birbirine bağlı yapı kompleksleri.
/// Her level için tek bir TargetBuilding parent, tüm bloklar child.
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
        foreach (var b in existing) Destroy(b.gameObject);

        switch (GameData.CurrentLevel)
        {
            case 2:  BuildLevel2(); break;
            case 3:  BuildLevel3(); break;
            case 4:  BuildLevel4(); break;
            default: BuildLevel1(); break;
        }
    }

    // ── Renk yardımcıları ──────────────────────────────────────
    static Color Stone   (float t) => Color.Lerp(new Color(0.56f, 0.55f, 0.62f), new Color(0.20f, 0.18f, 0.26f), t);
    static Color Concrete(float t) => Color.Lerp(new Color(0.54f, 0.52f, 0.48f), new Color(0.22f, 0.20f, 0.18f), t);
    static Color Metal   (float t) => Color.Lerp(new Color(0.42f, 0.48f, 0.58f), new Color(0.20f, 0.24f, 0.34f), t);
    static Color Rust    (float t) => Color.Lerp(new Color(0.58f, 0.40f, 0.26f), new Color(0.28f, 0.16f, 0.10f), t);

    // ── LEVEL 1: Karakol ──────────────────────────────────────
    // Sol ve sağ gözetleme kulesi, aralarında duvarlı kapı.
    void BuildLevel1()
    {
        var root = CreateCompound(new Vector3(0, 0, 44), "Karakol", 150);
        var t    = root.transform;

        // Sol kule
        Tower(t, new Vector3(-7f, 0, 0), 3, 3f, 4f, Stone);
        // Sağ kule
        Tower(t, new Vector3( 7f, 0, 0), 3, 3f, 4f, Stone);

        // Sol bağlantı duvarı
        Block(t, new Vector3(-3.8f, 3.0f, 0), new Vector3(4.4f, 6.0f, 1.5f), Stone(0.20f));
        // Sağ bağlantı duvarı
        Block(t, new Vector3( 3.8f, 3.0f, 0), new Vector3(4.4f, 6.0f, 1.5f), Stone(0.20f));

        // Kapı sol sütun
        Block(t, new Vector3(-1.3f, 4.0f, 0), new Vector3(2.0f, 8.0f, 1.8f), Stone(0.40f));
        // Kapı sağ sütun
        Block(t, new Vector3( 1.3f, 4.0f, 0), new Vector3(2.0f, 8.0f, 1.8f), Stone(0.40f));
        // Kapı üst kemeri
        Block(t, new Vector3( 0.0f, 8.5f, 0), new Vector3(5.5f, 2.0f, 2.0f), Stone(0.60f));
    }

    // ── LEVEL 2: Depo Kompleksi ───────────────────────────────
    // Merkez depo + iki yan kule + köprüler + arka ek.
    void BuildLevel2()
    {
        var root = CreateCompound(new Vector3(0, 0, 58), "Depo Kompleksi", 320);
        var t    = root.transform;

        // Ana depo (geniş 3 kat)
        Block(t, new Vector3(0, 2.0f, 0), new Vector3(12f, 4f, 8f), Concrete(0.10f));
        Block(t, new Vector3(0, 6.0f, 0), new Vector3(10f, 4f, 7f), Concrete(0.30f));
        Block(t, new Vector3(0,10.0f, 0), new Vector3( 8f, 4f, 6f), Concrete(0.50f));

        // Sol kule
        Tower(t, new Vector3(-9f, 0, 1f), 4, 3f, 4.5f, Stone);
        // Sağ kule
        Tower(t, new Vector3( 9f, 0, 1f), 4, 3f, 4.5f, Stone);

        // Sol köprü (kat-3 seviyesi)
        Block(t, new Vector3(-5.5f, 9.5f, 0.5f), new Vector3(3.5f, 1.0f, 3f), Metal(0.20f));
        // Sağ köprü
        Block(t, new Vector3( 5.5f, 9.5f, 0.5f), new Vector3(3.5f, 1.0f, 3f), Metal(0.20f));

        // Arka ek yapı
        Tower(t, new Vector3(0, 0, 7f), 5, 3f, 3.5f, Stone);
        // Arka bağlantı duvarı
        Block(t, new Vector3(0, 4.0f, 4.0f), new Vector3(8f, 8f, 1.2f), Concrete(0.30f));
    }

    // ── LEVEL 3: Kale ─────────────────────────────────────────
    // 4 köşe kulesi + bağlantı duvarları + kapı kemeri + merkez kule.
    void BuildLevel3()
    {
        var root = CreateCompound(new Vector3(0, 0, 72), "Kale", 560);
        var t    = root.transform;

        // 4 köşe kulesi
        Tower(t, new Vector3(-11f, 0, -11f), 6, 3f, 4.5f, Stone);
        Tower(t, new Vector3( 11f, 0, -11f), 6, 3f, 4.5f, Stone);
        Tower(t, new Vector3(-11f, 0,  11f), 6, 3f, 4.5f, Stone);
        Tower(t, new Vector3( 11f, 0,  11f), 6, 3f, 4.5f, Stone);

        // Ön duvar – sol ve sağ parça (kapı için boşluk ortada)
        Block(t, new Vector3(-6.5f, 5.5f, -11f), new Vector3(5.5f, 11f, 1.5f), Stone(0.22f));
        Block(t, new Vector3( 6.5f, 5.5f, -11f), new Vector3(5.5f, 11f, 1.5f), Stone(0.22f));
        // Kapı sütunları
        Block(t, new Vector3(-1.5f, 4.5f, -11f), new Vector3(2.5f, 9.0f, 2.0f), Stone(0.42f));
        Block(t, new Vector3( 1.5f, 4.5f, -11f), new Vector3(2.5f, 9.0f, 2.0f), Stone(0.42f));
        // Kapı üst kemeri
        Block(t, new Vector3( 0.0f, 9.5f, -11f), new Vector3(7.5f, 2.5f, 2.5f), Stone(0.62f));

        // Yan duvarlar (W ve E)
        Block(t, new Vector3(-11f, 5.5f, 0), new Vector3(1.5f, 11f, 20f), Stone(0.20f));
        Block(t, new Vector3( 11f, 5.5f, 0), new Vector3(1.5f, 11f, 20f), Stone(0.20f));

        // Arka duvar
        Block(t, new Vector3(0, 5.5f, 11f), new Vector3(20f, 11f, 1.5f), Stone(0.22f));

        // Merkez kale kulesi
        Tower(t, new Vector3(0, 0, 0), 8, 3f, 5.5f, Stone);

        // Merkez kule üst platform
        Block(t, new Vector3(0, 24.5f, 0), new Vector3(7f, 1.5f, 7f), Stone(0.55f));
    }

    // ── LEVEL 4: Endüstriyel Kompleks ─────────────────────────
    // Fabrikalar + bacalar + yüksek platform + radar kulesi.
    void BuildLevel4()
    {
        var root = CreateCompound(new Vector3(0, 0, 88), "Endüstriyel Kompleks", 920);
        var t    = root.transform;

        // Sol fabrika
        Tower(t, new Vector3(-13f, 0, -4f), 4, 4f, 7f, Concrete);
        // Sağ fabrika
        Tower(t, new Vector3( 13f, 0, -4f), 4, 4f, 7f, Concrete);

        // Merkez ana bina (3 katlı, geniş)
        Block(t, new Vector3(0,  3.0f, 0), new Vector3(11f, 6f, 11f), Concrete(0.12f));
        Block(t, new Vector3(0,  9.0f, 0), new Vector3( 9f, 6f,  9f), Concrete(0.33f));
        Block(t, new Vector3(0, 15.0f, 0), new Vector3( 7f, 6f,  7f), Concrete(0.54f));

        // Sol baca (ince uzun)
        Tower(t, new Vector3(-5.5f, 0, 0), 11, 3f, 2.5f, Rust);
        // Sağ baca
        Tower(t, new Vector3( 5.5f, 0, 0), 11, 3f, 2.5f, Rust);

        // Üst platform (bacaları birbirine bağlar)
        Block(t, new Vector3(0, 24.5f, 0), new Vector3(15f, 1.5f, 4.5f), Metal(0.12f));

        // Sol fabrika→merkez köprüsü
        Block(t, new Vector3(-7.5f, 13.0f, -0.5f), new Vector3(4.5f, 1.2f, 4.5f), Metal(0.22f));
        // Sağ fabrika→merkez köprüsü
        Block(t, new Vector3( 7.5f, 13.0f, -0.5f), new Vector3(4.5f, 1.2f, 4.5f), Metal(0.22f));

        // Arka kule
        Tower(t, new Vector3(0, 0, 13f), 7, 3f, 4.5f, Metal);
        // Arka bağlantı duvarı
        Block(t, new Vector3(0, 5.5f, 7.5f), new Vector3(7f, 11f, 1.5f), Concrete(0.30f));

        // Radar kulesi
        Block(t, new Vector3(0, 22.0f, 0), new Vector3(3.5f, 7f, 3.5f), Metal(0.08f));
        // Radar anteni
        Block(t, new Vector3(0, 29.0f, 0), new Vector3(5f, 1f, 1f), Metal(0.05f));
        Block(t, new Vector3(0, 29.0f, 0), new Vector3(1f, 1f, 5f), Metal(0.05f));
    }

    // ── Yardımcı metodlar ──────────────────────────────────────

    static GameObject CreateCompound(Vector3 worldPos, string name, int reward)
    {
        var go = new GameObject(name);
        go.transform.position = worldPos;
        var tb = go.AddComponent<TargetBuilding>();
        tb.buildingName   = name;
        tb.coinRewardFull = reward;
        return go;
    }

    static void Tower(Transform parent, Vector3 baseLocal, int floors, float floorH, float width, Func<float, Color> palette)
    {
        for (int i = 0; i < floors; i++)
        {
            float ratio = floors > 1 ? (float)i / (floors - 1) : 0f;
            Vector3 pos = baseLocal + new Vector3(0, i * floorH + floorH * 0.5f, 0);
            Block(parent, pos, new Vector3(width, floorH * 0.94f, width), palette(ratio));
        }
    }

    static Shader _shader;
    static Shader URP => _shader != null ? _shader : (_shader = Shader.Find("Universal Render Pipeline/Lit"));

    static void Block(Transform parent, Vector3 localPos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;

        var mat = new Material(URP);
        mat.color = color;
        go.GetComponent<Renderer>().material = mat;

        go.AddComponent<BuildingSection>();
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.mass        = 5f;
    }
}
