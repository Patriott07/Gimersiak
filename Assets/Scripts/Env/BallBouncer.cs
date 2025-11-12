using UnityEngine;

public class BallBouncer : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float minBounceAngle = 30f;
    [SerializeField] private float maxBounceAngle = 150f;
    [SerializeField] private float bounceForceMultiplier = 1.2f;
    [SerializeField] private float minVelocity = 5f;
    
    [Header("Anti-Stuck Settings")]
    [SerializeField] private float stuckCheckInterval = 2f;
    [SerializeField] private float stuckVelocityThreshold = 2f;
    [SerializeField] private float randomForceAmount = 50f;
    
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = transform.position;
    }
    
    void Update()
    {
        CheckIfStuck();
        
        EnsureMinimumVelocity();
    }
    
    void CheckIfStuck()
    {
        stuckTimer += Time.deltaTime;
        
        if (stuckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector2.Distance(lastPosition, transform.position);
            
            if (distanceMoved < stuckVelocityThreshold && rb.linearVelocity.magnitude < stuckVelocityThreshold)
            {
                ApplyRandomForce();
            }
            
            lastPosition = transform.position;
            stuckTimer = 0f;
        }
    }
    
    void EnsureMinimumVelocity()
    {
        if (rb.linearVelocity.magnitude < minVelocity && rb.linearVelocity.magnitude > 0.1f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minVelocity;
        }
    }
    
    void ApplyRandomForce()
    {
        float randomAngle = Random.Range(minBounceAngle, maxBounceAngle);
        float angleInRadians = randomAngle * Mathf.Deg2Rad;
        
        Vector2 randomDirection = new Vector2(
            Mathf.Cos(angleInRadians) * (Random.value > 0.5f ? 1 : -1),
            Mathf.Abs(Mathf.Sin(angleInRadians))
        );
        
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(randomDirection * randomForceAmount, ForceMode2D.Impulse);
        
        Debug.Log("Ball was stuck! Applied random force.");
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ship"))
        {
            BounceFromBottom(collision);
        }
        
        if (collision.gameObject.CompareTag("wall"))
        {
            CorrectWallBounce(collision);
        }
    }
    
    void BounceFromBottom(Collision2D collision)
    {
        Vector2 contactPoint = collision.contacts[0].point;
        Vector2 center = collision.otherCollider.bounds.center;
        
        float offset = (contactPoint.x - center.x) / collision.otherCollider.bounds.extents.x;
        
        float angle = Mathf.Lerp(60f, 120f, (offset + 1f) / 2f);
        float angleInRadians = angle * Mathf.Deg2Rad;
        
        Vector2 bounceDirection = new Vector2(
            Mathf.Cos(angleInRadians),
            Mathf.Abs(Mathf.Sin(angleInRadians))
        );
        
        float currentSpeed = rb.linearVelocity.magnitude;
        rb.linearVelocity = bounceDirection * Mathf.Max(currentSpeed * bounceForceMultiplier, minVelocity);
    }
    
    void CorrectWallBounce(Collision2D collision)
    {
        Vector2 velocity = rb.linearVelocity;
        float angle = Mathf.Abs(Vector2.Angle(velocity, Vector2.right));
        
        if (angle < minBounceAngle || angle > maxBounceAngle)
        {
            float verticalBoost = Random.Range(2f, 5f) * Mathf.Sign(Random.value - 0.5f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + verticalBoost);
        }
    }
    
    public void ForceBounce(Vector2 direction, float force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }
}