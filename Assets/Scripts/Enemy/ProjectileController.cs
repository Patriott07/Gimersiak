using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float defaultSpeed = 10f;
    [SerializeField] float lifetime = 10f;
    
    [Header("Pulse Animation")]
    [SerializeField] bool enablePulse = true;
    [SerializeField] float pulseSpeed = 2f;
    [SerializeField] float pulseMinScale = 0.8f;
    [SerializeField] float pulseMaxScale = 1.2f;
    
    Rigidbody2D rb;
    float spawnTime;
    Vector3 originalScale;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        originalScale = transform.localScale;
    }
    
    void OnEnable()
    {
        spawnTime = Time.time;
        transform.localScale = originalScale;
    }
    
    void Update()
    {
        if (Time.time - spawnTime > lifetime)
        {
            ReturnToPool();
            return;
        }
        
        if (enablePulse)
        {
            float pulse = Mathf.Lerp(pulseMinScale, pulseMaxScale, 
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            transform.localScale = originalScale * pulse;
        }
    }
    
    public void Launch(Vector2 direction)
    {
        Launch(direction, defaultSpeed);
    }
    
    public void Launch(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        rb.linearVelocity = direction.normalized * speed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    
    void ReturnToPool()
    {
        if (BulletPool.Instance != null)
            BulletPool.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }
}