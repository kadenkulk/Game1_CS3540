using UnityEngine;

public class SpringPad : MonoBehaviour
{
    public float bounceImpulse = 18f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null) return;

        pc.HardSetVelocity(new Vector2(pc.GetComponent<Rigidbody2D>().linearVelocity.x, 0f));
        pc.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounceImpulse, ForceMode2D.Impulse);
    }
}
