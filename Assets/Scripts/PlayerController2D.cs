using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 12f;
    public float acceleration = 90f;
    public float deceleration = 140f;
    [Range(0.1f, 1.5f)] public float velPower = 1.0f;

    [Header("Jump")]
    public float jumpImpulse = 16f;
    public float coyoteTime = 0.10f;
    public float jumpBuffer = 0.10f;
    public float fallMultiplier = 2.0f;
    public float lowJumpMultiplier = 2.0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.24f;
    public LayerMask groundMask;

    // --- NEW: Shooting ---
    [Header("Shooting")]
    public Transform firePoint;                 // create as a child at hand height
    public GameObject bulletPrefab;            // assign the Bullet prefab
    public float bulletSpeed = 18f;
    public float shotsPerSecond = 6f;          // fire speed
    float fireCooldown;

    Rigidbody2D rb;
    float lastGroundedTime, lastJumpPressedTime;
    bool isJumpHeld;
    int facing = 1;                             // 1 right, -1 left

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // --- Input ---
        float x = Input.GetAxisRaw("Horizontal");
        isJumpHeld = Input.GetButton("Jump");
        if (Input.GetButtonDown("Jump")) lastJumpPressedTime = jumpBuffer;

        // face where you move
        if (x != 0) facing = x > 0 ? 1 : -1;
        // flip sprite here

        // Ground check
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        if (grounded) lastGroundedTime = coyoteTime;

        // Jump consume
        if (lastGroundedTime > 0f && lastJumpPressedTime > 0f) DoJump();

        // Horizontal move (force-based)
        float targetSpeed = x * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);

        // Better jump feel
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * rb.gravityScale) * (fallMultiplier - 1f) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0f && !isJumpHeld)
            rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * rb.gravityScale) * (lowJumpMultiplier - 1f) * Time.deltaTime;

        // --- Shooting on F key with fire rate ---
        fireCooldown -= Time.deltaTime;
        if (Input.GetKey(KeyCode.F) && fireCooldown <= 0f) {
            Fire();
            fireCooldown = 1f / Mathf.Max(0.01f, shotsPerSecond);
        }

        // timers
        lastGroundedTime -= Time.deltaTime;
        lastJumpPressedTime -= Time.deltaTime;
    }

    void DoJump()
    {
        lastGroundedTime = 0f; lastJumpPressedTime = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
    }

    void Fire()
    {
        if (!bulletPrefab) return;
        Vector3 spawnPos = firePoint ? firePoint.position : (transform.position + new Vector3(0.3f * facing, 0.6f, 0f));
        var go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        if (proj) proj.Init(new Vector2(facing, 0f), false, bulletSpeed); // false = player bullet
    }

    public void HardSetVelocity(Vector2 v) => rb.linearVelocity = v;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius); }
        if (firePoint) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(firePoint.position, 0.05f); }
    }
#endif
}
