using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    #region Events
    public event Action OnDeathAnimationComplete;
    public event Action OnExplosionTriggered;
    #endregion

    #region Inspector Fields
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
    
    [Header("Death Timing")]
    [SerializeField] private float preExplosionDelay = 0.15f;
    [SerializeField] private float postExplosionFadeDelay = 0.3f;
    [SerializeField] private float finalDestroyDelay = 2f;
    
    [Header("Death Debris (Optional)")]
    [SerializeField] private bool spawnDebris = false;
    [SerializeField] private BossDebrisConfig debrisConfig;
    
    [Header("Animation Parameters")]
    [SerializeField] private string deathTriggerName = "Death";
    #endregion

    #region Private Fields
    private bool isDead;
    private Coroutine currentDeathSequence;
    #endregion

    #region Constants
    private const float EXPLOSION_DESTROY_TIME = 3f;
    #endregion

    #region Unity Lifecycle
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
    #endregion

    #region Initialization
    private void ValidateAndCacheReferences()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError($"[BossAnimationController] No Animator found on {gameObject.name}", this);
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (health == null)
        {
            health = GetComponent<EnemyHealth>();
            if (health == null)
            {
                Debug.LogError($"[BossAnimationController] No EnemyHealth found on {gameObject.name}", this);
            }
        }

        if (controller == null)
        {
            controller = GetComponent<BossController>();
        }

        if (attackManager == null)
        {
            attackManager = GetComponent<BossAttackManager>();
        }
    }

    private void SubscribeToEvents()
    {
        if (health != null)
        {
            health.OnDeath += HandleBossDeath;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (health != null)
        {
            health.OnDeath -= HandleBossDeath;
        }
    }
    #endregion

    #region Death Handling
    private void HandleBossDeath()
    {
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

        yield return new WaitForSeconds(preExplosionDelay);

        SpawnExplosionEffect();
        OnExplosionTriggered?.Invoke();

        if (spawnDebris && debrisConfig != null)
        {
            SpawnDebrisEffects();
        }

        yield return new WaitForSeconds(postExplosionFadeDelay);

        HideBossSprite();

        yield return new WaitForSeconds(finalDestroyDelay);

        OnDeathAnimationComplete?.Invoke();
        DestroyBoss();
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
        if (controller != null)
        {
            controller.enabled = false;
        }

        if (attackManager != null)
        {
            attackManager.enabled = false;
        }
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
    #endregion

    #region Public API
    public void ForceStopDeathSequence()
    {
        if (currentDeathSequence != null)
        {
            StopCoroutine(currentDeathSequence);
            currentDeathSequence = null;
        }

        isDead = false;
    }

    public bool IsPlayingDeathSequence()
    {
        return isDead && currentDeathSequence != null;
    }
    #endregion

    #region Editor
    private void OnDrawGizmosSelected()
    {
        if (deathExplosionPrefab == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Vector3 explosionPos = transform.position + explosionOffset;
        Gizmos.DrawWireSphere(explosionPos, 0.5f * explosionScale);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, explosionPos);
    }
    #endregion
}

#region Supporting Classes
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
#endregion