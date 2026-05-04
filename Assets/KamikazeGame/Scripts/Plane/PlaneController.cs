using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [Header("Joystick")]
    public float joystickRadius = 320f;

    [Header("Stabilize")]
    public float stabilizeSpeed = 3f;

    [Header("Dalış Fiziği")]
    public float diveAcceleration  = 32f;  // dalışta hızlanma (m/s²)
    public float climbDeceleration = 18f;  // tırmanışta yavaşlama (m/s²)
    public float minSpeedFactor    = 0.30f;
    public float maxSpeedFactor    = 4f;

    [Header("Başlangıç")]
    public float startAltitude  = 45f;  // başlangıç yüksekliği (m)
    public float startPitchDown = 15f;  // başlangıç burun aşağı açısı

    [Header("Süzülme")]
    public float glideGravity      = 8f;
    public float glideDeceleration = 4f;

    // Inspector'da izlemek için (readonly)
    [HideInInspector] public float currentSpeedDisplay;

    private float   _baseSpeed;
    private float   _currentSpeed;
    private float   _turnSpeed;
    private float   _targetPitch;
    private float   _targetYaw;
    private Vector2 _touchStartPos;
    private bool    _isTouching;
    private bool    _isFlying;
    private bool    _isGliding;

    private Vector3    _startPos;
    private Quaternion _startRot;

    void Awake()
    {
        var pos = transform.position;
        pos.y   = startAltitude;
        _startPos = pos;
        _startRot = Quaternion.Euler(startPitchDown, transform.eulerAngles.y, 0f);
    }

    void OnEnable()  => GameStateManager.OnPhaseChanged += HandlePhaseChange;
    void OnDisable() => GameStateManager.OnPhaseChanged -= HandlePhaseChange;

    void HandlePhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Flying || phase == GamePhase.Menu)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity  = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic     = true;
            }

            transform.position = _startPos;
            transform.rotation = _startRot;
            _targetPitch = 0f;
            _targetYaw   = 0f;
            _isGliding   = false;

            _baseSpeed    = UpgradeData.HullSpeed(GameData.HullLevel);
            _currentSpeed = _baseSpeed;
            _turnSpeed    = UpgradeData.StabilityTurnSpeed(GameData.StabilityLevel);
        }
        _isFlying = (phase == GamePhase.Flying);
    }

    void Update()
    {
        if (_isGliding) { Glide(); return; }
        if (!_isFlying)  return;
        HandleInput();
        Fly();
        currentSpeedDisplay = _currentSpeed;
    }

    void HandleInput()
    {
        Touchscreen touch = Touchscreen.current;
        Mouse        mouse = Mouse.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            Vector2 pos = touch.primaryTouch.position.ReadValue();
            if (touch.primaryTouch.press.wasPressedThisFrame)
            { _touchStartPos = pos; _isTouching = true; }
            if (_isTouching) ApplyJoystick(pos - _touchStartPos);
        }
        else if (mouse != null && mouse.leftButton.isPressed)
        {
            Vector2 pos = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
            { _touchStartPos = pos; _isTouching = true; }
            if (_isTouching) ApplyJoystick(pos - _touchStartPos);
        }
        else
        {
            _isTouching  = false;
            _targetPitch = Mathf.Lerp(_targetPitch, 0f, Time.deltaTime * stabilizeSpeed);
            _targetYaw   = Mathf.Lerp(_targetYaw,   0f, Time.deltaTime * stabilizeSpeed);
        }
    }

    void ApplyJoystick(Vector2 offset)
    {
        Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadius);
        _targetYaw   =  clamped.x / joystickRadius;
        _targetPitch = -clamped.y / joystickRadius;
    }

    void Fly()
    {
        float pitch = _targetPitch * _turnSpeed;
        float yaw   = _targetYaw   * _turnSpeed;
        transform.Rotate(pitch * Time.deltaTime, yaw * Time.deltaTime, 0f, Space.Self);

        // Pitch açısına göre hız değişimi: dalış=hızlan, tırmanış=yavaşla
        // Unity: pozitif euler X → burun aşağı (dalış), negatif → burun yukarı (tırmanış)
        float pitchAngle = transform.eulerAngles.x;
        if (pitchAngle > 180f) pitchAngle -= 360f;
        float t = Mathf.Clamp(pitchAngle / 75f, -1f, 1f);
        float accel = t > 0f
            ?  t * diveAcceleration    // burun aşağı (t pozitif): hızlan
            :  t * climbDeceleration;  // burun yukarı (t negatif): yavaşla (accel negatif)

        _currentSpeed = Mathf.Clamp(
            _currentSpeed + accel * Time.deltaTime,
            _baseSpeed * minSpeedFactor,
            _baseSpeed * maxSpeedFactor);

        transform.position += transform.forward * _currentSpeed * Time.deltaTime;
    }

    void Glide()
    {
        _currentSpeed = Mathf.Max(0f, _currentSpeed - glideDeceleration * Time.deltaTime);
        transform.position += transform.forward * _currentSpeed * Time.deltaTime;
        transform.position -= new Vector3(0, glideGravity * Time.deltaTime, 0);
    }

    public void SetSpeed(float s)  { _baseSpeed = s; _currentSpeed = s; }
    public void StopFlying()       => _isFlying = false;
    public void StartGliding()     { _isFlying = false; _isGliding = true; }
}
