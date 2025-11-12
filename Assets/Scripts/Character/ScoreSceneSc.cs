using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreSceneSc : MonoBehaviour
{
    [SerializeField] List<string> levelCondition;
    [SerializeField] List<TMP_Text> textTMPList; // 0 level
    [SerializeField] List<RawImage> bintangsRawImage;
    [SerializeField] List<Texture2D> textureStars;
    [SerializeField] AudioSource audioSourceSfx;

    public static ScoreSceneSc Instance;

    public int last_level;
    public int myBintang;

    void Awake()
    {
        levelCondition = new List<string>(new string[3]); // inisialisasi dengan 3 elemen

    }

    void SetupStar(int star)
    {
        if (star == 1)
        {
            bintangsRawImage[1].texture = textureStars[1]; // abu
            bintangsRawImage[2].texture = textureStars[1]; // abu
        }
        else if (star == 2)
        {
            bintangsRawImage[2].texture = textureStars[1]; // abu
        }
        else if (star == 3)
        {
            // do nothing because we done :)
        }
    }
    void Start()
    {
        Instance = this;
        
        myBintang = PlayerPrefs.GetInt("myBintang");
        SetupStar(myBintang);

        last_level = PlayerPrefs.GetInt("last_level");
        textTMPList[0].text = last_level.ToString();

        // setup level condition
        levelCondition[0] = PlayerPrefs.GetString("bintang1");
        levelCondition[1] = PlayerPrefs.GetString("bintang2");
        levelCondition[2] = PlayerPrefs.GetString("bintang3");

        for (int i = 0; i < levelCondition.Count; i++)
        {
            textTMPList[i + 1].text = levelCondition[i];
        }

        // prpare UI
        StartShowScore();
    }
    public void StartShowScore()
    {
        textTMPList[4].text = "";
        string teks = $"Score {PlayerPrefs.GetInt("last_score")}";
        StartCoroutine(ShowScore(teks));
    }

     public void PlayAudio()
    {
        audioSourceSfx.Stop();
        audioSourceSfx.pitch = Random.Range(0.8f, 1.1f);
        audioSourceSfx.Play();
    }

    IEnumerator ShowScore(string teks)
    {
        yield return new WaitForSeconds(2.3f);
        foreach (char c in teks)
        {
            textTMPList[4].text += c;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void NextStage()
    {
        SceneManager.LoadScene($"Game{last_level + 1}");
    }

    public void Retry()
    {
        SceneManager.LoadScene($"Game{last_level}");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Home");
    }
}
