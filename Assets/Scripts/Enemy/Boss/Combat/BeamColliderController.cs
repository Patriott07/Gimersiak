using UnityEngine;

public class BeamColliderController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float fallSpeed = 10f;
    [SerializeField] float riseSpeed = 8f;
    [SerializeField] float maxFallDistance = 50f;
    
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
    
    void Start()
    {
        startPosition = transform.position;
        
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
            
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }
    
    void Update()
    {
        if (!hasHitWall && !isRising)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            
            if (Vector3.Distance(startPosition, transform.position) > maxFallDistance)
            {
                hasHitWall = true;
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
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitWall || isRising) return;
        
        if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log($"[BeamCollider] Hit wall at {transform.position}!");
            
            hasHitWall = true;
            wallHitPosition = transform.position;
            
            if (sparkUpPrefab != null)
            {
                currentSpark = Instantiate(sparkUpPrefab, wallHitPosition, Quaternion.identity);
                
                Animator sparkAnim = currentSpark.GetComponent<Animator>();
                if (sparkAnim != null)
                {
                    sparkAnim.Play("SparkUp");
                }
                
                Debug.Log("[BeamCollider] SparkUp spawned!");
            }
        }
    }
    
    public void StartRising()
    {
        Debug.Log("[BeamCollider] Starting to rise!");
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
                
                Debug.Log("[BeamCollider] SparkDown spawned!");
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
    }
}