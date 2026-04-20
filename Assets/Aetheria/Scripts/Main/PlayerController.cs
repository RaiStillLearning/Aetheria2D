using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 12f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 2.5f;
    public float maxFallSpeed = -20f;

    [Header("Coyote and Buffer")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;

    [Header("Wall Jump")]
    public float wallJumpForce = 10f;
    public float wallJumpUpForce = 12f;
    public float wallJumpLockTime = 0.15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckDistance = 0.1f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private Rigidbody2D rb;
    private float hInput;
    private bool grounded;
    private bool wallLeft;
    private bool wallRight;
    private float coyoteCounter;
    private float jumpBuffer;
    private float wallLock;
    private bool jumping;

    static readonly int ASpeed    = Animator.StringToHash("Speed");
    static readonly int AGrounded = Animator.StringToHash("IsGrounded");
    static readonly int AJumping  = Animator.StringToHash("IsJumping");
    static readonly int AFalling  = Animator.StringToHash("IsFalling");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        hInput = 0f;
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) hInput = -1f;
        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) hInput =  1f;

        bool jumpPressed   = keyboard.spaceKey.wasPressedThisFrame
                          || keyboard.upArrowKey.wasPressedThisFrame
                          || keyboard.wKey.wasPressedThisFrame;
        bool jumpReleased  = keyboard.spaceKey.wasReleasedThisFrame
                          || keyboard.upArrowKey.wasReleasedThisFrame
                          || keyboard.wKey.wasReleasedThisFrame;

        if (jumpPressed) jumpBuffer = jumpBufferTime;
        if (jumpReleased && rb.linearVelocity.y > 0 && jumping)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.deltaTime;
        jumpBuffer -= Time.deltaTime;
        if (wallLock > 0f) wallLock -= Time.deltaTime;

        if (jumpBuffer > 0f)
        {
            if (coyoteCounter > 0f)
            { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); jumpBuffer = 0f; coyoteCounter = 0f; jumping = true; }
            else if (wallLeft && !grounded)
            { rb.linearVelocity = new Vector2(wallJumpForce, wallJumpUpForce); jumpBuffer = 0f; jumping = true; wallLock = wallJumpLockTime; }
            else if (wallRight && !grounded)
            { rb.linearVelocity = new Vector2(-wallJumpForce, wallJumpUpForce); jumpBuffer = 0f; jumping = true; wallLock = wallJumpLockTime; }
        }

        if (animator != null)
        {
            animator.SetFloat(ASpeed, Mathf.Abs(rb.linearVelocity.x));
            animator.SetBool(AGrounded, grounded);
            animator.SetBool(AJumping, rb.linearVelocity.y > 0.1f && !grounded);
            animator.SetBool(AFalling, rb.linearVelocity.y < -0.1f && !grounded);
        }
    }

    void FixedUpdate()
    {
        grounded = groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (grounded) jumping = false;
        wallLeft  = wallCheckLeft  && Physics2D.Raycast(wallCheckLeft.position,  Vector2.left,  wallCheckDistance, groundLayer);
        wallRight = wallCheckRight && Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, groundLayer);

        if (wallLock <= 0f)
        {
            float target = hInput * moveSpeed;
            float rate   = Mathf.Abs(target) > 0.01f ? acceleration : deceleration;
            rb.AddForce(new Vector2((target - rb.linearVelocity.x) * rate, 0f), ForceMode2D.Force);
            if (hInput != 0 && spriteRenderer) spriteRenderer.flipX = hInput < 0;
        }

        if (rb.linearVelocity.y < 0f)
        {
            float newY = Mathf.Max(rb.linearVelocity.y + Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime, maxFallSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newY);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius); }
        if (wallCheckLeft)  { Gizmos.color = Color.blue; Gizmos.DrawRay(wallCheckLeft.position,  Vector2.left  * wallCheckDistance); }
        if (wallCheckRight) { Gizmos.color = Color.blue; Gizmos.DrawRay(wallCheckRight.position, Vector2.right * wallCheckDistance); }
    }
}