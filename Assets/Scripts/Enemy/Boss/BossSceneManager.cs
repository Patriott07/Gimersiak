using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BossSceneManager : MonoBehaviour
{
    public static BossSceneManager Instance { get; private set; }

    [Header("Boss Reference")]
    [SerializeField] GameObject bossObject;
    [SerializeField] BossController bossController;
    [SerializeField] BossAttackManager bossAttackManager;
    [SerializeField] BossAnimationController bossAnimationController;

    [Header("Timing")]
    [SerializeField] float delayBeforeBossAppears = 20f;
    [SerializeField] float dialogDisplayDuration = 2f;

    [Header("Dialog UI")]
    [SerializeField] GameObject dialogPanel;
    [SerializeField] CanvasGroup dialogCanvasGroup;
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] Image bossPortrait;
    [SerializeField] float textSpeed = 0.05f;
    [SerializeField] KeyCode skipKey = KeyCode.Space;

    [Header("Entrance Dialogs")]
    [SerializeField] DialogData[] entranceDialogs;

    [Header("Death Dialogs")]
    [SerializeField] DialogData[] deathDialogs;

    [Header("Effects")]
    [SerializeField] GameObject bossEntranceVFX;
    [SerializeField] Vector3 bossSpawnPosition;
    [SerializeField] float entranceShakeDuration = 0.5f;
    [SerializeField] float entranceShakeStrength = 0.8f;

    [Header("Death Sequence Settings")]
    [SerializeField] float maxDeathWaitTime = 5f;

    bool bossHasAppeared;
    bool bossIsDead;
    bool dialogIsPlaying;
    bool bossSequenceStarted;
    bool isTyping;
    string currentFullText;
    Coroutine typingCoroutine;
    float gameTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeBoss();
    }

    void Update()
    {
        if (!bossSequenceStarted)
        {
            gameTimer += Time.deltaTime;
            if (gameTimer >= delayBeforeBossAppears)
            {
                bossSequenceStarted = true;
                StartCoroutine(BossAppearanceSequence());
            }
        }

        if (dialogIsPlaying && Input.GetKeyDown(skipKey))
        {
            SkipTyping();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (bossAnimationController != null) bossAnimationController.OnDeathAnimationComplete -= HandleBossDeathComplete;
    }

    void InitializeBoss()
    {
        if (bossObject != null) 
        {
            bossObject.SetActive(false);
        }
        
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (dialogCanvasGroup != null) dialogCanvasGroup.alpha = 0f;
        if (bossAnimationController != null) bossAnimationController.OnDeathAnimationComplete += HandleBossDeathComplete;
        if (bossController == null && bossObject != null) bossController = bossObject.GetComponent<BossController>();
        if (bossAttackManager == null && bossObject != null) bossAttackManager = bossObject.GetComponent<BossAttackManager>();
        if (bossAnimationController == null && bossObject != null) bossAnimationController = bossObject.GetComponent<BossAnimationController>();
    }

    IEnumerator BossAppearanceSequence()
    {
        if (bossHasAppeared) yield break;
        bossHasAppeared = true;

        FreezeAllEnemies(true);
        FreezePlayer(true);

        if (bossObject != null)
        {
            bossObject.transform.position = bossSpawnPosition;
            bossObject.SetActive(true);
        }

        if (bossEntranceVFX != null)
        {
            Instantiate(bossEntranceVFX, bossSpawnPosition, Quaternion.identity);
        }

        if (Camera.main != null)
        {
            StartCoroutine(ShakeCamera(entranceShakeDuration, entranceShakeStrength));
        }

        yield return new WaitForSeconds(0.5f);
        yield return ShowDialogs(entranceDialogs);

        EnableBossComponents();
        FreezeAllEnemies(false);
        FreezePlayer(false);
    }

    public void TriggerBossDeathSequence()
    {
        if (bossIsDead) return;
        bossIsDead = true;
        StartCoroutine(BossDeathSequence());
    }

    IEnumerator BossDeathSequence()
    {
        FreezeAllEnemies(true);
        FreezePlayer(true);
        DisableBossComponents();
        
        yield return new WaitForSeconds(0.3f);
        yield return ShowDialogs(deathDialogs);

        bool deathCompleted = false;
        if (bossAnimationController != null)
        {
            Action originalCallback = null;
            originalCallback = () => {
                deathCompleted = true;
                bossAnimationController.OnDeathAnimationComplete -= originalCallback;
            };
            bossAnimationController.OnDeathAnimationComplete += originalCallback;
            bossAnimationController.ExecuteDeathWithoutDialog();

            float waitTimer = 0f;
            while (!deathCompleted && waitTimer < maxDeathWaitTime)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            if (!deathCompleted)
            {
                bossAnimationController.OnDeathAnimationComplete -= originalCallback;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        HandleBossDeathComplete();
    }

    void HandleBossDeathComplete()
    {
        FreezeAllEnemies(false);
        FreezePlayer(false);
    }

    IEnumerator ShowDialogs(DialogData[] dialogs)
    {
        if (dialogs == null || dialogs.Length == 0) yield break;

        bool hasValidDialog = false;
        foreach (DialogData dialog in dialogs)
        {
            if (dialog != null && !string.IsNullOrEmpty(dialog.text))
            {
                hasValidDialog = true;
                break;
            }
        }
        if (!hasValidDialog) yield break;
        if (dialogPanel == null || dialogText == null) yield break;

        dialogIsPlaying = true;
        dialogPanel.SetActive(true);
        if (dialogCanvasGroup != null) yield return FadeCanvasGroup(dialogCanvasGroup, 0f, 1f, 0.3f);

        foreach (DialogData dialog in dialogs)
        {
            if (dialog == null || string.IsNullOrEmpty(dialog.text)) continue;
            
            if (bossPortrait != null && dialog.portrait != null)
            {
                bossPortrait.sprite = dialog.portrait;
                bossPortrait.enabled = true;
            }

            currentFullText = dialog.text;
            dialogText.text = "";
            isTyping = true;
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(dialog.text));
            while (isTyping) yield return null;
            yield return new WaitForSeconds(dialogDisplayDuration);
        }

        if (dialogCanvasGroup != null) yield return FadeCanvasGroup(dialogCanvasGroup, 1f, 0f, 0.3f);
        dialogPanel.SetActive(false);
        dialogIsPlaying = false;
    }

    IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
    }

    void SkipTyping()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogText.text = currentFullText;
            isTyping = false;
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float time = 0f;
        group.alpha = start;
        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }
        group.alpha = end;
    }

    void DisableBossComponents()
    {
        if (bossController != null) bossController.enabled = false;
        if (bossAttackManager != null) bossAttackManager.enabled = false;
    }

    void EnableBossComponents()
    {
        if (bossController != null) bossController.enabled = true;
        if (bossAttackManager != null) bossAttackManager.enabled = true;
    }

    void FreezeAllEnemies(bool freeze)
    {
        MinionController[] minions = FindObjectsByType<MinionController>(FindObjectsSortMode.None);
        foreach (MinionController minion in minions)
        {
            minion.enabled = !freeze;
            Rigidbody2D minionRb = minion.GetComponent<Rigidbody2D>();
            if (minionRb != null)
            {
                if (freeze)
                {
                    minionRb.linearVelocity = Vector2.zero;
                    minionRb.simulated = false;
                }
                else minionRb.simulated = true;
            }
        }

        ProjectileController[] projectiles = FindObjectsByType<ProjectileController>(FindObjectsSortMode.None);
        foreach (ProjectileController projectile in projectiles)
        {
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                if (freeze)
                {
                    projRb.linearVelocity = Vector2.zero;
                    projRb.simulated = false;
                }
                else projRb.simulated = true;
            }
        }

        if (MinionSpawner.Instance != null && freeze) MinionSpawner.Instance.StopAutoWave();
    }

    void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (freeze)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.simulated = false;
                }
                else rb.simulated = true;
            }

            MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in playerScripts)
            {
                if (script.GetType().Name != "Transform") script.enabled = !freeze;
            }
        }

        Rigidbody2D[] allBalls = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (Rigidbody2D ball in allBalls)
        {
            if (ball.gameObject.CompareTag("Ball") || ball.GetComponent<BallSc>() != null)
            {
                if (freeze)
                {
                    ball.linearVelocity = Vector2.zero;
                    ball.simulated = false;
                }
                else ball.simulated = true;
            }
        }
    }

    IEnumerator ShakeCamera(float duration, float strength)
    {
        if (Camera.main == null) yield break;
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * strength;
            float y = UnityEngine.Random.Range(-1f, 1f) * strength;
            Camera.main.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = originalPos;
    }

    public bool IsBossActive()
    {
        return bossHasAppeared && !bossIsDead;
    }

    public bool IsDialogPlaying()
    {
        return dialogIsPlaying;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bossSpawnPosition, 1f);
        Gizmos.DrawLine(bossSpawnPosition + Vector3.up, bossSpawnPosition + Vector3.down);
        Gizmos.DrawLine(bossSpawnPosition + Vector3.left, bossSpawnPosition + Vector3.right);
    }
}

[Serializable]
public class DialogData
{
    public string text;
    public Sprite portrait;
}