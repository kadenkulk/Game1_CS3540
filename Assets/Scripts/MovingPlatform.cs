using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 3f;
    public float waitAtEnds = 0.5f;

    Rigidbody2D rb;
    Vector3 target;
    float waitTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        target = pointB != null ? pointB.position : transform.position;
    }

    void FixedUpdate()
    {
        if (pointA == null || pointB == null) return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 next = Vector2.MoveTowards(transform.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (Vector2.Distance(transform.position, target) < 0.02f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
            waitTimer = waitAtEnds;
        }
    }
}
