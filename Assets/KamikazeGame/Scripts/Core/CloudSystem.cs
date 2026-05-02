using UnityEngine;

/// <summary>
/// Başlangıçta sahnede çeşitli konumlara bulut puf grupları yerleştirir.
/// </summary>
public class CloudSystem : MonoBehaviour
{
    void Start() => SpawnClouds();

    void SpawnClouds()
    {
        // x: -150..150  z: -60..200  y: 70..130
        var positions = new Vector3[]
        {
            new Vector3( -60,  95,  30), new Vector3(  70,  85,  20),
            new Vector3( -90,  110, 80), new Vector3(  50, 100,  90),
            new Vector3(  20,  90, 140), new Vector3(-110,  80,  60),
            new Vector3(  90,  95, 130), new Vector3( -40, 115,  10),
            new Vector3(  30, 105, -20), new Vector3(-130,  90, 110),
            new Vector3(  80,  80, -40), new Vector3( -20, 120,  50),
            new Vector3( 120,  88, 170), new Vector3( -80, 100, 170),
            new Vector3(   0, 108,  60),
        };

        foreach (var p in positions)
            CreateCloud(p, Random.Range(0.8f, 1.4f));
    }

    void CreateCloud(Vector3 center, float scale)
    {
        var root = new GameObject("Cloud");
        root.transform.position = center;
        root.transform.parent   = transform;

        // Ana puf
        Puf(root.transform, Vector3.zero,                new Vector3(18, 9, 14) * scale);
        // Yan puflar
        Puf(root.transform, new Vector3(-10,  -2,  0), new Vector3(12, 7, 11) * scale);
        Puf(root.transform, new Vector3( 10,  -2,  0), new Vector3(13, 7, 11) * scale);
        Puf(root.transform, new Vector3(  0,  -2, -8), new Vector3(10, 6,  9) * scale);
        Puf(root.transform, new Vector3(  0,   4,  2), new Vector3( 9, 6,  8) * scale);
        Puf(root.transform, new Vector3( -6,   3, -4), new Vector3( 7, 5,  7) * scale);
    }

    static readonly Color _cloudColor = new Color(0.96f, 0.97f, 1.00f);

    void Puf(Transform parent, Vector3 localPos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Bulutlar fizik etkileşimine girmesin
        Destroy(go.GetComponent<SphereCollider>());

        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;

        var rend = go.GetComponent<Renderer>();
        rend.shadowCastingMode    = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows       = false;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = _cloudColor;
        rend.material = mat;
    }
}
