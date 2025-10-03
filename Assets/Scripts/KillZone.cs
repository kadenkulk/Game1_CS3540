using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.KillPlayer(); // shows death UI where you pick Checkpoint or Restart
        }
    }
}
