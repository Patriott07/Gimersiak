using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
using System.Collections;
public class VideoSetting : MonoBehaviour
{

    [Header("UI Components")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    void Start()
    {

        StartCoroutine(LoadSettings());

    }

    IEnumerator LoadSettings()
    {
        yield return null;
        // === RESOLUTION SETUP ===
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("resolutionIndex", currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
        SetResolution(resolutionDropdown.value);

        // === FULLSCREEN ===
        bool isFull = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        fullscreenToggle.isOn = isFull;
        Screen.fullScreen = isFull;

        // === QUALITY ===
        int qualityIndex = PlayerPrefs.GetInt("qualityIndex", QualitySettings.GetQualityLevel());
        qualityDropdown.value = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex);

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // ===== FUNCTIONS =====

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resolutionIndex", index);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("qualityIndex", qualityIndex);
        PlayerPrefs.Save();
    }
}


