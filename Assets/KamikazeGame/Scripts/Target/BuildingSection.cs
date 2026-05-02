using UnityEngine;

public class BuildingSection : MonoBehaviour
{
    public bool IsDestroyed { get; private set; }

    private Renderer _renderer;
    private Collider _collider;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
    }

    // Doğrudan patlama yarıçapında: güçlü itme + koyu hasar rengi
    public void Destroy(Vector3 explosionCenter, float blastForce)
    {
        if (IsDestroyed) return;
        MarkDestroyed(new Color(0.18f, 0.09f, 0.04f));

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.AddExplosionForce(blastForce, explosionCenter, 15f, 1.5f, ForceMode.Impulse);
        }

        Invoke(nameof(DisableCollider), 4f);
    }

    // Dalga kuvveti: daha zayıf itme, hafif hasar rengi + yıkılmış sayılır
    public void ApplyForce(Vector3 explosionCenter, float blastForce)
    {
        if (IsDestroyed) return;
        MarkDestroyed(new Color(0.28f, 0.16f, 0.08f));

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.AddExplosionForce(blastForce, explosionCenter, 15f, 0.5f, ForceMode.Impulse);
        }

        Invoke(nameof(DisableCollider), 5f);
    }

    void MarkDestroyed(Color damagedColor)
    {
        IsDestroyed = true;
        if (_renderer != null)
            _renderer.material.color = damagedColor;
    }

    void DisableCollider()
    {
        if (_collider != null) _collider.enabled = false;
    }
}
