using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Menu fazında uçak hareketsiz durur.
/// Flying fazına geçince başlangıç pozisyonundan uçuşa başlar.
/// </summary>
public class PlaneController : MonoBehaviour
{
    [Header("Hareket")]
    public float speed = 20f;
    public float turnSpeed = 80f;
    public float stabilizeSpeed = 3f;

    [Header("Sürükleme Hassasiyeti")]
    public float dragSensitivity = 0.3f;

    [Header("Süzülme")]
    public float glideGravity = 5f;
    public float glideDeceleration = 3f;

    private float _targetPitch;
    private float _targetYaw;
    private Vector2 _lastPointerPos;
    private bool _isTouching;
    private bool _isFlying = false;
    private bool _isGliding = false;

    private Vector3 _startPos;
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
        if (!_isFlying) return;
        HandleInput();
        Fly();
    }

    void HandleInput()
    {
        Touchscreen touch = Touchscreen.current;
        Mouse mouse = Mouse.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            Vector2 pos = touch.primaryTouch.position.ReadValue();
            if (touch.primaryTouch.press.wasPressedThisFrame)
            {
                _lastPointerPos = pos;
                _isTouching = true;
            }
            else if (_isTouching)
            {
                Vector2 delta = pos - _lastPointerPos;
                _targetYaw   =  delta.x * dragSensitivity;
                _targetPitch = -delta.y * dragSensitivity;
                _lastPointerPos = pos;
            }
        }
        else if (mouse != null && mouse.leftButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _targetYaw   =  delta.x * dragSensitivity * 2f;
            _targetPitch = -delta.y * dragSensitivity * 2f;
            _isTouching = true;
        }
        else
        {
            _isTouching = false;
        }

        if (!_isTouching)
        {
            _targetPitch = Mathf.Lerp(_targetPitch, 0f, Time.deltaTime * stabilizeSpeed);
            _targetYaw   = Mathf.Lerp(_targetYaw,   0f, Time.deltaTime * stabilizeSpeed);
        }
    }

    void Fly()
    {
        float pitch = Mathf.Clamp(_targetPitch * turnSpeed, -90f, 90f);
        float yaw   = Mathf.Clamp(_targetYaw   * turnSpeed, -90f, 90f);
        transform.Rotate(pitch * Time.deltaTime, yaw * Time.deltaTime, 0f, Space.Self);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void Glide()
    {
        speed = Mathf.Max(0f, speed - glideDeceleration * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;
        transform.position -= new Vector3(0, glideGravity * Time.deltaTime, 0);
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void StopFlying()  { _isFlying = false; }
    public void StartGliding() { _isFlying = false; _isGliding = true; }
}
