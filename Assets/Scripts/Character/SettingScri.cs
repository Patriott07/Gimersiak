using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingScri : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] List<Slider> sliders; // master, music, sfx

    [SerializeField] List<CanvasGroup> canvasGroups; // audio, video
    [SerializeField] List<TMP_Text> textMenus;

    void Start()
    {
        sliders[0].value = PlayerPrefs.GetFloat("master");
        sliders[1].value = PlayerPrefs.GetFloat("music");
        sliders[2].value = PlayerPrefs.GetFloat("sfx");
    }

    public void SetMusicVolume()
    {
        float value = sliders[1].value;
        audioMixer.SetFloat("volume_music", value);
        PlayerPrefs.SetFloat("music", value);
        PlayerPrefs.Save();

        Debug.Log("music Volume Saved: " + PlayerPrefs.GetFloat("music"));
    }

    public void SetSfxVolume()
    {
        float value = sliders[2].value;
        audioMixer.SetFloat("volume_sfx", value);
        PlayerPrefs.SetFloat("sfx", value);
        PlayerPrefs.Save();

        Debug.Log("sfx Volume Saved: " + PlayerPrefs.GetFloat("sfx"));
    }

    public void SetMasterVolume()
    {
        float value = sliders[0].value;
        audioMixer.SetFloat("volume_master", value);
        PlayerPrefs.SetFloat("master", value);
        PlayerPrefs.Save();

        Debug.Log("Master Volume Saved: " + PlayerPrefs.GetFloat("master"));
    }

    public void SetMenu(int i)
    {
        foreach (CanvasGroup CG in canvasGroups)
        {
            CG.alpha = 0;
            CG.interactable = false;
            CG.blocksRaycasts = false;
        }

        foreach (TMP_Text text in textMenus)
        {
            text.color = Color.white;
        }

        textMenus[i].color = new Color32(247, 210, 59, 255);
        canvasGroups[i].alpha = 1;
        canvasGroups[i].interactable = true;
        canvasGroups[i].blocksRaycasts = true;
        PlayerPrefs.Save();
    }


    public void Back()
    {
        SceneManager.UnloadSceneAsync("Setting");
        PlayerPrefs.Save();
    }

}
