using UnityEngine;

/// <summary>
/// Sahnedeki çarpılabilir çevre objesi (ağaç, kaya, çalı).
/// PlaneImpact bu component'i algılar ve çarpışmada objeyi devirir.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnvironmentObstacle : MonoBehaviour
{
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity  = true;
    }

    public void GetHit(Vector3 hitDirection)
    {
        _rb.isKinematic = false;
        _rb.AddForce(hitDirection * 8f + Vector3.up * 2f, ForceMode.Impulse);
        _rb.AddTorque(Random.insideUnitSphere * 6f, ForceMode.Impulse);
    }
}
