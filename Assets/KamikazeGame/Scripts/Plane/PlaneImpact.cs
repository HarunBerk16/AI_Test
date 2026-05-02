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
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(transform.forward * 3f - Vector3.up * 2f, ForceMode.Impulse);
        rb.AddTorque(transform.right * 5f + Random.insideUnitSphere * 2f, ForceMode.Impulse);

        SeparatePiece("WingLeft");
        SeparatePiece("WingRight");
        SeparatePiece("Tail");
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
