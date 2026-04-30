using UnityEngine;

/// <summary>
/// Görev durumunu yönetir:
/// - Uçak hedefi geçtiyse → süzülme başlar, kamera durur
/// - Süzülürken yere değdiyse → crash
/// </summary>
public class MissionController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform plane;
    public CameraFollow cameraFollow;

    [Header("Ayarlar")]
    public float missThreshold = 8f;   // Hedefi kaç metre geçince "kaçırdı"
    public float groundHeight  = 0.3f; // Bu yüksekliğin altı = yere değdi

    private TargetBuilding _target;
    private PlaneController _planeController;
    private PlaneImpact _planeImpact;
    private bool _missionEnded = false;
    private bool _isGliding = false;

    void Start()
    {
        _target          = FindFirstObjectByType<TargetBuilding>();
        _planeController = plane.GetComponent<PlaneController>();
        _planeImpact     = plane.GetComponent<PlaneImpact>();

        GameEvents.OnPlaneImpact += OnImpact;
        GameEvents.OnPlaneCrash  += OnCrash;
    }

    void OnDestroy()
    {
        GameEvents.OnPlaneImpact -= OnImpact;
        GameEvents.OnPlaneCrash  -= OnCrash;
    }

    void OnImpact(Vector3 _) => _missionEnded = true;
    void OnCrash()           => _missionEnded = true;

    void Update()
    {
        if (_missionEnded || plane == null || _target == null) return;

        // Hedefi geçti mi?
        if (!_isGliding && plane.position.z > _target.transform.position.z + missThreshold)
        {
            _isGliding = true;
            _planeController.StartGliding();

            if (cameraFollow != null) cameraFollow.enabled = false;

            GameEvents.OnTargetMissed?.Invoke();
            Debug.Log("Hedef kacirıldı, suzulme basliyor.");
        }

        // Glide sırasında yere değdi mi?
        if (_isGliding && plane.position.y <= groundHeight)
        {
            _missionEnded = true;
            _planeImpact.NotifyGroundCrash();
            Debug.Log("Suzulurken yere degdi: crash.");
        }
    }
}
