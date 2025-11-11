using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class MinionController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float flySpeed = 8f;
    
    [Header("Attack")]
    [SerializeField] float minAttackCooldown = 10f;
    [SerializeField] float maxAttackCooldown = 20f;
    [SerializeField] int maxActiveBullets = 2;
    
    [Header("Projectile")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float customBulletSpeed = 5f;
    [SerializeField] bool useCustomSpeed = false;
    
    Vector3 targetPosition;
    Transform playerTransform;
    Rigidbody2D rb;
    EnemyHealth health;
    
    bool hasReachedTarget;
    float attackTimer;
    float currentCooldown;
    List<GameObject> activeBullets;

    void Awake()
    {
        activeBullets = new List<GameObject>(maxActiveBullets);
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
        
        FindPlayer();
        InitializeAttackTimer();
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
        
        DestroyActiveBullets();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogWarning($"[MinionController] Player not found for {gameObject.name}");
    }

    void InitializeAttackTimer()
    {
        currentCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
        attackTimer = Random.Range(0f, currentCooldown * 0.5f);
    }

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        hasReachedTarget = false;
    }

    void Update()
    {
        if (health != null && !health.IsAlive) return;
        
        if (!hasReachedTarget)
        {
            MoveToTarget();
        }
        else
        {
            MaintainPosition();
            UpdateAttackTimer();
        }
        
        CleanupDestroyedBullets();
    }

    void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            flySpeed * Time.deltaTime
        );
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            hasReachedTarget = true;
            rb.linearVelocity = Vector2.zero;
            transform.position = targetPosition;
        }
    }

    void MaintainPosition()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = targetPosition;
    }

    void UpdateAttackTimer()
    {
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= currentCooldown && CanShoot())
        {
            ExecuteShoot();
            ResetAttackTimer();
        }
    }

    bool CanShoot()
    {
        return activeBullets.Count < maxActiveBullets 
            && playerTransform != null 
            && bulletPrefab != null;
    }

    void ExecuteShoot()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        GameObject bullet = null;
        
        if (BulletPool.Instance != null)
        {
            bullet = BulletPool.Instance.Get(transform.position, Quaternion.identity);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Debug.LogWarning("[MinionController] BulletPool not found, using Instantiate fallback");
        }
        
        if (bullet == null)
        {
            Debug.LogError($"[MinionController] Failed to get bullet on {gameObject.name}");
            return;
        }
        
        ProjectileController projectile = bullet.GetComponent<ProjectileController>();
        if (projectile != null)
        {
            if (useCustomSpeed)
                projectile.Launch(direction, customBulletSpeed);
            else
                projectile.Launch(direction);
        }
        else
        {
            Debug.LogError($"[MinionController] Bullet prefab missing ProjectileController on {gameObject.name}");
            
            if (BulletPool.Instance != null)
                BulletPool.Instance.Return(bullet);
            else
                Destroy(bullet);
            
            return;
        }
        
        activeBullets.Add(bullet);
    }

    void ResetAttackTimer()
    {
        attackTimer = 0f;
        currentCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
    }

    void CleanupDestroyedBullets()
    {
        activeBullets.RemoveAll(bullet => bullet == null);
    }

    void DestroyActiveBullets()
    {
        foreach (GameObject bullet in activeBullets)
        {
            if (bullet != null)
            {
                if (BulletPool.Instance != null)
                    BulletPool.Instance.Return(bullet);
                else
                    Destroy(bullet);
            }
        }
        activeBullets.Clear();
    }

    void HandleDeath()
    {
        DestroyActiveBullets();
    }
}