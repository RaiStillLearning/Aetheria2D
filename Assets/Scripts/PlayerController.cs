using UnityEngine;

/// <summary>
/// PlayerController - Vertical Platformer Cave Climber
/// A/D atau Left/Right = jalan kiri-kanan
/// Space / W / Up / Click = lompat
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float fallMultiplier = 2.8f;
    public float lowJumpMultiplier = 2.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.18f;
    public LayerMask groundLayer;

    // Private state
    Rigidbody2D _rb;
    SpriteRenderer _sr;
    Animator _anim;
    bool _isGrounded;
    float _moveInput;
    bool _wasGrounded;
    public bool isDead = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.gravityScale = 2.5f;

        _sr   = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();

        if (groundCheck == null)
        {
            var gc = transform.Find("GroundCheck");
            if (gc != null) groundCheck = gc;
        }

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (isDead) return;

        _moveInput = Input.GetAxisRaw("Horizontal");

        Vector2 checkPos = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.5f;

        _wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);

        bool jumpPressed = Input.GetButtonDown("Jump")
                        || Input.GetKeyDown(KeyCode.W)
                        || Input.GetKeyDown(KeyCode.UpArrow)
                        || Input.GetMouseButtonDown(0);

        if (jumpPressed && _isGrounded)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

        if (_sr != null)
        {
            if (_moveInput > 0f) _sr.flipX = false;
            else if (_moveInput < 0f) _sr.flipX = true;
        }

        RefreshAnimator();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);

        if (_rb.linearVelocity.y < 0f)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (_rb.linearVelocity.y > 0f && !Input.GetButton("Jump"))
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    void RefreshAnimator()
    {
        if (_anim == null) return;
        foreach (var p in _anim.parameters)
        {
            if (p.name == "Speed" && p.type == AnimatorControllerParameterType.Float)
                _anim.SetFloat("Speed", Mathf.Abs(_moveInput));
            if (p.name == "isGrounded" && p.type == AnimatorControllerParameterType.Bool)
                _anim.SetBool("isGrounded", _isGrounded);
            if (p.name == "isJumping" && p.type == AnimatorControllerParameterType.Bool)
                _anim.SetBool("isJumping", !_isGrounded);
            if (p.name == "isRunning" && p.type == AnimatorControllerParameterType.Bool)
                _anim.SetBool("isRunning", _isGrounded && Mathf.Abs(_moveInput) > 0.1f);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        if (_anim != null) foreach (var p in _anim.parameters)
            if (p.name == "isDead" && p.type == AnimatorControllerParameterType.Bool)
                _anim.SetBool("isDead", true);
        if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
    }

    public void ResetPlayer(Vector3 pos)
    {
        transform.position = pos;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = Vector2.zero;
        isDead = false;
        if (_anim != null) foreach (var p in _anim.parameters)
            if (p.name == "isDead" && p.type == AnimatorControllerParameterType.Bool)
                _anim.SetBool("isDead", false);
    }

    void OnDrawGizmosSelected()
    {
        Vector2 pos = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.5f;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
}
