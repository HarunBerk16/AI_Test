using UnityEngine;

[RequireComponent(typeof(PlaneController))]
public class PlaneImpact : MonoBehaviour
{
    private PlaneController _controller;
    private bool _hasImpacted = false;

    void Awake()
    {
        _controller = GetComponent<PlaneController>();
    }

    public void OnChildTriggerEnter(Collider other)
    {
        if (_hasImpacted) return;

        // Zemin çarpması: coin yok, crash
        if (other.gameObject.name == "Ground")
        {
            _hasImpacted = true;
            _controller.StopFlying();
            GameEvents.OnPlaneCrash?.Invoke();
            Debug.Log("Yere carpmak: crash!");
            return;
        }

        // Bina çarpması: patlama ve coin
        if (other.GetComponentInParent<TargetBuilding>() != null)
        {
            _hasImpacted = true;
            _controller.StopFlying();
            GameEvents.OnPlaneImpact?.Invoke(transform.position);
            Debug.Log($"Bina carpma: {transform.position}");
        }
    }

    // Dışarıdan zemin çarpması bildirilebilir (glide sonunda yere inince)
    public void NotifyGroundCrash()
    {
        if (_hasImpacted) return;
        _hasImpacted = true;
        _controller.StopFlying();
        GameEvents.OnPlaneCrash?.Invoke();
    }
}
