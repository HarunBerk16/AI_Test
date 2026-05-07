using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [Header("Joystick")]
    public float joystickRadius = 320f;

    [Header("Dönüş & Yatış")]
    public float maxBankAngle    = 40f;
    public float bankReturnSpeed = 5f;

    [Header("Dalış Fiziği")]
    public float diveAcceleration  = 32f;
    public float climbDeceleration = 42f;   // tırmanışta hız çok daha hızlı düşer
    public float minSpeedFactor    = 0.28f;
    public float maxSpeedFactor    = 4f;

    [Header("Stall")]
    public float stallSpeedFactor = 0.45f;  // base speed'in bu oranının altı → stall başlar
    public float stallPitchRate   = 50f;    // burun düşme hızı (derece/s) — tam stall'da
    public float stallDownForce   = 12f;    // ekstra aşağı baskı (m/s) — tam stall'da

    [Header("Pitch Sınırı")]
    public float pitchMin = -60f;  // maksimum burun yukarı açısı (ters dönmeyi önler)
    public float pitchMax =  85f;  // maksimum burun aşağı açısı

    [Header("Yaw Sınırı")]
    public float yawLimit       = 60f;  // maksimum yaw açısı (derece)
    public float yawSoftZone    = 20f;  // bu kadar kala input zayıflamaya başlar
    public float yawReturnSpeed = 30f;  // sınırdan geri dönüş hızı (derece/s)

    [Header("İrtifa Tavanı")]
    public float maxAltitude        = 120f;  // bulut irtifası — bu yüksekliğe çıkılamaz
    public float altitudeSoftZone   =  25f;  // bu kadar kala input kısılmaya, geri baskı başlamaya başlar
    public float altitudePitchRate  =  30f;  // tavana yakınken otomatik burun aşağı hızı (derece/s)

    [Header("Başlangıç")]
    public float startAltitude  = 45f;
    public float startPitchDown = 15f;

    [Header("Süzülme (Isska)")]
    public float glideGravity      = 28f;   // ivmelenen yerçekimi (m/s²)
    public float glideDeceleration = 4f;

    [HideInInspector] public float currentSpeedDisplay;

    private float _baseSpeed;
    private float _currentSpeed;
    private float _turnSpeed;
    private float _worldYaw;
    private float _pitchAngle;
    private float _bankAngle;
    private float _yawInput;
    private float _pitchInput;
    private float _glideVerticalSpeed;
    private Vector2 _touchStartPos;
    private bool _isTouching;
    private bool _isFlying;
    private bool _isGliding;

    private Vector3    _startPos;
    private Quaternion _startRot;

    void Awake()
    {
        var pos = transform.position;
        pos.y     = startAltitude;
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

            var e = _startRot.eulerAngles;
            _worldYaw   = e.y;
            _pitchAngle = startPitchDown;
            _bankAngle  = 0f;
            _yawInput   = 0f;
            _pitchInput = 0f;
            _isGliding  = false;
            _glideVerticalSpeed = 0f;

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
            _isTouching = false;
            _yawInput   = 0f;
            _pitchInput = 0f;
        }
    }

    void ApplyJoystick(Vector2 offset)
    {
        Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadius);
        _yawInput   =  clamped.x / joystickRadius;
        _pitchInput = -clamped.y / joystickRadius;
    }

    void Fly()
    {
        float dt = Time.deltaTime;

        // --- Yumuşak yaw sınırı ---
        // Sınıra yaklaştıkça o yöndeki input sıfıra azalır; sınırdan geri çekim kuvveti artar.
        float effectiveYaw = _yawInput;
        float absYaw    = Mathf.Abs(_worldYaw);
        float softStart = yawLimit - yawSoftZone;
        if (absYaw > softStart)
        {
            float t = Mathf.Clamp01((absYaw - softStart) / yawSoftZone);
            if (Mathf.Sign(_yawInput) == Mathf.Sign(_worldYaw))
                effectiveYaw *= (1f - t);                                   // input kısılır
            _worldYaw -= Mathf.Sign(_worldYaw) * yawReturnSpeed * t * dt;  // otomatik geri çekim
        }
        _worldYaw += effectiveYaw * _turnSpeed * dt;
        _worldYaw  = Mathf.Clamp(_worldYaw, -yawLimit, yawLimit);          // güvenlik hard clamp

        // --- Yumuşak irtifa tavanı ---
        // Tavana yaklaştıkça burun-yukarı input kısılır ve otomatik burun-aşağı baskı artar.
        float effectivePitch = _pitchInput;
        float altOvershoot   = transform.position.y - (maxAltitude - altitudeSoftZone);
        if (altOvershoot > 0f)
        {
            float at = Mathf.Clamp01(altOvershoot / altitudeSoftZone);
            if (_pitchInput < 0f)          // burun yukarı input → kıs
                effectivePitch *= (1f - at);
            _pitchAngle += altitudePitchRate * at * dt;  // otomatik burun aşağı
        }

        // --- Pitch (yumuşak irtifa dahil, hard clamp ile ters dönme engeli) ---
        _pitchAngle += effectivePitch * _turnSpeed * dt;
        _pitchAngle  = Mathf.Clamp(_pitchAngle, pitchMin, pitchMax);

        // --- Smooth stall ---
        // _pitchAngle < 0 → burun yukarı (tırmanış). Hız eşiğin altına düşünce stallFactor artar.
        // stallFactor: 0 = eşikte, 1 = minimum hızda → smooth, binary geçiş yok.
        float stallFactor = 0f;
        if (_pitchAngle < -5f)
        {
            float speedRatio = _currentSpeed / _baseSpeed;
            stallFactor = Mathf.Clamp01(
                (stallSpeedFactor - speedRatio) / (stallSpeedFactor - minSpeedFactor));
        }

        // Stall derinleştikçe burun giderek daha hızlı aşağı döner
        if (stallFactor > 0f)
            _pitchAngle += stallPitchRate * stallFactor * dt;

        // --- Bank: effective yaw'a göre — sınırda banking de azalır ---
        float targetBank = _isTouching ? -effectiveYaw * maxBankAngle : 0f;
        _bankAngle = Mathf.Lerp(_bankAngle, targetBank, dt * bankReturnSpeed);

        transform.rotation = Quaternion.Euler(_pitchAngle, _worldYaw, _bankAngle);

        // --- Hız: pitch açısına göre ivme/yavaşlama ---
        float p = _pitchAngle;
        if (p > 180f) p -= 360f;
        float pitchT = Mathf.Clamp(p / 75f, -1f, 1f);
        float accel = pitchT > 0f
            ?  pitchT * diveAcceleration
            :  pitchT * climbDeceleration;

        _currentSpeed = Mathf.Clamp(
            _currentSpeed + accel * dt,
            _baseSpeed * minSpeedFactor,
            _baseSpeed * maxSpeedFactor);

        // --- Pozisyon ---
        Vector3 pos = transform.position + transform.forward * _currentSpeed * dt;

        // Stall ekstra aşağı baskı: burun henüz dönmemişken düşüşe yardımcı olur
        if (stallFactor > 0f)
            pos.y -= stallDownForce * stallFactor * dt;

        // İrtifa hard tavanı
        pos.y = Mathf.Min(pos.y, maxAltitude);

        transform.position = pos;
    }

    void Glide()
    {
        float dt = Time.deltaTime;
        _currentSpeed = Mathf.Max(0f, _currentSpeed - glideDeceleration * dt);
        _glideVerticalSpeed += glideGravity * dt;   // ivmelenen düşüş

        Vector3 pos = transform.position;
        pos += transform.forward * _currentSpeed * dt;
        pos.y -= _glideVerticalSpeed * dt;
        transform.position = pos;
    }

    public void SetSpeed(float s)  { _baseSpeed = s; _currentSpeed = s; }
    public void StopFlying()       => _isFlying = false;
    public void StartGliding()     { _isFlying = false; _isGliding = true; }
}
