using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyShooter : EnemyBase
{
    [Header("Shooting")]
    public Transform firePoint;                 // where bullets spawn
    public GameObject bulletPrefab;
    public float shotsPerSecond = 2f;           // fire speed
    public float bulletSpeed = 14f;
    public float detectionRange = 20f;

    [Header("Patrol (A ⇄ B)")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;                // movement speed along the path
    public float waitAtEnds = 0.25f;            // pause when reaching A/B
    public bool startAtA = true;                // which end to head from first
    public bool flipXOnMove = true;             // auto face travel direction

    Transform player;
    float cd;                                   // fire cooldown
    Vector3 target;
    float waitTimer;

    void Start()
    {
        // choose initial target end
        if (pointA && pointB)
            target = (startAtA ? pointB.position : pointA.position);
        else
            target = transform.position; // no waypoints assigned
    }

    void Update()
    {
        // ========== PATROL MOVE ==========
        if (pointA && pointB)
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
            }
            else
            {
                Vector3 next = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

                // optional sprite flip to face travel direction
                if (flipXOnMove)
                {
                    float dx = next.x - transform.position.x;
                    if (Mathf.Abs(dx) > 0.0001f)
                    {
                        var s = transform.localScale;
                        s.x = Mathf.Sign(dx) * Mathf.Abs(s.x);
                        transform.localScale = s;
                    }
                }

                transform.position = next;

                if (Vector2.Distance(transform.position, target) < 0.02f)
                {
                    // swap ends and wait a moment
                    target = (target == pointA.position) ? pointB.position : pointA.position;
                    waitTimer = waitAtEnds;
                }
            }
        }
        else if (moveSpeed != 0f)
        {
            // fallback: simple horizontal drift if no waypoints assigned
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }

        // ========== TARGET & FIRE ==========
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);
        cd -= Time.deltaTime;
        if (dist <= detectionRange && cd <= 0f)
        {
            ShootAt(player.position);
            cd = 1f / Mathf.Max(0.01f, shotsPerSecond);
        }
    }

    void ShootAt(Vector3 targetPos)
    {
        if (!bulletPrefab) return;
        Vector3 spawn = firePoint ? firePoint.position : transform.position;
        var go = Instantiate(bulletPrefab, spawn, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        Vector2 dir = (targetPos - spawn).normalized;
        if (proj) proj.Init(dir, true, bulletSpeed); // true = enemy bullet
    }

    // Kill player on touch (works whether colliders are triggers or not)
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player")) GameManager.Instance.KillPlayer();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) GameManager.Instance.KillPlayer();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (pointA && pointB)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.1f);
            Gizmos.DrawWireSphere(pointB.position, 0.1f);
        }
    }
#endif

    // --- (reset patrol + cooldown on respawn) ---
    public override void ResetForRespawn()
    {
        base.ResetForRespawn(); // restores transform to the original scene position

        if (pointA && pointB)
        {
            // re-seed the patrol target like Start()
            target = (startAtA ? pointB.position : pointA.position);
            waitTimer = 0f;
        }

        cd = 0f; // clear fire cooldown so it can shoot immediately again
    }
    
}
