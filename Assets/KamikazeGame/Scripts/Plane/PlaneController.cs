using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [Header("Hareket")]
    public float speed      = 20f;
    public float turnSpeed  = 80f;
    public float stabilizeSpeed = 4f;

    [Header("Joystick")]
    public float joystickRadius = 320f; // ekran pikseli cinsinden yarıçap

    [Header("Süzülme")]
    public float glideGravity     = 5f;
    public float glideDeceleration = 3f;

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
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

    void OnEnable()  => GameStateManager.OnPhaseChanged += HandlePhaseChange;
    void OnDisable() => GameStateManager.OnPhaseChanged -= HandlePhaseChange;

    void HandlePhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Flying || phase == GamePhase.Menu)
        {
            // Rigidbody varsa kinematik yap — BreakApart sonrası fizik çakışmasını önler
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
            speed     = UpgradeData.FuelSpeed(GameData.FuelLevel);
            turnSpeed = UpgradeData.WingTurnSpeed(GameData.WingLevel);
        }
        _isFlying = (phase == GamePhase.Flying);
    }

    void Update()
    {
        if (_isGliding) { Glide(); return; }
        if (!_isFlying)  return;
        HandleInput();
        Fly();
    }

    void HandleInput()
    {
        Touchscreen touch = Touchscreen.current;
        Mouse        mouse = Mouse.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            Vector2 pos = touch.primaryTouch.position.ReadValue();

            if (touch.primaryTouch.press.wasPressedThisFrame)
            {
                _touchStartPos = pos;
                _isTouching    = true;
            }

            if (_isTouching)
                ApplyJoystick(pos - _touchStartPos);
        }
        else if (mouse != null && mouse.leftButton.isPressed)
        {
            Vector2 pos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _touchStartPos = pos;
                _isTouching    = true;
            }

            if (_isTouching)
                ApplyJoystick(pos - _touchStartPos);
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
        // Yarıçap dışına taşmayı engelle, normalize et (-1..+1)
        Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadius);
        _targetYaw   =  clamped.x / joystickRadius;
        _targetPitch = -clamped.y / joystickRadius;
    }

    void Fly()
    {
        float pitch = _targetPitch * turnSpeed;
        float yaw   = _targetYaw   * turnSpeed;
        transform.Rotate(pitch * Time.deltaTime, yaw * Time.deltaTime, 0f, Space.Self);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void Glide()
    {
        speed = Mathf.Max(0f, speed - glideDeceleration * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;
        transform.position -= new Vector3(0, glideGravity * Time.deltaTime, 0);
    }

    public void SetSpeed(float s)  => speed = s;
    public void StopFlying()       => _isFlying = false;
    public void StartGliding()     { _isFlying = false; _isGliding = true; }
}
