using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ShieldScript : MonoBehaviour
{
    [Header("Shield Settings")]
    public int shield = 3;
    public int maxShield = 3;
    
    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.3f;
    [SerializeField] private int shakeVibrato = 10;
    
    [Header("Shield Destruction")]
    [SerializeField] private GameObject shieldBreakVFX;
    
    private Color originalColor;
    private Vector3 originalPosition;
    private bool isHitting = false;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        originalColor = spriteRenderer.color;
        originalPosition = transform.position;
        shield = maxShield;
    }

    void Update()
    {
        if (shield <= 0)
        {
            DestroyShield();
        }
    }

    public void TakeHit()
    {
        if (isHitting) return;
        
        shield--;
        StartCoroutine(HitEffect());
        
        UpdateShieldAlpha();
    }

    IEnumerator HitEffect()
    {
        isHitting = true;
        
        // Shake effect
        transform.DOShakePosition(hitDuration, shakeStrength, shakeVibrato, 90, false)
            .OnComplete(() => transform.position = originalPosition);
        
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitDuration);
        spriteRenderer.color = originalColor;
        
        isHitting = false;
    }

    void UpdateShieldAlpha()
    {
        float alpha = Mathf.Lerp(0.3f, 1f, (float)shield / maxShield);
        Color newColor = originalColor;
        newColor.a = alpha;
        spriteRenderer.color = newColor;
        originalColor = newColor;
    }

    void DestroyShield()
    {
        if (shieldBreakVFX != null)
        {
            Instantiate(shieldBreakVFX, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }

    public void RestoreShield(int amount)
    {
        shield = Mathf.Min(shield + amount, maxShield);
        UpdateShieldAlpha();
    }

    public void ResetShield()
    {
        shield = maxShield;
        UpdateShieldAlpha();
    }
}