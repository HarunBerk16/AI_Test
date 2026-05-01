using UnityEngine;

/// <summary>
/// Menu fazı : uçağın sol-üst-önünden çapraz çekim (ön gösterim).
/// Flying fazı: uçağın arkasından yumuşak takip.
/// Result fazı: son pozisyonda sabit kalır (hasar görünsün).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 followOffset = new Vector3(0f, 4f, -12f);
    public Vector3 menuOffset   = new Vector3(-8f, 6f, -4f);
    public float smoothSpeed = 5f;

    private bool _menuMode = true;

    void OnEnable()  => GameStateManager.OnPhaseChanged += HandlePhaseChange;
    void OnDisable() => GameStateManager.OnPhaseChanged -= HandlePhaseChange;

    void HandlePhaseChange(GamePhase phase) => _menuMode = (phase == GamePhase.Menu);

    void LateUpdate()
    {
        if (target == null) return;

        if (_menuMode)
        {
            Vector3 menuPos = target.position + target.rotation * menuOffset;
            transform.position = Vector3.Lerp(transform.position, menuPos, Time.deltaTime * smoothSpeed);
            transform.LookAt(target.position + Vector3.up * 1f);
        }
        else
        {
            Vector3 desiredPos = target.position + target.rotation * followOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
            transform.LookAt(target.position + target.forward * 3f);
        }
    }
}
