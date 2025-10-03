using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 18f;
    public float lifeTime = 3f;
    public bool isEnemyBullet = false;     // who fired it
    public bool canHitGround = true;       // set false if you want to ignore ground

    Rigidbody2D rb;

    public void Init(Vector2 dir, bool enemyBullet, float overrideSpeed = -1f)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<Collider2D>().isTrigger = true;

        isEnemyBullet = enemyBullet;
        float spd = overrideSpeed > 0 ? overrideSpeed : speed;

        // (project uses linearVelocity; keep it as-is)
        rb.linearVelocity = dir.normalized * spd;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player hit by enemy bullet
        if (isEnemyBullet && other.CompareTag("Player"))
        {
            GameManager.Instance.KillPlayer();
            Destroy(gameObject);
            return;
        }

        // Enemy hit by player bullet
        if (!isEnemyBullet)
        {
            var enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy)
            {
                if (enemy.reflectBullets)
                {
                    // reflect: flip velocity, convert to enemy bullet
                    isEnemyBullet = true;
                    rb.linearVelocity = -rb.linearVelocity;
                    return;
                }
                else
                {
                    enemy.Die();
                    Destroy(gameObject);
                    return;
                }
            }
        }

        // Ground/platform cleanup
        if (canHitGround && other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            if (other.GetComponent<EnemyBase>() == null && !other.CompareTag("Player"))
                Destroy(gameObject);
        }
    }
}
