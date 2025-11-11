using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class MinionController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float flySpeed = 8f;
    [SerializeField] float swaySpeed = 2f;
    [SerializeField] float swayAmount = 1.5f;
    [SerializeField] float swayRandomness = 0.3f;

    [Header("Collision Avoidance")]
    [SerializeField] float avoidanceRadius = 1.2f;
    [SerializeField] float avoidanceForce = 3f;
    [SerializeField] LayerMask minionLayer;

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

    float swayOffset;
    float swayTimer;
    Vector3 basePosition;

    void Awake()
    {
        activeBullets = new List<GameObject>(maxActiveBullets);
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        swayOffset = Random.Range(0f, Mathf.PI * 2f);
        swayTimer = swayOffset;
    }

    void Start()
    {
        if (health != null)
        {
            health.OnDeath += HandleDeath;
            health.OnDamageTaken += HandleHit;
        }

        FindPlayer();
        InitializeAttackTimer();
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
            health.OnDamageTaken -= HandleHit;
        }

        DestroyActiveBullets();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void InitializeAttackTimer()
    {
        currentCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
        attackTimer = Random.Range(0f, currentCooldown * 0.5f);
    }

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
        basePosition = target;
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
            SwayMovement();
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
            basePosition = targetPosition;
        }
    }

    void SwayMovement()
    {
        swayTimer += Time.deltaTime * swaySpeed;
        
        float randomizedAmount = swayAmount * (1f + Random.Range(-swayRandomness, swayRandomness));
        float swayX = Mathf.Sin(swayTimer) * randomizedAmount;
        
        Vector3 swayPosition = basePosition + new Vector3(swayX, 0f, 0f);
        
        Vector3 avoidanceVector = CalculateAvoidance();
        swayPosition += avoidanceVector;
        
        transform.position = Vector3.Lerp(
            transform.position, 
            swayPosition, 
            Time.deltaTime * 5f
        );
        
        rb.linearVelocity = Vector2.zero;
    }

    Vector3 CalculateAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Collider2D[] nearbyMinions = Physics2D.OverlapCircleAll(
            transform.position, 
            avoidanceRadius, 
            minionLayer
        );

        foreach (Collider2D other in nearbyMinions)
        {
            if (other.gameObject == gameObject) continue;
            
            Vector3 directionAway = transform.position - other.transform.position;
            float distance = directionAway.magnitude;
            
            if (distance > 0 && distance < avoidanceRadius)
            {
                float strength = 1f - (distance / avoidanceRadius);
                avoidance += directionAway.normalized * strength * avoidanceForce * Time.deltaTime;
            }
        }

        return avoidance;
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
            bullet = BulletPool.Instance.Get(transform.position, Quaternion.identity);
        else
            bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        if (bullet == null) return;

        ProjectileController projectile = bullet.GetComponent<ProjectileController>();
        if (projectile != null)
        {
            if (useCustomSpeed)
                projectile.Launch(direction, customBulletSpeed);
            else
                projectile.Launch(direction);

            activeBullets.Add(bullet);
        }
        else
        {
            if (BulletPool.Instance != null)
                BulletPool.Instance.Return(bullet);
            else
                Destroy(bullet);
        }
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

    void HandleHit(float damage)
    {
        MinionSpawner.Instance?.PlayHitSound();
    }

    void HandleDeath()
    {
        MinionSpawner.Instance?.PlayDeathSound();
        DestroyActiveBullets();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}