using UnityEngine;

/// <summary>
/// Hull seviyesine göre uçağın görsel şeklini primitives ile inşa eder.
/// Fizik/collider child'larına dokunmaz — ayrı "VisualModel" grubu oluşturur.
/// </summary>
public class PlaneVisuals : MonoBehaviour
{
    private GameObject _modelRoot;

    void OnEnable()  => GameEvents.OnUpgradePurchased += Rebuild;
    void OnDisable() => GameEvents.OnUpgradePurchased -= Rebuild;
    void Start()     => Rebuild();

    void Rebuild()
    {
        if (_modelRoot != null) Destroy(_modelRoot);

        // Orijinal mesh'leri gizle (VisualModel dışındaki tüm renderer'lar)
        var ownRend = GetComponent<MeshRenderer>();
        if (ownRend != null) ownRend.enabled = false;

        foreach (Transform child in transform)
        {
            if (child.name == "VisualModel") continue;
            foreach (var rend in child.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
                rend.enabled = false;
        }

        _modelRoot = new GameObject("VisualModel");
        _modelRoot.transform.SetParent(transform, false);

        switch (GameData.HullLevel)
        {
            case 0: BuildSmallPlane(_modelRoot.transform); break;
            case 1: BuildTubePlane(_modelRoot.transform);  break;
            default: BuildShahed(_modelRoot.transform);    break;
        }
    }

    // ── Hull 0: Küçük konvansiyonel uçak ───────────────────────────────────
    void BuildSmallPlane(Transform r)
    {
        Color body  = new Color(0.78f, 0.78f, 0.82f);
        Color wing  = new Color(0.72f, 0.72f, 0.76f);
        Color tail  = new Color(0.68f, 0.68f, 0.72f);
        Color glass = new Color(0.35f, 0.65f, 1.00f);

        // Gövde — kapsül, Z boyunca
        Part(r, "Fuselage", PrimitiveType.Capsule,
             new Vector3(0,    0,     0),    Quaternion.Euler(90,0,0),
             new Vector3(0.44f, 1.9f, 0.44f), body);

        // Kanatlar — hafif sweepback
        Part(r, "WingL", PrimitiveType.Cube,
             new Vector3(-1.45f, 0, 0.15f), Quaternion.Euler(0, 12, -3f),
             new Vector3(2.7f, 0.07f, 0.95f), wing);
        Part(r, "WingR", PrimitiveType.Cube,
             new Vector3( 1.45f, 0, 0.15f), Quaternion.Euler(0,-12,  3f),
             new Vector3(2.7f, 0.07f, 0.95f), wing);

        // Yatay kuyruk
        Part(r, "TailH", PrimitiveType.Cube,
             new Vector3(0, 0, -1.65f), Quaternion.identity,
             new Vector3(1.35f, 0.065f, 0.5f), tail);

        // Dikey kuyruk
        Part(r, "TailV", PrimitiveType.Cube,
             new Vector3(0, 0.38f, -1.70f), Quaternion.identity,
             new Vector3(0.065f, 0.65f, 0.48f), tail);

        // Burun koni
        Part(r, "Nose", PrimitiveType.Sphere,
             new Vector3(0, 0, 1.82f), Quaternion.identity,
             new Vector3(0.28f, 0.28f, 0.48f), body);

        // Kokpit
        Part(r, "Cockpit", PrimitiveType.Sphere,
             new Vector3(0, 0.26f, 1.05f), Quaternion.identity,
             new Vector3(0.26f, 0.20f, 0.42f), glass);

        // Motor çıkıntısı (altında)
        Part(r, "Engine", PrimitiveType.Cylinder,
             new Vector3(0, -0.28f, 0.3f), Quaternion.Euler(90,0,0),
             new Vector3(0.18f, 0.45f, 0.18f), new Color(0.3f,0.3f,0.32f));
    }

    // ── Hull 1: Uzun boru gövde ──────────────────────────────────────────────
    void BuildTubePlane(Transform r)
    {
        Color bodyCol = new Color(0.60f, 0.65f, 0.70f);
        Color finCol  = new Color(0.50f, 0.55f, 0.60f);
        Color whead   = new Color(0.80f, 0.28f, 0.18f);

        // Uzun ince gövde
        Part(r, "Fuselage", PrimitiveType.Capsule,
             Vector3.zero, Quaternion.Euler(90,0,0),
             new Vector3(0.28f, 3.0f, 0.28f), bodyCol);

        // Küçük geride kanatlar
        Part(r, "WingL", PrimitiveType.Cube,
             new Vector3(-0.95f, 0, -0.4f), Quaternion.Euler(0, 18, -4f),
             new Vector3(1.8f, 0.055f, 0.62f), finCol);
        Part(r, "WingR", PrimitiveType.Cube,
             new Vector3( 0.95f, 0, -0.4f), Quaternion.Euler(0,-18,  4f),
             new Vector3(1.8f, 0.055f, 0.62f), finCol);

        // Çapraz kuyruk yüzgeçleri (X-tail)
        float fz = -2.6f;
        Part(r, "FinT",  PrimitiveType.Cube, new Vector3(0,    0.38f, fz), Quaternion.identity, new Vector3(0.055f, 0.55f, 0.38f), finCol);
        Part(r, "FinB",  PrimitiveType.Cube, new Vector3(0,   -0.38f, fz), Quaternion.identity, new Vector3(0.055f, 0.55f, 0.38f), finCol);
        Part(r, "FinL",  PrimitiveType.Cube, new Vector3(-0.38f, 0,   fz), Quaternion.identity, new Vector3(0.55f, 0.055f, 0.38f), finCol);
        Part(r, "FinR",  PrimitiveType.Cube, new Vector3( 0.38f, 0,   fz), Quaternion.identity, new Vector3(0.55f, 0.055f, 0.38f), finCol);

        // Savaş başlığı (öne, konik kırmızı)
        Part(r, "Warhead", PrimitiveType.Sphere,
             new Vector3(0, 0, 2.85f), Quaternion.identity,
             new Vector3(0.26f, 0.26f, 0.58f), whead);
    }

    // ── Hull 2: Shahed drone ─────────────────────────────────────────────────
    void BuildShahed(Transform r)
    {
        Color sand   = new Color(0.55f, 0.50f, 0.38f);
        Color dark   = new Color(0.40f, 0.36f, 0.28f);
        Color engine = new Color(0.22f, 0.22f, 0.22f);
        Color sensor = new Color(0.08f, 0.08f, 0.12f);

        // Merkez gövde
        Part(r, "Body", PrimitiveType.Cube,
             Vector3.zero, Quaternion.identity,
             new Vector3(0.48f, 0.13f, 2.3f), sand);

        // Sol delta kanat (sweepback ~30°)
        Part(r, "DeltaL", PrimitiveType.Cube,
             new Vector3(-1.5f, 0, -0.5f), Quaternion.Euler(0,-30f,-1.5f),
             new Vector3(2.6f, 0.065f, 0.65f), dark);

        // Sağ delta kanat
        Part(r, "DeltaR", PrimitiveType.Cube,
             new Vector3( 1.5f, 0, -0.5f), Quaternion.Euler(0, 30f, 1.5f),
             new Vector3(2.6f, 0.065f, 0.65f), dark);

        // V-kuyruk sol
        Part(r, "VTailL", PrimitiveType.Cube,
             new Vector3(-0.32f, 0.22f, -1.05f), Quaternion.Euler(0, 12,-38f),
             new Vector3(0.48f, 0.055f, 0.32f), dark);

        // V-kuyruk sağ
        Part(r, "VTailR", PrimitiveType.Cube,
             new Vector3( 0.32f, 0.22f, -1.05f), Quaternion.Euler(0,-12, 38f),
             new Vector3(0.48f, 0.055f, 0.32f), dark);

        // Arka piston motoru
        Part(r, "Engine", PrimitiveType.Cylinder,
             new Vector3(0, 0.14f, -1.1f), Quaternion.Euler(90,0,0),
             new Vector3(0.14f, 0.28f, 0.14f), engine);

        // Pervane disk (görsel)
        Part(r, "PropDisk", PrimitiveType.Cylinder,
             new Vector3(0, 0.14f, -1.38f), Quaternion.Euler(90,0,0),
             new Vector3(0.35f, 0.02f, 0.35f), new Color(0.2f,0.2f,0.2f,0.5f));

        // Optik sensör kafası
        Part(r, "Sensor", PrimitiveType.Sphere,
             new Vector3(0, -0.04f, 1.15f), Quaternion.identity,
             new Vector3(0.16f, 0.16f, 0.16f), sensor);
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────────
    static void Part(Transform parent, string partName, PrimitiveType type,
                     Vector3 pos, Quaternion rot, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = partName;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
        go.transform.localScale    = scale;

        // Görsel amaçlı: collider gereksiz
        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        var rend = go.GetComponent<MeshRenderer>();
        if (rend == null) return;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null) mat = new Material(Shader.Find("Standard"));
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", 0.25f);
        rend.sharedMaterial = mat;
    }
}
