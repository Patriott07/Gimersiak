using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    public event Action OnDeathAnimationComplete;
    public event Action OnExplosionTriggered;

    [Header("Core References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private BossController controller;
    [SerializeField] private BossAttackManager attackManager;
    
    [Header("Death Visual Effects")]
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private float explosionScale = 1f;
    [SerializeField] private Vector3 explosionOffset = Vector3.zero;
    
    [Header("Death Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] [Range(0f, 1f)] private float explosionVolume = 1f;
    
    [Header("Death Timing")]
    [SerializeField] private bool useAnimationEvent = false;
    [SerializeField] private float preExplosionDelay = 0f;
    [SerializeField] private float postExplosionFadeDelay = 0.3f;
    [SerializeField] private float finalDestroyDelay = 2f;
    
    [Header("Death Debris (Optional)")]
    [SerializeField] private bool spawnDebris = false;
    [SerializeField] private BossDebrisConfig debrisConfig;
    
    [Header("Animation Parameters")]
    [SerializeField] private string deathTriggerName = "Death";

    private bool isDead;
    private bool explosionSpawned;
    private Coroutine currentDeathSequence;
    private const float EXPLOSION_DESTROY_TIME = 3f;

    private void Awake()
    {
        ValidateAndCacheReferences();
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        if (currentDeathSequence != null)
        {
            StopCoroutine(currentDeathSequence);
        }
    }

    private void ValidateAndCacheReferences()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (health == null) health = GetComponent<EnemyHealth>();
        if (controller == null) controller = GetComponent<BossController>();
        if (attackManager == null) attackManager = GetComponent<BossAttackManager>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void SubscribeToEvents()
    {
        if (health != null)
        {
            health.OnNearDeath += HandleBossNearDeath;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (health != null)
        {
            health.OnNearDeath -= HandleBossNearDeath;
        }
    }

    private void HandleBossNearDeath()
    {
        if (BossSceneManager.Instance != null)
        {
            BossSceneManager.Instance.TriggerBossDeathSequence();
            return;
        }

        if (isDead) return;
        isDead = true;
        currentDeathSequence = StartCoroutine(ExecuteDeathSequence());
    }

    private IEnumerator ExecuteDeathSequence()
    {
        TriggerDeathAnimation();
        DisableBossComponents();
        DisablePhysics();
        DisableCollisions();

        if (useAnimationEvent)
        {
            while (!explosionSpawned)
            {
                yield return null;
            }
        }
        else
        {
            if (preExplosionDelay > 0f)
            {
                yield return new WaitForSeconds(preExplosionDelay);
            }
            
            SpawnExplosionEffect();
            PlayExplosionSound();
            OnExplosionTriggered?.Invoke();

            if (spawnDebris && debrisConfig != null)
            {
                SpawnDebrisEffects();
            }
        }

        yield return new WaitForSeconds(postExplosionFadeDelay);

        if (health != null)
        {
            health.ForceBossDeath();
        }

        HideBossSprite();

        yield return new WaitForSeconds(finalDestroyDelay);

        OnDeathAnimationComplete?.Invoke();
        DestroyBoss();
    }

    public void TriggerExplosion()
    {
        if (explosionSpawned) return;
        
        explosionSpawned = true;
        SpawnExplosionEffect();
        PlayExplosionSound();
        OnExplosionTriggered?.Invoke();

        if (spawnDebris && debrisConfig != null)
        {
            SpawnDebrisEffects();
        }
    }

    private void TriggerDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerName);
        }
    }

    private void DisableBossComponents()
    {
        if (controller != null) controller.enabled = false;
        if (attackManager != null) attackManager.enabled = false;
    }

    private void DisablePhysics()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }
    }

    private void DisableCollisions()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }

    private void SpawnExplosionEffect()
    {
        if (deathExplosionPrefab == null) return;

        Vector3 spawnPosition = transform.position + explosionOffset;
        GameObject explosion = Instantiate(deathExplosionPrefab, spawnPosition, Quaternion.identity);
        
        if (explosionScale != 1f)
        {
            explosion.transform.localScale = Vector3.one * explosionScale;
        }

        Destroy(explosion, EXPLOSION_DESTROY_TIME);
    }

    private void PlayExplosionSound()
    {
        if (explosionSound == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound, explosionVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }
    }

    private void SpawnDebrisEffects()
    {
        if (debrisConfig == null) return;

        BossDebrisSpawner.SpawnDebris(
            transform.position,
            debrisConfig,
            spriteRenderer
        );
    }

    private void HideBossSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void DestroyBoss()
    {
        Destroy(gameObject);
    }

    public void ForceStopDeathSequence()
    {
        if (currentDeathSequence != null)
        {
            StopCoroutine(currentDeathSequence);
            currentDeathSequence = null;
        }

        isDead = false;
        explosionSpawned = false;
    }

    public void ExecuteDeathWithoutDialog()
    {
        if (isDead) return;
        
        isDead = true;
        currentDeathSequence = StartCoroutine(ExecuteDeathSequence());
    }

    public bool IsPlayingDeathSequence()
    {
        return isDead && currentDeathSequence != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (deathExplosionPrefab == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Vector3 explosionPos = transform.position + explosionOffset;
        Gizmos.DrawWireSphere(explosionPos, 0.5f * explosionScale);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, explosionPos);
    }
}

[Serializable]
public class BossDebrisConfig
{
    [Header("Debris Prefabs")]
    public GameObject leftDebrisPrefab;
    public GameObject rightDebrisPrefab;
    
    [Header("Physics Settings")]
    public float horizontalForce = 5f;
    public float verticalForce = 3f;
    public float torqueForce = 200f;
    public float gravityScale = 1f;
    
    [Header("Timing")]
    public float debrisLifetime = 3f;
    
    public bool HasValidPrefabs()
    {
        return leftDebrisPrefab != null || rightDebrisPrefab != null;
    }
}

public static class BossDebrisSpawner
{
    public static void SpawnDebris(Vector3 spawnPosition, BossDebrisConfig config, SpriteRenderer originalRenderer)
    {
        if (config == null || !config.HasValidPrefabs()) return;

        if (config.leftDebrisPrefab != null)
        {
            SpawnDebrisPiece(
                config.leftDebrisPrefab,
                spawnPosition,
                new Vector2(-config.horizontalForce, config.verticalForce),
                -config.torqueForce,
                config,
                originalRenderer
            );
        }

        if (config.rightDebrisPrefab != null)
        {
            SpawnDebrisPiece(
                config.rightDebrisPrefab,
                spawnPosition,
                new Vector2(config.horizontalForce, config.verticalForce),
                config.torqueForce,
                config,
                originalRenderer
            );
        }
    }

    private static void SpawnDebrisPiece(
        GameObject prefab,
        Vector3 position,
        Vector2 velocity,
        float torque,
        BossDebrisConfig config,
        SpriteRenderer originalRenderer)
    {
        GameObject debris = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);

        Rigidbody2D rb = debris.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = debris.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = config.gravityScale;
        rb.linearVelocity = velocity;
        rb.angularVelocity = torque;

        if (originalRenderer != null)
        {
            SpriteRenderer debrisSr = debris.GetComponent<SpriteRenderer>();
            if (debrisSr != null)
            {
                debrisSr.sortingLayerName = originalRenderer.sortingLayerName;
                debrisSr.sortingOrder = originalRenderer.sortingOrder;
            }
        }

        UnityEngine.Object.Destroy(debris, config.debrisLifetime);
    }
}