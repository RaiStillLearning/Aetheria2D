using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController - Vertical Platformer Cave Climber
/// Menggunakan New Input System (Unity 6).
/// Kontrol:
///   A/D atau Left/Right Arrow = jalan kiri-kanan
///   Space / W / Up Arrow     = lompat
///   Touch / Mouse click      = lompat (mobile/mouse)
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

    // Internal
    Rigidbody2D    _rb;
    SpriteRenderer _sr;
    Animator       _anim;
    bool  _isGrounded;
    float _moveInput;
    bool  _jumpConsumed;
    public bool isDead = false;

    // Input System
    Keyboard _kb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.gravityScale = 2.5f;

        _sr   = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();

        // Auto-find GroundCheck child jika belum di-assign
        if (groundCheck == null)
        {
            Transform gc = transform.Find("GroundCheck");
            if (gc != null) groundCheck = gc;
        }

        // Auto-set layer Ground jika belum di-set
        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground", "GhostPlatform");
    }

    void OnEnable()
    {
        _kb = Keyboard.current;
    }

    void Update()
    {
        if (isDead) return;

        // Refresh keyboard reference (bisa berubah)
        _kb = Keyboard.current;

        // Ground check
        Vector2 checkPos = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.5f;
        _isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);

        // Baca input horizontal
        _moveInput = 0f;
        if (_kb != null)
        {
            bool left  = _kb.aKey.isPressed || _kb.leftArrowKey.isPressed;
            bool right = _kb.dKey.isPressed || _kb.rightArrowKey.isPressed;
            if (right) _moveInput += 1f;
            if (left)  _moveInput -= 1f;
        }

        // Flip sprite
        if (_sr != null)
        {
            if      (_moveInput > 0f) _sr.flipX = false;
            else if (_moveInput < 0f) _sr.flipX = true;
        }

        // Jump input
        bool jumpPressed = false;
        if (_kb != null)
            jumpPressed = _kb.spaceKey.wasPressedThisFrame
                       || _kb.wKey.wasPressedThisFrame
                       || _kb.upArrowKey.wasPressedThisFrame;

        // Mouse / Touch click juga bisa lompat
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            jumpPressed = true;

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    jumpPressed = true;
        }

        if (jumpPressed && _isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _jumpConsumed = true;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Horizontal movement
        _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);

        // Better jump feel
        bool holdingJump = _kb != null && (_kb.spaceKey.isPressed || _kb.wKey.isPressed || _kb.upArrowKey.isPressed);

        if (_rb.linearVelocity.y < 0f)
        {
            // Jatuh lebih cepat
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                                * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (_rb.linearVelocity.y > 0f && !holdingJump)
        {
            // Tombol dilepas saat naik = arc pendek
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                                * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void UpdateAnimator()
    {
        if (_anim == null) return;
        foreach (var p in _anim.parameters)
        {
            switch (p.name)
            {
                case "Speed" when p.type == AnimatorControllerParameterType.Float:
                    _anim.SetFloat("Speed", Mathf.Abs(_moveInput)); break;
                case "isGrounded" when p.type == AnimatorControllerParameterType.Bool:
                    _anim.SetBool("isGrounded", _isGrounded); break;
                case "isJumping" when p.type == AnimatorControllerParameterType.Bool:
                    _anim.SetBool("isJumping", !_isGrounded && _rb.linearVelocity.y > 0f); break;
                case "isRunning" when p.type == AnimatorControllerParameterType.Bool:
                    _anim.SetBool("isRunning", _isGrounded && Mathf.Abs(_moveInput) > 0.1f); break;
                case "isFalling" when p.type == AnimatorControllerParameterType.Bool:
                    _anim.SetBool("isFalling", !_isGrounded && _rb.linearVelocity.y < 0f); break;
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        if (_anim != null)
            foreach (var p in _anim.parameters)
                if (p.name == "isDead" && p.type == AnimatorControllerParameterType.Bool)
                    _anim.SetBool("isDead", true);
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }

    public void ResetPlayer(Vector3 spawnPos)
    {
        transform.position = spawnPos;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = Vector2.zero;
        isDead = false;
        if (_anim != null)
            foreach (var p in _anim.parameters)
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
