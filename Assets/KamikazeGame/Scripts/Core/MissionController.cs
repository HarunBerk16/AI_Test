using UnityEngine;

/// <summary>
/// Flying fazında aktif olur.
/// Hedef geçilirse süzülme başlatır, yere değince crash bildirir.
/// </summary>
public class MissionController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform plane;

    [Header("Ayarlar")]
    public float missThreshold = 8f;
    public float groundHeight  = 0.3f;

    private TargetBuilding _target;
    private PlaneController _planeController;
    private PlaneImpact _planeImpact;
    private bool _missionActive = false;
    private bool _isGliding = false;

    void Awake()
    {
        _planeController = plane.GetComponent<PlaneController>();
        _planeImpact     = plane.GetComponent<PlaneImpact>();
    }

    void OnEnable()
    {
        GameStateManager.OnPhaseChanged += HandlePhaseChange;
        GameEvents.OnPlaneImpact += OnImpact;
        GameEvents.OnPlaneCrash  += OnCrash;
    }

    void OnDisable()
    {
        GameStateManager.OnPhaseChanged -= HandlePhaseChange;
        GameEvents.OnPlaneImpact -= OnImpact;
        GameEvents.OnPlaneCrash  -= OnCrash;
    }

    void HandlePhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Flying)
        {
            _target = null; // Update'te yeniden bulunur (LevelBuilder rebuild'den sonra)
            _missionActive = true;
            _isGliding = false;
        }
        else
        {
            _missionActive = false;
            _isGliding = false;
        }
    }

    void OnImpact(Vector3 _) => _missionActive = false;
    void OnCrash()           => _missionActive = false;

    void Update()
    {
        if (!_missionActive || plane == null) return;

        if (_target == null)
        {
            _target = FindFirstObjectByType<TargetBuilding>();
            return;
        }

        if (!_isGliding && plane.position.z > _target.transform.position.z + missThreshold)
        {
            _isGliding = true;
            _planeController.StartGliding();
            GameEvents.OnTargetMissed?.Invoke();
        }

        if (_isGliding && plane.position.y <= groundHeight)
        {
            _missionActive = false;
            _planeImpact.NotifyGroundCrash();
        }
    }
}
