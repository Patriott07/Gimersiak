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
    [SerializeField] Transform beamVisual;
    
    [Header("Beam Scaling")]
    [SerializeField] bool autoScaleBeam = true;
    [SerializeField] float beamWidth = 2f;
    [SerializeField] bool staticBeam = false;
    
    Vector3 startPosition;
    GameObject currentSpark;
    bool hasHitWall = false;
    bool isRising = false;
    Vector3 wallHitPosition;
    Vector3 targetGroundPosition;
    bool hasTargetPosition = false;
    Vector3 initialScale;
    float groundDistance;
    float currentExtension;
    float targetExtension;
    
    void Start()
    {
        startPosition = transform.position;
        
        if (beamVisual == null)
        {
            beamVisual = transform.GetChild(0);
        }
        
        if (beamVisual != null)
        {
            initialScale = beamVisual.localScale;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(startPosition, Vector2.down, raycastDistance, groundLayer);
        
        if (hit.collider != null)
        {
            targetGroundPosition = hit.point;
            targetGroundPosition.y += groundOffset;
            groundDistance = Vector3.Distance(startPosition, targetGroundPosition);
            hasTargetPosition = true;
            
            if (staticBeam)
            {
                targetExtension = groundDistance;
            }
            else
            {
                maxFallDistance = groundDistance;
            }
        }
        
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
            
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }
    
    void Update()
    {
        if (staticBeam)
        {
            UpdateStaticBeam();
        }
        else
        {
            UpdateFallingBeam();
        }
    }
    
    void UpdateStaticBeam()
    {
        if (!hasHitWall && !isRising)
        {
            currentExtension += fallSpeed * Time.deltaTime;
            
            if (currentExtension >= targetExtension)
            {
                currentExtension = targetExtension;
                hasHitWall = true;
                OnHitGround(targetGroundPosition);
            }
            
            UpdateBeamScale(currentExtension);
        }
        else if (isRising)
        {
            currentExtension -= riseSpeed * Time.deltaTime;
            
            if (currentExtension <= 0f)
            {
                currentExtension = 0f;
                enabled = false;
            }
            
            UpdateBeamScale(currentExtension);
        }
    }
    
    void UpdateFallingBeam()
    {
        if (!hasHitWall && !isRising)
        {
            float moveDistance = fallSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + Vector3.down * moveDistance;
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, moveDistance + 0.5f, groundLayer);
            
            if (hit.collider != null)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + groundOffset, transform.position.z);
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
                        UpdateBeamScale(Mathf.Abs(transform.position.y - startPosition.y));
                    }
                }
                else
                {
                    transform.position = newPosition;
                    UpdateBeamScale(Mathf.Abs(transform.position.y - startPosition.y));
                    
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
            UpdateBeamScale(Mathf.Abs(transform.position.y - startPosition.y));
            
            if (transform.position.y >= startPosition.y)
            {
                transform.position = startPosition;
                enabled = false;
            }
        }
    }
    
    void UpdateBeamScale(float distance)
    {
        if (!autoScaleBeam || beamVisual == null) return;
        
        beamVisual.localScale = new Vector3(beamWidth, distance, initialScale.z);
        
        Vector3 visualOffset = beamVisual.localPosition;
        visualOffset.y = -distance * 0.5f;
        beamVisual.localPosition = visualOffset;
    }
    
    void OnHitGround(Vector3 hitPoint)
    {
        wallHitPosition = hitPoint;
        
        if (staticBeam)
        {
            UpdateBeamScale(currentExtension);
        }
        else
        {
            UpdateBeamScale(Mathf.Abs(transform.position.y - startPosition.y));
        }
        
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
}