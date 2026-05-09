using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlaneController))]
public class PlaneImpact : MonoBehaviour
{
    private PlaneController        _controller;
    private bool                   _hasImpacted;
    private readonly List<GameObject> _separatedPieces = new();

    void Awake()
    {
        _controller = GetComponent<PlaneController>();
    }

    void OnEnable()  => GameStateManager.OnPhaseChanged += HandlePhaseChange;
    void OnDisable() => GameStateManager.OnPhaseChanged -= HandlePhaseChange;

    void HandlePhaseChange(GamePhase phase)
    {
        if (phase == GamePhase.Menu || phase == GamePhase.Flying)
        {
            _hasImpacted = false;
            RestorePlane();
        }
    }

    void RestorePlane()
    {
        // Debris klonlarını yok et
        foreach (var p in _separatedPieces)
            if (p != null) Destroy(p);
        _separatedPieces.Clear();

        // Gizlenen orijinal parçaları geri göster
        foreach (string n in new[] { "WingLeft", "WingRight", "Tail" })
        {
            var t = transform.Find(n);
            if (t != null) t.gameObject.SetActive(true);
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic     = true;
        }
    }

    public void OnChildTriggerEnter(Collider other)
    {
        if (_hasImpacted) return;

        if (other.gameObject.name == "Ground")
        {
            _hasImpacted = true;
            _controller.StopFlying();
            BreakApart();
            GameEvents.OnPlaneCrash?.Invoke();
            return;
        }

        if (other.GetComponentInParent<TargetBuilding>() != null)
        {
            _hasImpacted = true;
            _controller.StopFlying();
            BreakApart();
            GameEvents.OnPlaneImpact?.Invoke(transform.position);
            return;
        }

        var obstacle = other.GetComponentInParent<EnvironmentObstacle>();
        if (obstacle != null)
        {
            _hasImpacted = true;
            _controller.StopFlying();
            BreakApart();
            obstacle.GetHit(transform.forward);
            GameEvents.OnPlaneCrash?.Invoke();
        }
    }

    public void NotifyGroundCrash()
    {
        if (_hasImpacted) return;
        _hasImpacted = true;
        _controller.StopFlying();
        BreakApart();
        GameEvents.OnPlaneCrash?.Invoke();
    }

    void BreakApart()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic            = false;
        rb.useGravity             = true;
        rb.linearDamping          = 1.5f;   // hızlı yavaşlama → sürüklenme etkisi
        rb.angularDamping         = 8f;     // takla atmaması için yüksek rotasyon sönümü
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Sadece ileri + hafif aşağı itki — takla yok
        Vector3 slideDir = Vector3.Lerp(transform.forward, Vector3.forward, 0.5f);
        rb.AddForce(slideDir * 4f - Vector3.up * 1.5f, ForceMode.Impulse);
        // Minimal spin — doğal görünsün ama gerçek dışı olmasın
        rb.AddTorque(Random.insideUnitSphere * 0.8f, ForceMode.Impulse);

        // Yerden geçmemesi için pozisyon düzeltici başlat
        _groundClampActive = true;

        SeparatePiece("WingLeft");
        SeparatePiece("WingRight");
        SeparatePiece("Tail");
    }

    bool _groundClampActive;
    const float GROUND_Y = 0.3f;

    void LateUpdate()
    {
        if (!_groundClampActive) return;
        var rb = GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic) { _groundClampActive = false; return; }

        // Yerden aşağı gitmeyi engelle
        if (transform.position.y < GROUND_Y)
        {
            var pos = transform.position;
            pos.y = GROUND_Y;
            transform.position = pos;

            // Dikey hızı kes, yatay hızı sürtünmeyle azalt
            var vel = rb.linearVelocity;
            vel.y = Mathf.Max(vel.y, 0f);
            vel.x *= 0.85f;
            vel.z *= 0.85f;
            rb.linearVelocity = vel;

            // Rotasyonu ground'a yakın sabit tut (devrilmesin)
            var euler = transform.eulerAngles;
            euler.x = Mathf.LerpAngle(euler.x, 0f, 0.15f);
            euler.z = Mathf.LerpAngle(euler.z, 0f, 0.15f);
            transform.eulerAngles = euler;
        }

        // Durunca temizle
        if (rb.linearVelocity.magnitude < 0.05f && rb.angularVelocity.magnitude < 0.05f)
            _groundClampActive = false;
    }

    void SeparatePiece(string pieceName)
    {
        var piece = transform.Find(pieceName);
        if (piece == null) return;

        // Orijinali gizle — resetlenince tekrar gösterilecek
        piece.gameObject.SetActive(false);

        // Fizik debris için klon oluştur
        var clone = Instantiate(piece.gameObject, piece.position, piece.rotation);
        clone.SetActive(true);
        _separatedPieces.Add(clone);

        var rb = clone.GetComponent<Rigidbody>();
        if (rb == null) rb = clone.AddComponent<Rigidbody>();
        rb.isKinematic            = false;
        rb.useGravity             = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Vector3 dir = (transform.forward + Random.insideUnitSphere * 0.8f).normalized;
        rb.AddForce(dir * Random.Range(4f, 8f), ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * Random.Range(3f, 7f), ForceMode.Impulse);
    }
}
