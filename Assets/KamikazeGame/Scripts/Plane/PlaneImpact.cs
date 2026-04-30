using UnityEngine;

/// <summary>
/// Uçak hedefe çarpınca çalışır.
/// PlaneController'ı durdurur, GameEvents'e bildirir.
/// </summary>
[RequireComponent(typeof(PlaneController))]
public class PlaneImpact : MonoBehaviour
{
    private PlaneController _controller;
    private bool _hasImpacted = false;

    void Awake()
    {
        _controller = GetComponent<PlaneController>();
    }

    // Body child'ın trigger'ı buraya iletilir
    public void OnChildTriggerEnter(Collider other)
    {
        if (_hasImpacted) return;

        // Sadece BuildingSection veya TargetBuilding olan objelere çarp
        if (other.GetComponentInParent<TargetBuilding>() == null) return;

        _hasImpacted = true;
        _controller.StopFlying();

        Vector3 impactPoint = transform.position;
        GameEvents.OnPlaneImpact?.Invoke(impactPoint);

        Debug.Log($"Carpma noktasi: {impactPoint}");
    }
}
