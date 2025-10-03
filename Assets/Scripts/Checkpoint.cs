using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    bool activated;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.SetCheckpoint(transform.position);
        activated = true;
       
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.cyan;
    }
}
