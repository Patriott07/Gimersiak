using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform firePoint;
    [SerializeField] Transform player;
    
    [Header("Attack Settings")]
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] BaseAttack[] attacks;
    
    float attackTimer;
    bool isAttacking;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        if (firePoint == null)
            firePoint = transform;
        
        foreach (var attack in attacks)
        {
            if (attack != null)
                attack.Initialize(firePoint, player);
        }
    }
    
    void Update()
    {
        if (isAttacking) return;
        
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            StartCoroutine(ExecuteRandomAttack());
        }
    }
    
    IEnumerator ExecuteRandomAttack()
    {
        isAttacking = true;
        
        List<BaseAttack> availableAttacks = new List<BaseAttack>();
        foreach (var attack in attacks)
        {
            if (attack != null && attack.IsEnabled)
                availableAttacks.Add(attack);
        }
        
        if (availableAttacks.Count > 0)
        {
            BaseAttack selectedAttack = availableAttacks[Random.Range(0, availableAttacks.Count)];
            yield return selectedAttack.Execute();
        }
        
        isAttacking = false;
    }
}
