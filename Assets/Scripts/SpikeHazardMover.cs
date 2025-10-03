using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeHazardMover : MonoBehaviour
{
    [Header("Hazard")]
    public bool killOnTouch = true;          // kills player on collision/trigger
    public bool colliderIsTrigger = false;   // set true if you prefer triggers

    [Header("Path")]
    public Transform[] points;               // set 2+ waypoints in order
    public float speed = 3f;                 // movement speed
    public float waitAtPoint = 0.2f;         // default wait at each point
    public float[] waitOverrides;            // optional per-point waits
    public bool pingPong = true;             // A→B→C→B→A… (true) or loop A→B→C→A (false)
    public int startIndex = 0;               // starting point index (0..N-1)

    [Header("Motion")]
    public bool useRigidbodyIfPresent = true;  // if RB2D exists, MovePosition
    public bool flipXOnMove = false;           // flip localScale.x to face travel dir

    Rigidbody2D rb;
    int current;
    int dir = 1;
    float waitTimer;

    // For respawn reset (optional integration with your GameManager)
    Vector3 _startPos;
    Quaternion _startRot;
    Vector3 _startScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = colliderIsTrigger;

        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        _startPos = transform.position;
        _startRot = transform.rotation;
        _startScale = transform.localScale;

        if (points != null && points.Length > 0)
        {
            current = Mathf.Clamp(startIndex, 0, points.Length - 1);
            transform.position = points[current].position;
            dir = 1;
            waitTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        float dt = rb && useRigidbodyIfPresent ? Time.fixedDeltaTime : Time.deltaTime;
        Tick(dt);
    }

    void Tick(float dt)
    {
        if (points == null || points.Length == 0) return;

        if (waitTimer > 0f)
        {
            waitTimer -= dt;
            return;
        }

        Vector3 targetPos = points[current].position;
        Vector3 next = Vector3.MoveTowards(transform.position, targetPos, speed * dt);

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

        if (rb && useRigidbodyIfPresent) rb.MovePosition(next);
        else transform.position = next;

        if (Vector2.Distance(transform.position, targetPos) < 0.02f)
        {
            // reached point → set wait, then advance index
            float w = waitAtPoint;
            if (waitOverrides != null &&
                current >= 0 && current < waitOverrides.Length &&
                waitOverrides[current] > 0f)
            {
                w = waitOverrides[current];
            }
            waitTimer = w;
            AdvanceIndex();
        }
    }

    void AdvanceIndex()
    {
        if (points.Length <= 1) return;

        if (pingPong)
        {
            int nextIdx = current + dir;
            if (nextIdx < 0 || nextIdx >= points.Length)
            {
                dir *= -1;
                nextIdx = current + dir;
            }
            current = nextIdx;
        }
        else
        {
            current = (current + 1) % points.Length;
        }
    }

    // Kill player on either collision type
    void OnCollisionEnter2D(Collision2D col)
    {
        if (killOnTouch && col.collider.CompareTag("Player"))
            GameManager.Instance.KillPlayer();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (killOnTouch && other.CompareTag("Player"))
            GameManager.Instance.KillPlayer();
    }

    //  called from GameManager if you want spikes to reset on respawn
    public void ResetForRespawn()
    {
        transform.SetPositionAndRotation(_startPos, _startRot);
        transform.localScale = _startScale;
        if (rb)
        {
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
            #else
            rb.velocity = Vector2.zero;
            #endif
            rb.angularVelocity = 0f;
            rb.WakeUp();
        }
        current = Mathf.Clamp(startIndex, 0, (points != null ? points.Length - 1 : 0));
        dir = 1;
        waitTimer = 0f;
        if (points != null && points.Length > 0)
            transform.position = points[current].position;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (points == null || points.Length == 0) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i])
            {
                Gizmos.DrawWireSphere(points[i].position, 0.1f);
                if (i + 1 < points.Length && points[i + 1])
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
        if (!pingPong && points.Length > 1 && points[0] && points[points.Length - 1])
            Gizmos.DrawLine(points[points.Length - 1].position, points[0].position);
    }
#endif
}
