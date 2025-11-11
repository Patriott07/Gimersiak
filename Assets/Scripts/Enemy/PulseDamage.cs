using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PulseDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] float damage = 10f;
    
    [Header("Targets")]
    [SerializeField] string[] targetTags = { "Player" };
    [SerializeField] bool damageEnemies = false;
    
    [Header("Collision Behavior")]
    [SerializeField] bool destroyOnHit = false;
    [SerializeField] bool destroyOnWall = true;
    [SerializeField] LayerMask wallLayers;
    
    bool hasDealtDamage;
    float originalDamage;

    public float Damage => damage;
    
    void Awake()
    {
        originalDamage = damage;
    }
    
    void OnEnable()
    {
        hasDealtDamage = false;
        damage = originalDamage;
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        ProcessCollision(col.gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        ProcessCollision(col.gameObject);
    }

    void ProcessCollision(GameObject target)
    {
        if (hasDealtDamage && destroyOnHit) return;
        
        if (IsWall(target))
        {
            if (destroyOnWall)
                ReturnToPoolOrDestroy();
            return;
        }
        
        if (!IsValidTarget(target)) return;
        
        EnemyHealth targetHealth = target.GetComponent<EnemyHealth>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            
            if (destroyOnHit)
            {
                hasDealtDamage = true;
                ReturnToPoolOrDestroy();
            }
        }
    }

    bool IsWall(GameObject target)
    {
        if (destroyOnWall)
        {
            int targetLayer = 1 << target.layer;
            return (wallLayers.value & targetLayer) != 0;
        }
        return false;
    }

    bool IsValidTarget(GameObject target)
    {
        if (targetTags == null || targetTags.Length == 0) return true;
        
        if (!damageEnemies && target.CompareTag("Enemy") && gameObject.CompareTag("Enemy"))
            return false;
        
        foreach (string tag in targetTags)
        {
            if (target.CompareTag(tag)) return true;
        }
        
        return false;
    }

    void OnBecameInvisible()
    {
        ReturnToPoolOrDestroy();
    }

    void ReturnToPoolOrDestroy()
    {
        ProjectileController projectile = GetComponent<ProjectileController>();
        
        if (projectile != null && BulletPool.Instance != null)
        {
            BulletPool.Instance.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}