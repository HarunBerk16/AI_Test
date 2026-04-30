using UnityEditor;
using UnityEngine;

public class SceneSetup
{
    public static void Execute()
    {
        // --- Uçak ---
        GameObject plane = new GameObject("Plane");
        plane.transform.position = new Vector3(0, 10, -30);
        plane.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Gövde (küp)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(plane.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1f, 0.3f, 2f);

        // Sol kanat
        GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingL.name = "WingLeft";
        wingL.transform.SetParent(plane.transform);
        wingL.transform.localPosition = new Vector3(-1.2f, 0f, 0f);
        wingL.transform.localScale = new Vector3(1.5f, 0.1f, 0.8f);

        // Sağ kanat
        GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingR.name = "WingRight";
        wingR.transform.SetParent(plane.transform);
        wingR.transform.localPosition = new Vector3(1.2f, 0f, 0f);
        wingR.transform.localScale = new Vector3(1.5f, 0.1f, 0.8f);

        // Kuyruk
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tail.name = "Tail";
        tail.transform.SetParent(plane.transform);
        tail.transform.localPosition = new Vector3(0f, 0.3f, -0.9f);
        tail.transform.localScale = new Vector3(0.1f, 0.6f, 0.4f);

        // PlaneController script ekle
        plane.AddComponent<PlaneController>();

        // Collider sadece body'de olsun, trigger olarak
        Collider bodyCol = body.GetComponent<Collider>();
        bodyCol.isTrigger = true;

        // --- Kamera: uçağı takip etsin ---
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 12, -40);
            cam.transform.rotation = Quaternion.Euler(10, 0, 0);

            CameraFollow cf = cam.gameObject.AddComponent<CameraFollow>();
            cf.target = plane.transform;
        }

        Debug.Log("Sahne kurulumu tamamlandi!");
    }
}
