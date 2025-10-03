using System.Collections.Generic;  // --- ADDED ---
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Refs")]
    public Transform player;             // drag Player
    public GameObject deathUI;           // drag Canvas with two buttons

    [Header("Respawn")]
    public float checkpointSpeedBonus = 2f; // added to player's moveSpeed on checkpoint respawn

    Vector3 initialSpawn;
    Vector3 checkpoint;
    bool hasCheckpoint = false;

    PlayerController2D pc;
    float baseMoveSpeed;

    // (track enemies to reset) ---
    List<EnemyBase> enemies = new List<EnemyBase>();

    
    List<SpikeHazardMover> spikes = new List<SpikeHazardMover>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        pc = player ? player.GetComponent<PlayerController2D>() : null;
        if (pc) baseMoveSpeed = pc.moveSpeed;

        initialSpawn = player ? player.position : Vector3.zero;

        //  (cache all enemies in scene, including inactive) ---
        enemies = new List<EnemyBase>(FindObjectsOfType<EnemyBase>(true));
    
        spikes = new List<SpikeHazardMover>(FindObjectsOfType<SpikeHazardMover>(true));

    }

    public void SetCheckpoint(Vector3 pos)
    {
        checkpoint = pos;
        hasCheckpoint = true;
    }

    public void KillPlayer()
    {
        // Try to auto-find if not assigned
        if (deathUI == null)
        {
            var found = GameObject.FindGameObjectWithTag("DeathUI");
            if (found) deathUI = found;
        }

        if (deathUI != null)
        {
            deathUI.SetActive(true); // 1) show UI first
            Time.timeScale = 0f;     // 2) THEN pause the game
        }
        else
        {
            Debug.LogWarning("Death UI not assigned. Restarting run instead.");
            RestartRun();            // fallback so you never get stuck frozen
        }
    }

    // UI Button -> “Checkpoint”
    public void RespawnAtCheckpoint()
    {
        if (!player) return;
        Vector3 spawn = hasCheckpoint ? checkpoint : initialSpawn;
        player.position = spawn;
        pc = player.GetComponent<PlayerController2D>();
        if (pc)
        {
            pc.HardSetVelocity(Vector2.zero);
            pc.moveSpeed = pc.moveSpeed + 2; // speed up after checkpoint (kept your behavior)
        }

        ResetAllEnemiesAndBullets();
       

        CloseDeathUI();
    }

    // UI Button -> “Restart”
    public void RestartRun()
    {
        if (!player) return;
        hasCheckpoint = false;
        player.position = initialSpawn;
        pc = player.GetComponent<PlayerController2D>();
        if (pc)
        {
            pc.HardSetVelocity(Vector2.zero);
            pc.moveSpeed = baseMoveSpeed; // reset speed
        }

        
        ResetAllEnemiesAndBullets();
        

        CloseDeathUI();
    }

    //  (reactivate & reset enemies, clear bullets) ---
    void ResetAllEnemiesAndBullets()
    {
        // Bring back every enemy we recorded at start
        foreach (var e in enemies)
        {
            if (e != null)
                e.ResetForRespawn();
        }

        // Remove any bullets currently in flight
        var allProjectiles = FindObjectsOfType<Projectile>();
        foreach (var p in allProjectiles)
        {
            if (p != null)
                Destroy(p.gameObject);
        }
        foreach (var s in spikes) if (s) s.ResetForRespawn();

    }
    

    void CloseDeathUI()
    {
        if (deathUI) deathUI.SetActive(false);
        Time.timeScale = 1f;
    }
}
