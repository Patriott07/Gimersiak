using System;
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public event Action OnDeath;
    public event Action OnNearDeath;
    public event Action<float> OnDamageTaken;
    
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    
    [Header("Enemy Type")]
    [SerializeField] EnemyType enemyType = EnemyType.Minion;
    [SerializeField] int scoreReward = 10;
    
    [Header("Boss Settings")]
    [SerializeField] float bossImmuneThreshold = 1f;
    
    [Header("Visual Feedback")]
    [SerializeField] bool flashOnHit = true;
    [SerializeField] Color hitColor = Color.red;
    [SerializeField] float flashDuration = 0.1f;
    
    [Header("Death Behavior")]
    [SerializeField] bool autoDestroy = true;
    [SerializeField] float destroyDelay = 0f;
    
    float currentHealth;
    bool isDead;
    bool isImmune;
    bool hasTriggeredNearDeath;
    bool hasReducedTotalHealth;
    SpriteRenderer spriteRenderer;
    Color originalColor;
    Coroutine flashCoroutine;
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => !isDead && currentHealth > 0;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsBoss => enemyType == EnemyType.Boss;
    public bool IsMinion => enemyType == EnemyType.Minion;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) 
            originalColor = spriteRenderer.color;
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        if (GameManager.Instance == null) return;

        if (IsBoss)
        {
            GameManager.Instance.totalHealthEnemy += (int)maxHealth;
            GameManager.Instance.isHaveEnemy = true;
        }
        else if (IsMinion)
        {
            GameManager.Instance.totalHealthEnemy += (int)maxHealth;
            GameManager.Instance.isHaveEnemy = true;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0) return;
        
        if (IsBoss && isImmune)
        {
            return;
        }
        
        float previousHealth = currentHealth;
        currentHealth -= amount;
        
        if (IsBoss)
        {
            if (currentHealth <= bossImmuneThreshold && previousHealth > bossImmuneThreshold)
            {
                currentHealth = bossImmuneThreshold;
                
                if (!hasTriggeredNearDeath)
                {
                    hasTriggeredNearDeath = true;
                    isImmune = true;
                    
                    if (!hasReducedTotalHealth)
                    {
                        float damageToThreshold = previousHealth - bossImmuneThreshold;
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.totalHealthEnemy -= (int)damageToThreshold;
                            GameManager.Instance.totalHealthEnemy = Mathf.Max(0, GameManager.Instance.totalHealthEnemy);
                        }
                        hasReducedTotalHealth = true;
                    }
                    
                    OnNearDeath?.Invoke();
                }
                
                if (flashOnHit && spriteRenderer != null)
                {
                    if (flashCoroutine != null)
                        StopCoroutine(flashCoroutine);
                    
                    flashCoroutine = StartCoroutine(FlashEffect());
                }
                
                OnDamageTaken?.Invoke(previousHealth - currentHealth);
                return;
            }
            
            if (isImmune)
            {
                currentHealth = bossImmuneThreshold;
                return;
            }
        }
        
        currentHealth = Mathf.Max(0f, currentHealth);
        
        float actualDamage = previousHealth - currentHealth;
        
        OnDamageTaken?.Invoke(actualDamage);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.totalHealthEnemy -= (int)actualDamage;
            GameManager.Instance.totalHealthEnemy = Mathf.Max(0, GameManager.Instance.totalHealthEnemy);
        }
        
        if (flashOnHit && spriteRenderer != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            
            flashCoroutine = StartCoroutine(FlashEffect());
        }
        
        if (currentHealth <= 0 && !IsBoss)
        {
            Die();
        }
    }

    public void ForceBossDeath()
    {
        if (!IsBoss) return;
        
        isImmune = false;
        
        if (GameManager.Instance != null && currentHealth > 0)
        {
            GameManager.Instance.totalHealthEnemy -= (int)currentHealth;
            GameManager.Instance.totalHealthEnemy = Mathf.Max(0, GameManager.Instance.totalHealthEnemy);
        }
        
        currentHealth = 0f;
        
        Die();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        float actualHeal = currentHealth - previousHealth;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.totalHealthEnemy += (int)actualHeal;
        }
    }

    public void SetMaxHealth(float value)
    {
        if (value <= 0) return;
        
        float healthPercentage = HealthPercentage;
        maxHealth = value;
        currentHealth = maxHealth * healthPercentage;
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    IEnumerator FlashEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        
        if (!isDead && spriteRenderer != null)
            spriteRenderer.color = originalColor;
        
        flashCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreReward);
            
            if (transform.position != null)
            {
                GameManager.Instance.SpawnVfxExplode(transform.position);
            }
        }
        
        OnDeath?.Invoke();
        
        if (autoDestroy)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    void OnDestroy()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
    }
}

public enum EnemyType
{
    Minion,
    Boss
}