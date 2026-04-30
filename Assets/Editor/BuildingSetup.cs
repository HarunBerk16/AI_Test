using UnityEditor;
using UnityEngine;

public class BuildingSetup
{
    public static void Execute()
    {
        // PlaneImpact ve bridge ekle
        GameObject plane = GameObject.Find("Plane");
        if (plane != null)
        {
            if (plane.GetComponent<PlaneImpact>() == null)
                plane.AddComponent<PlaneImpact>();

            GameObject body = plane.transform.Find("Body")?.gameObject;
            if (body != null && body.GetComponent<PlaneCollisionBridge>() == null)
                body.AddComponent<PlaneCollisionBridge>();
        }

        // --- Test Binası: 3 katlı ---
        GameObject building = new GameObject("TargetBuilding_01");
        building.transform.position = new Vector3(0, 0, 30);
        building.AddComponent<TargetBuilding>();

        float katYuksekligi = 2.5f;
        Color[] katRenkleri = {
            new Color(0.6f, 0.6f, 0.7f),
            new Color(0.5f, 0.5f, 0.65f),
            new Color(0.4f, 0.4f, 0.6f)
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject kat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kat.name = $"Kat_{i + 1}";
            kat.transform.SetParent(building.transform);
            kat.transform.localPosition = new Vector3(0, i * katYuksekligi + katYuksekligi * 0.5f, 0);
            kat.transform.localScale = new Vector3(5f, katYuksekligi, 5f);

            // Renk
            Renderer r = kat.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = katRenkleri[i];
            r.material = mat;

            // BuildingSection ekle
            kat.AddComponent<BuildingSection>();
        }

        Debug.Log("Test binasi olusturuldu!");
    }
}
