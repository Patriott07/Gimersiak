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

    // variabel
    public int health = 5, score, combo, level, totalBlock, totalHealthBoss;
    public bool isCombo = false, isPlay = false, isHaveBoss = false, isFirstPlay = true, isLoose = false, isCanPlay = false;

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

    void Start()
    {
        Instance = this;

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

        if (IsLevelCompleted())
        {
            StageWasClear();
        }

        if (health <= 0 && !isLoose)
        {
            isLoose = true;
            characterMoveSc.isCanMove = false;
            ballSc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            isPlay = false;
            HUDManager.Instance.ShowGameOver();
        }


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
                return true;
        }
        else
        {
            // berdasarkan balok
            if (totalBlock <= 0) return true;
        }

        return false;
    }

    void StageWasClear()
    {
        hUDManager.ShowCompletedStage(GetTextGrading(totalPlaySeconds), 4f, () =>
        {
            // Show UI Scoring
            SceneManager.LoadSceneAsync("Scoring", LoadSceneMode.Additive);
            Debug.Log("Load scene scoring");
        });
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
