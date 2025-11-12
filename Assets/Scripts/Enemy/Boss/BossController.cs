using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float leftBound = -6f;
    [SerializeField] float rightBound = 6f;
    [SerializeField] float pauseDuration = 0.5f;
    
    Rigidbody2D rb;
    EnemyHealth health;
    float moveDirection = 1f;
    bool isPaused;
    float lockedY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        
        lockedY = transform.position.y;
    }

    void Update()
    {
        if (health == null || !health.IsAlive) return;
        
        HandleMovement();
    }

    void HandleMovement()
    {
        if (isPaused) return;
        
        Vector3 pos = transform.position;
        pos.x += moveDirection * moveSpeed * Time.deltaTime;
        pos.y = lockedY;
        
        if (pos.x <= leftBound)
        {
            pos.x = leftBound;
            moveDirection = 1f;
            StartCoroutine(Pause());
        }
        else if (pos.x >= rightBound)
        {
            pos.x = rightBound;
            moveDirection = -1f;
            StartCoroutine(Pause());
        }
        
        transform.position = pos;
    }

    IEnumerator Pause()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 left = new Vector3(leftBound, transform.position.y, 0f);
        Vector3 right = new Vector3(rightBound, transform.position.y, 0f);
        Gizmos.DrawLine(left, right);
    }
}