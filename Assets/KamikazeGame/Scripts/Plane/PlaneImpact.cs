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

    void OnEnable()  => GameStateManager.OnPhaseChanged += HandlePhaseChange;
    void OnDisable() => GameStateManager.OnPhaseChanged -= HandlePhaseChange;

    void HandlePhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Menu || phase == GamePhase.Flying)
            _hasImpacted = false;
    }

    public void OnChildTriggerEnter(Collider other)
    {
        if (_hasImpacted) return;

        if (other.gameObject.name == "Ground")
        {
            _hasImpacted = true;
            _controller.StopFlying();
            GameEvents.OnPlaneCrash?.Invoke();
            return;
        }

        if (other.GetComponentInParent<TargetBuilding>() != null)
        {
            _hasImpacted = true;
            _controller.StopFlying();
            GameEvents.OnPlaneImpact?.Invoke(transform.position);
        }
    }

    public void NotifyGroundCrash()
    {
        if (_hasImpacted) return;
        _hasImpacted = true;
        _controller.StopFlying();
        GameEvents.OnPlaneCrash?.Invoke();
    }
}
