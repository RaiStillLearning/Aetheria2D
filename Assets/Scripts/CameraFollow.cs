using UnityEngine;

/// <summary>
/// CameraFollow - smooth follow untuk vertical platformer.
/// Karakter selalu terlihat full, kamera smooth naik-turun.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset dari target")]
    public float offsetX = 0f;
    [Tooltip("Seberapa jauh kamera di atas karakter (world units)")]
    public float offsetY = 2.0f;

    [Header("Smooth Speed")]
    [Range(1f, 20f)] public float smoothY = 8f;
    [Range(1f, 20f)] public float smoothX = 5f;

    [Header("Dead Zone (berhenti follow kalau dalam zona ini)")]
    public float deadZoneX = 0.5f;
    public float deadZoneY = 0.3f;

    [Header("Clamp (batas kamera tidak bisa lebih rendah dari sini)")]
    public bool  useLowerBound = true;
    public float lowerBoundY   = -4f;

    // Target posisi yang diinginkan kamera
    Vector3 _velocity = Vector3.zero;

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.Find("--- PLAYER ---");
            if (p != null) target = p.transform;
        }

        // Snap kamera langsung ke posisi target di awal (tidak slide dari 0,0)
        if (target != null)
        {
            Vector3 snap = new Vector3(
                target.position.x + offsetX,
                target.position.y + offsetY,
                transform.position.z);
            transform.position = snap;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float desiredX = target.position.x + offsetX;
        float desiredY = target.position.y + offsetY;

        // Clamp batas bawah
        if (useLowerBound)
            desiredY = Mathf.Max(desiredY, lowerBoundY);

        float currentX = transform.position.x;
        float currentY = transform.position.y;

        // Dead zone - jangan gerak kalau sudah cukup dekat
        float newX = currentX;
        float newY = currentY;

        if (Mathf.Abs(desiredX - currentX) > deadZoneX)
            newX = Mathf.Lerp(currentX, desiredX, smoothX * Time.deltaTime);

        if (Mathf.Abs(desiredY - currentY) > deadZoneY)
            newY = Mathf.Lerp(currentY, desiredY, smoothY * Time.deltaTime);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}
