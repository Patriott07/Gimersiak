using System.Collections;
using UnityEngine;

public abstract class BaseAttack : ScriptableObject
{
    [SerializeField] protected bool enabled = true;
    [SerializeField] protected float damage = 10f;
    
    protected Transform firePoint;
    protected Transform player;
    
    public bool IsEnabled => enabled;
    
    public virtual void Initialize(Transform firePoint, Transform player)
    {
        this.firePoint = firePoint;
        this.player = player;
    }
    
    public abstract IEnumerator Execute();
    
    protected Vector2 GetDirectionToPlayer()
    {
        if (player == null || firePoint == null) return Vector2.down;
        return (player.position - firePoint.position).normalized;
    }
}