using UnityEngine;

public class PlayerSlowEffect : MonoBehaviour
{
    public static PlayerSlowEffect Instance;
    
    [Header("Slow Settings")]
    [SerializeField] float slowMultiplier = 0.5f;
    [SerializeField] float slowDuration = 2f;
    
    float originalMoveSpeed;
    float slowTimer = 0f;
    bool isSlowed = false;
    CharacterMove characterMove;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        characterMove = GetComponent<CharacterMove>();
        if (characterMove != null)
            originalMoveSpeed = characterMove.moveSpeed;
    }
    
    void Update()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            
            if (slowTimer <= 0f)
            {
                RemoveSlow();
            }
        }
    }
    
    public void ApplySlow(float duration, float multiplier)
    {
        if (characterMove == null) return;
        
        if (!isSlowed)
        {
            originalMoveSpeed = characterMove.moveSpeed;
        }
        
        isSlowed = true;
        slowTimer = duration;
        characterMove.moveSpeed = originalMoveSpeed * multiplier;
    }
    
    public void ApplySlow(float duration)
    {
        ApplySlow(duration, slowMultiplier);
    }
    
    public void ApplySlow()
    {
        ApplySlow(slowDuration, slowMultiplier);
    }
    
    void RemoveSlow()
    {
        if (characterMove == null) return;
        
        isSlowed = false;
        characterMove.moveSpeed = originalMoveSpeed;
        slowTimer = 0f;
    }
    
    public bool IsSlowed()
    {
        return isSlowed;
    }
}