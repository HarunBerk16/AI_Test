using UnityEngine;

/// <summary>
/// Binanın tek bir bölümü (kat, kule vs.).
/// Yıkılınca görsel olarak çöker.
/// </summary>
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

    public void Destroy()
    {
        if (IsDestroyed) return;
        IsDestroyed = true;

        // Görsel: rengi koyulaştır (yıkık efekti)
        if (_renderer != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", new Color(0.2f, 0.1f, 0.05f));
            _renderer.SetPropertyBlock(block);
        }

        // Hafif aşağı düşür
        StartCoroutine(CollapseRoutine());
    }

    System.Collections.IEnumerator CollapseRoutine()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0, transform.localScale.y * 0.5f, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        if (_collider != null) _collider.enabled = false;
    }
}
