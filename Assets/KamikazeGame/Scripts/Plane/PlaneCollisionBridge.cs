using UnityEngine;

/// <summary>
/// Child objedeki (Body) trigger'ı parent'taki PlaneImpact'a iletir.
/// Unity'de trigger sadece o objenin script'lerini çağırır,
/// bu bridge parent'a aktarır.
/// </summary>
public class PlaneCollisionBridge : MonoBehaviour
{
    private PlaneImpact _impact;

    void Awake()
    {
        _impact = GetComponentInParent<PlaneImpact>();
    }

    void OnTriggerEnter(Collider other)
    {
        _impact?.OnChildTriggerEnter(other);
    }
}
