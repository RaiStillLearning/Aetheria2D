using UnityEngine;

/// <summary>
/// GhostPlatform - Karakter bisa menembus dari samping/bawah,
/// tapi tetap bisa berpijak dan lompat dari atas.
///
/// Cara pakai:
/// 1. Tambahkan script ini ke GameObject platform/obstacle yang ingin ditembus.
/// 2. Script ini otomatis mengubah layer object ke "GhostPlatform".
/// 3. Physics2D sudah dikonfigurasi: Player tidak bertabrakan dengan GhostPlatform
///    secara fisik (bisa tembus), tapi GroundCheck di PlayerController tetap
///    mendeteksinya sehingga karakter bisa berpijak & lompat dari atas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GhostPlatform : MonoBehaviour
{
    [Header("Ghost Platform Settings")]
    [Tooltip("Warna tint sprite saat menjadi ghost platform (opsional)")]
    public bool applyVisualTint = false;
    [Tooltip("Warna tint yang diterapkan ke SpriteRenderer")]
    public Color tintColor = new Color(1f, 1f, 1f, 0.6f);

    void Awake()
    {
        // Set layer ke GhostPlatform agar collision matrix berlaku
        int ghostLayer = LayerMask.NameToLayer("GhostPlatform");
        if (ghostLayer == -1)
        {
            Debug.LogError("[GhostPlatform] Layer 'GhostPlatform' tidak ditemukan! " +
                           "Pastikan layer sudah dibuat di Project Settings > Tags and Layers.");
            return;
        }

        gameObject.layer = ghostLayer;

        // Pastikan collider BUKAN trigger (harus solid agar GroundCheck detect)
        var col = GetComponent<Collider2D>();
        if (col.isTrigger)
        {
            col.isTrigger = false;
            Debug.LogWarning("[GhostPlatform] Collider2D di-set ke non-trigger agar GroundCheck bisa mendeteksi.");
        }

        // Visual tint opsional
        if (applyVisualTint)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = tintColor;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Tampilkan garis hijau putus-putus di sekeliling collider saat dipilih
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        UnityEditor.Handles.color = new Color(0f, 1f, 0.5f, 0.5f);
        var bounds = col.bounds;
        UnityEditor.Handles.DrawSolidRectangleWithOutline(
            new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y),
            new Color(0f, 1f, 0.5f, 0.08f),
            new Color(0f, 1f, 0.5f, 0.8f)
        );
    }
#endif
}
