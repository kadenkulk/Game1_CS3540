using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Common")]
    public bool reflectBullets = false;   // toggle in Inspector
    public int maxHP = 1;
    int hp;

    // ---(respawn support) ---
    protected Rigidbody2D rb;
    Vector3 _startPos;
    Quaternion _startRot;
    Vector3 _startScale;
    

    protected virtual void Awake()
    {
        hp = maxHP;

        // ---(cache start state for respawn) ---
        rb = GetComponent<Rigidbody2D>();
        _startPos = transform.position;
        _startRot = transform.rotation;
        _startScale = transform.localScale;
        
    }

    public virtual void Die()
    {
        // Destroy(gameObject);  // (original)
        // --- CHANGED (disable so we can respawn/reset) ---
        gameObject.SetActive(false);
    }

    protected void TouchKillPlayer(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
            GameManager.Instance.KillPlayer();
    }

    // ---(called by GameManager on respawn/restart) ---
    public virtual void ResetForRespawn()
    {
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(_startPos, _startRot);
        transform.localScale = _startScale;

        if (rb)
        {
            // clear any motion
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
            #else
            rb.velocity = Vector2.zero;
            #endif
            rb.angularVelocity = 0f;
            rb.WakeUp();
        }


    }
    
}
