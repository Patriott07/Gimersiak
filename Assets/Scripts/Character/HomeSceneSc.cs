using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class HomeSceneSc : MonoBehaviour
{
    public static HomeSceneSc Instance;
    public AudioSource audioSourceMusic, audioSourceSfx, audioSourceClick;
    int lastLevel;
    [SerializeField] CanvasGroup canvasGroupContinue;
    [SerializeField] AudioMixer audioMixer;

    void Start()
    {

        Instance = this;
        // cek level
        lastLevel = PlayerPrefs.GetInt("last_level", 0);
        Debug.Log(lastLevel);
        StartCoroutine(CheckIsCanContinue());

        audioMixer.SetFloat("volume_music", PlayerPrefs.GetFloat("music"));
        audioMixer.SetFloat("volume_master", PlayerPrefs.GetFloat("master"));
        audioMixer.SetFloat("volume_sfx", PlayerPrefs.GetFloat("sfx"));
    }

    public IEnumerator CheckIsCanContinue()
    {
        yield return new WaitForSeconds(2f);
        if (lastLevel == 0)
        {
            canvasGroupContinue.DOFade(0.4f, 0.5f);
            canvasGroupContinue.interactable = false;
            canvasGroupContinue.blocksRaycasts = false;
        }
        else
        {
             canvasGroupContinue.DOFade(1f, 0.5f);
            canvasGroupContinue.interactable = true;
            canvasGroupContinue.blocksRaycasts = true;
        }
    }

    public void PlayAudio()
    {
        audioSourceSfx.Stop();
        audioSourceSfx.pitch = Random.Range(0.8f, 1.1f);
        audioSourceSfx.Play();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("CutScene01");
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene($"Game{lastLevel}");
    }

    public void SettingGame()
    {
        SceneManager.LoadSceneAsync("Setting", LoadSceneMode.Additive);
    }

    public void CreditMenu()
    {
        SceneManager.LoadScene("CreditScene");
    }

    public void QuitGame()
    {
        // save
        Application.Quit();
    }
}
