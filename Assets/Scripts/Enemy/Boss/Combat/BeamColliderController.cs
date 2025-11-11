using UnityEngine;

public class BeamColliderController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float fallSpeed = 10f;
    [SerializeField] float riseSpeed = 8f;
    [SerializeField] float maxFallDistance = 50f;
    
    [Header("Ground Detection")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float raycastDistance = 100f;
    [SerializeField] float groundOffset = 0.1f;
    
    [Header("Spark Prefabs")]
    [SerializeField] GameObject sparkUpPrefab;
    [SerializeField] GameObject sparkDownPrefab;
    
    [Header("References")]
    [SerializeField] Collider2D triggerCollider;
    
    [Header("Debug")]
    [SerializeField] bool showDebugGizmo = true;
    
    Vector3 startPosition;
    GameObject currentSpark;
    bool hasHitWall = false;
    bool isRising = false;
    Vector3 wallHitPosition;
    Vector3 targetGroundPosition;
    bool hasTargetPosition = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        RaycastHit2D hit = Physics2D.Raycast(
            startPosition, 
            Vector2.down, 
            raycastDistance, 
            groundLayer
        );
        
        if (hit.collider != null)
        {
            targetGroundPosition = hit.point;
            targetGroundPosition.y += groundOffset;
            maxFallDistance = Vector3.Distance(startPosition, targetGroundPosition);
            hasTargetPosition = true;
            
        }
        
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
            
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }
    
    void Update()
    {
        if (!hasHitWall && !isRising)
        {
            float moveDistance = fallSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + Vector3.down * moveDistance;
            
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                moveDistance + 0.5f,
                groundLayer
            );
            
            if (hit.collider != null)
            {
                transform.position = new Vector3(
                    transform.position.x, 
                    hit.point.y + groundOffset, 
                    transform.position.z
                );
                OnHitGround(hit.point);
                hasHitWall = true;
            }
            else
            {
                if (hasTargetPosition)
                {
                    if (newPosition.y <= targetGroundPosition.y)
                    {
                        transform.position = targetGroundPosition;
                        OnHitGround(targetGroundPosition);
                        hasHitWall = true;
                    }
                    else
                    {
                        transform.position = newPosition;
                    }
                }
                else
                {
                    transform.position = newPosition;
                    
                    if (Vector3.Distance(startPosition, transform.position) > maxFallDistance)
                    {
                        hasHitWall = true;
                        OnHitGround(transform.position);
                    }
                }
            }
        }
        else if (isRising)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            
            if (transform.position.y >= startPosition.y)
            {
                transform.position = startPosition;
                enabled = false;
            }
        }
    }
    
    void OnHitGround(Vector3 hitPoint)
    {
        wallHitPosition = hitPoint;
        
        if (sparkUpPrefab != null)
        {
            currentSpark = Instantiate(sparkUpPrefab, wallHitPosition, Quaternion.identity);
            
            Animator sparkAnim = currentSpark.GetComponent<Animator>();
            if (sparkAnim != null)
            {
                sparkAnim.Play("SparkUp");
            }
            
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitWall || isRising) return;
        
        if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            
            hasHitWall = true;
            OnHitGround(transform.position);
        }
    }
    
    public void StartRising()
    {
        isRising = true;
        
        if (currentSpark != null)
        {
            Destroy(currentSpark);
            
            if (sparkDownPrefab != null)
            {
                currentSpark = Instantiate(sparkDownPrefab, wallHitPosition, Quaternion.identity);
                
                Animator sparkAnim = currentSpark.GetComponent<Animator>();
                if (sparkAnim != null)
                {
                    sparkAnim.Play("SparkDown");
                }
                
            }
        }
    }
    
    void OnDestroy()
    {
        if (currentSpark != null)
            Destroy(currentSpark);
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmo) return;
        
        Gizmos.color = Color.red;
        if (triggerCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, triggerCollider.bounds.size);
        }
        
        if (Application.isPlaying && hasTargetPosition)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetGroundPosition, 0.5f);
            Gizmos.DrawLine(startPosition, targetGroundPosition);
        }
    }
}