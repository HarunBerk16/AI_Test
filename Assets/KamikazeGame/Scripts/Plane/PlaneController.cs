using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Uçak kontrolü:
/// - Uçak her zaman ileri gider (transform.forward)
/// - Parmakla sürükleyince yön değişir (pitch/yaw)
/// - Parmak kalkınca düz uçar
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
    public float glideGravity = 5f;      // Hedefi geçince aşağı çekiş
    public float glideDeceleration = 3f; // Hedefi geçince yavaşlama

    private float _targetPitch;
    private float _targetYaw;
    private Vector2 _lastPointerPos;
    private bool _isTouching;
    private bool _isFlying = true;
    private bool _isGliding = false;

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
        // Yavaşça ileri ve aşağı git
        speed = Mathf.Max(0f, speed - glideDeceleration * Time.deltaTime);
        transform.position += transform.forward * speed * Time.deltaTime;
        transform.position -= new Vector3(0, glideGravity * Time.deltaTime, 0);

        // Yere değdi mi kontrolü MissionController halleder
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
    public void StopFlying() => _isFlying = false;
    public void StartGliding() { _isFlying = false; _isGliding = true; }
}
