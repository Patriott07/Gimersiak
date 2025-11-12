using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // singletoon
    public static GameManager Instance;

    // Character
    [SerializeField] GameObject Character, vfxExplode;
    [SerializeField] List<GameObject> vfxExplodes;

    // Sc
    [SerializeField] CharacterMove characterMoveSc;
    [SerializeField] HUDManager hUDManager;
    [SerializeField] BallSc ballSc;

    // light
    [SerializeField] GameObject LightUltimate, fireKiri, fireKanan;

    // SpriteRendered
    [SerializeField] List<SpriteRenderer> spriteRenderersShield;

    // variabel
    public int health = 5, score, combo, level, totalBlock, totalHealthBoss;
    public bool isCombo = false, isPlay = false, isHaveBoss = false, isFirstPlay = true, isLoose = false, isCanPlay = false, isWinning = false, isCanUlt = false, isCanGetHit = true, isHUDUlt = false, isCanRestoreUltimate = true;

    public int bintangSatu_Menit, bintangDua_Menit, bintangTiga_Menit;

    public float totalPlaySeconds, ultimate;

    public float Ultimate
    {
        get
        {
            return ultimate;
        }
        set
        {
            ultimate = value;
            hUDManager.UpdateBarUltimate(ultimate);
        }
    }

    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            hUDManager.textScore.text = value.ToString();
        }
    }

    // Audio
    [SerializeField] AudioSource audioSourceBox;

    public void PlaySound()
    {
        audioSourceBox.Stop();
        audioSourceBox.loop = false;
        audioSourceBox.volume = Random.Range(0.9f, 1.2f);
        audioSourceBox.pitch = Random.Range(0.8f, 1.2f);
        audioSourceBox.Play();
    }


    /// <summary>
    /// ultimate
    /// </summary>
    /// 
    // fragment (ball split) settings
    [SerializeField] GameObject ballFragmentPrefab;
    [SerializeField] int fragmentCount = 8;
    [SerializeField] float fragmentForce = 200f;
    [SerializeField] float fragmentLifetime = 2f;
    [SerializeField] float originalBallDisableDuration = 1f;


    void Start()
    {
        Instance = this;
        PlayerPrefs.SetInt("last_level", level);
        SaveTargetScoreToPlayerPref();

        for (int i = 0; i < 20; i++)
        {
            GameObject vfx = Instantiate(vfxExplode);
            vfxExplodes.Add(vfx);
            vfx.SetActive(false);
        }

        characterMoveSc.isCanMove = false;
        isPlay = false;

        List<NoteBalok> noteComps = FindObjectsByType<NoteBalok>(FindObjectsSortMode.None).ToList();
        totalBlock = noteComps.Count;

        StartCoroutine(hUDManager.ShowPrePlay("Clear The Stage And Protect Our Ship", level, 3f, () =>
        {
            HUDManager.Instance.tutorialUI.DOFade(1, 0.5f);
            // CameraFollow.Instance.target = Character.transform;
            Camera.main.DOOrthoSize(15f, 2f);
            Camera.main.transform.DOMoveY(0.5f, 2f);

            float time = 0;
            while (time < 2)
            {
                time += Time.deltaTime;
            }

            isCanPlay = true;
        }));
    }

    void ChangeAllColorShield(Color32 color)
    {
        foreach (SpriteRenderer spriteRendererShield in spriteRenderersShield)
        {
            spriteRendererShield.color = color;
        }
    }

    void SaveTargetScoreToPlayerPref()
    {
        PlayerPrefs.SetString("bintang1", $"Clear Level in  {bintangSatu_Menit} minute");
        PlayerPrefs.SetString("bintang2", $"Clear Level in  {bintangDua_Menit} minute");
        PlayerPrefs.SetString("bintang3", $"Clear Level in  {bintangTiga_Menit} minute");
    }

    public void SpawnVfxExplode(Vector3 position)
    {
        foreach (GameObject vfx in vfxExplodes)
        {
            if (!vfx.activeInHierarchy)
            {
                vfx.transform.position = position;
                vfx.SetActive(true);
            }
        }
    }

    void UpdateUITimer()
    {
        int minutes = Mathf.FloorToInt(totalPlaySeconds / 60);
        int seconds = Mathf.FloorToInt(totalPlaySeconds % 60);
        HUDManager.Instance.textTimer.text = $"{minutes:00}:{seconds:00}";
    }

    void KeyAnyForStart()
    {
        if (!isCanPlay) return;
        isFirstPlay = false;
        HUDManager.Instance.HideTutorial();
        // first time
        isPlay = true;

        characterMoveSc.isCanMove = true;
        ballSc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }

    bool CheckIsCanUltimate()
    {
        if (Ultimate >= 1) return true;
        else return false;
    }
    void Update()
    {
        if (isLoose)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                BackToMainMenu();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadScene();
            }
        }

        if (Input.anyKeyDown && isFirstPlay)
        {
            Debug.LogWarning("WOY");
            KeyAnyForStart();
        }

        if (IsLevelCompleted() && !isWinning)
        {
            isWinning = true;
            characterMoveSc.isCanMove = false;
            ballSc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            isPlay = false;
            StageWasClear();
        }



        if (CheckIsCanUltimate())
        {
            // ngapain
            isCanUlt = true;

            if (isCanUlt && Input.GetKeyDown(KeyCode.K) && DetectorBall.Instance.rbBall != null)
            {
                Ultimate = 0;
                isCanUlt = false;
                StartCoroutine(UltimateActive());
            }
        }

        if (!isHUDUlt && Ultimate >= 1)
        {
            isHUDUlt = true;
            ChangeEnvToStateUltReady();
        }

        if (isPlay && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        else if (!isPlay && Input.GetKeyDown(KeyCode.Escape))
        {
            UnPauseGame();
        }

        if (!isPlay) return;

        // add timer
        totalPlaySeconds += Time.unscaledDeltaTime;
        UpdateUITimer();



        if (health <= 0 && !isLoose)
        {
            isLoose = true;
            characterMoveSc.isCanMove = false;
            ballSc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            isPlay = false;
            HUDManager.Instance.ShowGameOver();
        }




    }

    IEnumerator UltimateActive()
    {
        isCanRestoreUltimate = false;
        LightUltimate.SetActive(true);
        HUDManager.Instance.StartAnimationUlt(); // ui
        CharacterMove.Instance.isCanMove = false;

        // stop karakter & bola sementara
        CharacterMove.Instance.rb.linearVelocity = Vector2.zero;
        if (DetectorBall.Instance?.rbBall != null)
            DetectorBall.Instance.rbBall.linearVelocity = Vector2.zero;

        // freeze ball physics while playing ultimate intro
        if (DetectorBall.Instance?.rbBall != null)
        {
            DetectorBall.Instance.rbBall.bodyType = RigidbodyType2D.Kinematic;
            DetectorBall.Instance.rbBall.constraints = RigidbodyConstraints2D.FreezePosition;
        }

        yield return new WaitForSecondsRealtime(2f);

        if (DetectorBall.Instance?.rbBall != null)
        {
            DetectorBall.Instance.rbBall.bodyType = RigidbodyType2D.Dynamic;
            DetectorBall.Instance.rbBall.constraints = RigidbodyConstraints2D.None;
        }

        // kebal shield, ship
        isCanGetHit = false;
        ChangeAllColorShield(new Color32(61, 225, 227, 255));

        // belah -> apply a short push then split into fragments
        CharacterHit.Instance.AddForceToBallCustomStrength(200f);
        Time.timeScale = 3f;
        StartCoroutine(SplitBall());

        yield return new WaitForSecondsRealtime(1f);
        CharacterMove.Instance.isCanMove = true;

        yield return new WaitForSecondsRealtime(5f);
        LightUltimate.SetActive(false);
        isCanGetHit = true;
        ChangeAllColorShield(new Color32(255, 225, 255, 255));

        isHUDUlt = false;
        isCanRestoreUltimate = true;
        ChangeEnvToStateIdle();
    }
    IEnumerator SplitBall()
    {
        if (ballFragmentPrefab == null || DetectorBall.Instance == null || DetectorBall.Instance.rbBall == null)
            yield break;

        Rigidbody2D ballRb = DetectorBall.Instance.rbBall;
        GameObject ballGO = ballRb.gameObject;

        if (ballRb == null) Debug.LogWarning("BALL RBNULL");
        if (ballGO == null) Debug.LogWarning("BALL GO NULL");

        // cache state
        Vector3 savedPos = ballGO.transform.position;
        Vector2 savedVel = ballRb.linearVelocity;
        float savedAngularVel = ballRb.angularVelocity;
        bool wasActive = ballGO.activeSelf;

        // deactivate original ball (safer than messing with constraints)
        // ballGO.SetActive(false);

        // spawn fragments and apply impulse
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject frag = Instantiate(ballFragmentPrefab, savedPos, Quaternion.identity);
            Rigidbody2D rb = frag.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                rb.AddForce(dir * fragmentForce, ForceMode2D.Impulse);
                // rb.AddTorque(Random.Range(-200f, 200f));
            }
            Destroy(frag, fragmentLifetime);
        }

        // wait before restoring original ball
        yield return new WaitForSecondsRealtime(originalBallDisableDuration);

        // restore original ball
        Time.timeScale = 1f;
        ballGO.SetActive(wasActive);
        ballGO.transform.position = savedPos;
        ballRb.bodyType = RigidbodyType2D.Dynamic;
        ballRb.linearVelocity = savedVel;
        ballRb.angularVelocity = savedAngularVel;
    }

    public void ChangeEnvToStateUltReady()
    {
        fireKiri.SetActive(true);
        fireKanan.SetActive(true);

        HUDManager.Instance.ChangeAvatarToUltimateMode();
    }

    public void ChangeEnvToStateIdle()
    {
        fireKiri.SetActive(false);
        fireKanan.SetActive(false);

        HUDManager.Instance.ChangeAvatarToidle();
    }

    public void ReloadScene()
    {
        // pastikan game tidak dalam keadaan pause
        Time.timeScale = 1f;

        // cara cepat: reload scene secara sinkron
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddScore(int value = 1)
    {
        Score += value * ((combo * 15) + combo);
        hUDManager.textScore.text = Score.ToString();
    }

    bool IsLevelCompleted()
    {
        if (isHaveBoss)
        {

            // berdasarkan boss
            if (totalHealthBoss <= 0)
            {
                Debug.Log("masuk boss");
                return true;
            }

        }
        else
        {
            // berdasarkan balok
            if (totalBlock <= 0)
            {

                Debug.Log("masuk balok");
                return true;
            }

        }

        return false;
    }

    void StageWasClear()
    {
        Debug.Log("STAGE WAS CLEAR RUNN");
        PlayerPrefs.SetInt("myBintang", GetCountStar(totalPlaySeconds));
        PlayerPrefs.SetInt("last_score", Score);

        StartCoroutine(hUDManager.ShowCompletedStage(GetTextGrading(totalPlaySeconds), 4f, () =>
        {
            // Show UI Scoring
            SceneManager.LoadSceneAsync("MissionComplete", LoadSceneMode.Additive);
            Debug.LogWarning("Load scene scoring");
        }));
    }

    public int GetCountStar(float second)
    {
        if (second / 60f <= bintangTiga_Menit)
        {
            return 3;
        }
        else if (second / 60f <= bintangDua_Menit)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
    string GetTextGrading(float second)
    {
        if (second / 60f <= bintangTiga_Menit)
        {
            return "YOU'RE A MASTER";
        }
        else if (second / 60f <= bintangDua_Menit)
        {
            return "YOU'RE SKILLED";
        }
        else
        {
            return "YOU NEED PRACTICE";
        }
    }

    public void PauseGame()
    {
        if (isLoose) return;
        isPlay = false;
        ballSc.rb.bodyType = RigidbodyType2D.Kinematic;
        HUDManager.Instance.ShowPause();
    }

    public void UnPauseGame()
    {
        isPlay = true;
        ballSc.rb.bodyType = RigidbodyType2D.Dynamic;
        HUDManager.Instance.HidePause();
    }
    public void OpenSetting()
    {
        SceneManager.LoadSceneAsync("Setting", LoadSceneMode.Additive);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Home");
    }
}
