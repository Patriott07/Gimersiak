using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerHitResponse : MonoBehaviour
{
    [Header("Camera Shake")]
    [SerializeField] float shakeDuration = 0.25f;
    [SerializeField] float shakeStrength = 0.4f;
    [SerializeField] int shakeVibrato = 15;
    [SerializeField] float shakeRandomness = 80f;

    [Header("Slow Motion Effect")]
    [SerializeField] float slowMotionScale = 0.2f;
    [SerializeField] float slowMotionDuration = 0.3f;
    [SerializeField] float recoverySpeed = 2f;

    [Header("Invincibility")]
    [SerializeField] float invincibilityDuration = 1f;
    [SerializeField] SpriteRenderer playerSprite;
    [SerializeField] float blinkSpeed = 0.08f;

    [Header("Visual Feedback")]
    [SerializeField] GameObject hitVFX;
    [SerializeField] float vfxLifetime = 0.5f;

    bool isInvincible = false;
    float invincibilityTimer = 0f;
    bool isInSlowMotion = false;

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.unscaledDeltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (playerSprite != null)
                {
                    playerSprite.DOKill();
                    Color c = playerSprite.color;
                    c.a = 1f;
                    playerSprite.color = c;
                }
            }
        }
    }

    public void OnHit(float damage)
    {
        if (isInvincible) return;
        if (GameManager.Instance != null && !GameManager.Instance.isPlay) return;

        TriggerCameraShake();
        SpawnHitVFX();
        StartSlowMotion();
        StartInvincibility();
    }

    void SpawnHitVFX()
    {
        if (hitVFX != null)
        {
            GameObject vfx = Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(vfx, vfxLifetime);
        }
    }

    void TriggerCameraShake()
    {
        if (Camera.main != null)
        {
            Camera.main.DOShakePosition(
                shakeDuration, 
                shakeStrength, 
                shakeVibrato, 
                shakeRandomness, 
                false
            ).SetUpdate(true);
        }
    }

    void StartSlowMotion()
    {
        if (isInSlowMotion) return;
        
        StopAllCoroutines();
        StartCoroutine(SlowMotionEffect());
    }

    IEnumerator SlowMotionEffect()
    {
        isInSlowMotion = true;
        
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        
        float elapsedTime = 0f;
        float duration = 1f / recoverySpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;
            
            Time.timeScale = Mathf.Lerp(slowMotionScale, 1f, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            
            yield return null;
        }
        
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isInSlowMotion = false;
    }

    void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        if (playerSprite != null)
        {
            playerSprite.DOKill();
            BlinkEffect();
        }
    }

    void BlinkEffect()
    {
        if (!isInvincible || playerSprite == null) return;

        Sequence blinkSequence = DOTween.Sequence();
        blinkSequence.SetUpdate(true);
        
        float remainingTime = invincibilityTimer;
        int blinkCount = Mathf.CeilToInt(remainingTime / (blinkSpeed * 2));

        for (int i = 0; i < blinkCount; i++)
        {
            blinkSequence.Append(playerSprite.DOFade(0.3f, blinkSpeed).SetUpdate(true));
            blinkSequence.Append(playerSprite.DOFade(1f, blinkSpeed).SetUpdate(true));
        }

        blinkSequence.OnComplete(() =>
        {
            if (playerSprite != null)
            {
                Color c = playerSprite.color;
                c.a = 1f;
                playerSprite.color = c;
            }
        });
    }

    void OnDestroy()
    {
        if (playerSprite != null)
        {
            playerSprite.DOKill();
        }
        
        if (Camera.main != null)
        {
            Camera.main.DOKill();
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}