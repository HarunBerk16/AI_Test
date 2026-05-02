using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 followOffset = new Vector3(0f, 5f, -16f);
    public Vector3 menuOffset   = new Vector3(-10f, 7f, -5f);
    public float smoothSpeed    = 5f;

    private GamePhase _phase = GamePhase.Menu;
    private Vector3   _resultLookTarget;
    private Vector3   _planeFallbackPos;
    private bool      _showBuilding;

    void OnEnable()
    {
        GameStateManager.OnPhaseChanged  += HandlePhaseChange;
        GameEvents.OnMissionComplete     += OnMissionComplete;
    }

    void OnDisable()
    {
        GameStateManager.OnPhaseChanged  -= HandlePhaseChange;
        GameEvents.OnMissionComplete     -= OnMissionComplete;
    }

    void OnMissionComplete(float percent, int _earned)
    {
        _showBuilding = percent > 0f;
    }

    void HandlePhaseChange(GamePhase phase)
    {
        _phase = phase;

        if (phase == GamePhase.Menu || phase == GamePhase.Flying)
            _showBuilding = false;

        if (phase == GamePhase.Result)
        {
            if (target != null)
                _planeFallbackPos = target.position;

            // Her zaman bina pozisyonunu bul — _showBuilding bu noktada henüz set edilmemiş olabilir
            var buildings = FindObjectsByType<TargetBuilding>(FindObjectsSortMode.None);
            if (buildings.Length > 0)
            {
                Vector3 center = Vector3.zero;
                foreach (var b in buildings) center += b.transform.position;
                _resultLookTarget = center / buildings.Length;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (_phase == GamePhase.Menu)
        {
            Vector3 menuPos = target.position + target.rotation * menuOffset;
            transform.position = Vector3.Lerp(transform.position, menuPos, Time.deltaTime * smoothSpeed);
            transform.LookAt(target.position + Vector3.up * 1f);
        }
        else if (_phase == GamePhase.Flying)
        {
            Vector3 desiredPos = target.position + target.rotation * followOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
            transform.LookAt(target.position + target.forward * 3f);
        }
        else // Result
        {
            if (_showBuilding)
            {
                // Başarılı vuruş: bina yıkımını göster
                Vector3 desiredPos = _resultLookTarget + new Vector3(-20f, 14f, -28f);
                transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed * 0.6f);
                transform.LookAt(_resultLookTarget + Vector3.up * 4f);
            }
            else
            {
                // Kaçırma veya çarpma: uçağın yanında kal
                Vector3 desiredPos = _planeFallbackPos + new Vector3(0f, 8f, -18f);
                transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed * 0.5f);
                transform.LookAt(_planeFallbackPos + Vector3.up * 2f);
            }
        }
    }
}
