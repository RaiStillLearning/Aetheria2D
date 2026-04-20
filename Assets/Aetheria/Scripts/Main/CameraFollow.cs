using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public float smoothSpeed = 6f;
    public Vector2 offset = new Vector2(0f, 1.5f);

    [Header("Look Ahead")]
    public float lookAheadDistance = 1.5f;
    public float lookAheadSpeed = 4f;

    [Header("Dead Zone")]
    public float deadZoneX = 0.1f;
    public float deadZoneY = 0.05f;

    [Header("Bounds (optional)")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private float _z;
    private Vector3 _currentVelocity;
    private float _lookAheadX;
    private float _prevTargetX;

    void Awake()
    {
        _z = transform.position.z;
        if (target == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) target = pc.transform;
        }
        if (target != null)
        {
            _prevTargetX = target.position.x;
            transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, _z);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        float deltaX = target.position.x - _prevTargetX;
        _prevTargetX = target.position.x;
        _lookAheadX = Mathf.Lerp(_lookAheadX, Mathf.Sign(deltaX) * lookAheadDistance * (Mathf.Abs(deltaX) > 0.01f ? 1f : 0f), Time.deltaTime * lookAheadSpeed);

        Vector3 desired = new Vector3(
            target.position.x + offset.x + _lookAheadX,
            target.position.y + offset.y,
            _z
        );

        float dx = desired.x - transform.position.x;
        float dy = desired.y - transform.position.y;
        if (Mathf.Abs(dx) < deadZoneX) desired.x = transform.position.x;
        if (Mathf.Abs(dy) < deadZoneY) desired.y = transform.position.y;

        Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _currentVelocity, 1f / smoothSpeed);

        if (useBounds)
        {
            smoothed.x = Mathf.Clamp(smoothed.x, minBounds.x, maxBounds.x);
            smoothed.y = Mathf.Clamp(smoothed.y, minBounds.y, maxBounds.y);
        }

        smoothed.z = _z;
        transform.position = smoothed;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
        Vector3 size   = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}