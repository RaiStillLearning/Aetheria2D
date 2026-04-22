using UnityEngine;

/// <summary>
/// CameraFollow - Vertical Platformer
/// Kamera mengikuti player ke atas dengan smooth.
/// Tidak turun saat player jatuh (agar player tidak lihat bawah).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float smoothSpeed = 6f;
    public float offsetX = 0f;
    public float offsetY = 2.5f;

    [Header("Vertical Lock")]
    [Tooltip("Kamera hanya naik, tidak turun (false = ikut naik turun)")]
    public bool onlyFollowUp = true;

    float _highestY;

    void Start()
    {
        if (target == null)
        {
            // Auto-find player
            var player = GameObject.Find("--- PLAYER ---");
            if (player != null) target = player.transform;
        }
        _highestY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offsetX;
        float targetY = target.position.y + offsetY;

        if (onlyFollowUp)
            targetY = Mathf.Max(targetY, _highestY);

        float newY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);
        float newX = Mathf.Lerp(transform.position.x, targetX, smoothSpeed * Time.deltaTime);

        transform.position = new Vector3(newX, newY, transform.position.z);

        if (transform.position.y > _highestY)
            _highestY = transform.position.y;
    }
}
