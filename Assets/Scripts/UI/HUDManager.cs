using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.UIElements.Experimental;
using System.Collections;
using System;
using System.Collections.Generic;
public class HUDManager : MonoBehaviour
{
    // TMp
    public TMP_Text textScore, textCombo, textTypingPrePlay, textTypingCompleted, textTimer;

    // image & texture
    public RawImage imageAvatarHud;
    public List<Texture2D> avatarCharacters;

    // Component
    public CanvasGroup comboUI, leftUI, rightUI, preplayUI, completedUI, pauseUI, gameOverUI, tutorialUI, fireAvatarCG;
    public Image healthFill, ultimateFill;

    // Animator
    public Animator animatorCombo, animatorUltimate;
    public enum StateComboAnimator
    {
        Idle, ScoreHide, ScoreShow
    }

    public static HUDManager Instance;

    public List<Texture2D> textureStageText;

    void Start()
    {
        Instance = this;
    }

    public void ChangeAvatarToUltimateMode()
    {
        if (imageAvatarHud.mainTexture == avatarCharacters[1]) return;
        imageAvatarHud.texture = avatarCharacters[1];
        fireAvatarCG.alpha = 1;

    }

    public void ChangeAvatarToidle()
    {
        if (imageAvatarHud.mainTexture == avatarCharacters[0]) return;
        imageAvatarHud.texture = avatarCharacters[0];
        fireAvatarCG.alpha = 0;
    }

    public void StartAnimationUlt()
    {
        animatorUltimate.Play("main", 0, 0f);
    }



    public IEnumerator ShowPrePlay(string text, int level, float delay, Action OnSuccess)
    {
        textTypingPrePlay.text = "";
        preplayUI.alpha = 1;

        foreach (char c in text)
        {
            yield return new WaitForSeconds(0.05f);
            textTypingPrePlay.text += c;
        }

        yield return new WaitForSeconds(delay);

        preplayUI.DOFade(0, 2f).OnComplete(() =>
        {
            OnSuccess?.Invoke();
        });

    }

    public IEnumerator ShowCompletedStage(string text, float delay, Action OnSuccess)
    {
        Debug.LogWarning("AKU UDAH KELUAR COMPLETE STAGE");
        textTypingCompleted.text = "";
        completedUI.DOFade(1, 1f);

        yield return new WaitForSeconds(delay / 2);

        foreach (char c in text)
        {
            yield return new WaitForSeconds(0.05f);
            textTypingCompleted.text += c;
        }

        yield return new WaitForSeconds(delay);

        OnSuccess?.Invoke();
    }

    public void ShowCombo(int comboBaru)
    {
        // animatorCombo.Play(StateComboAnimator.Idle.ToString());
        textCombo.text = comboBaru.ToString();
        animatorCombo.Play(StateComboAnimator.ScoreShow.ToString(), 0, 0f);
    }

    public void ShowGameOver()
    {
        gameOverUI.alpha = 1f;
        gameOverUI.interactable = true;
        gameOverUI.blocksRaycasts = true;
    }

    public void HideTutorial()
    {
        tutorialUI.alpha = 0f;
        tutorialUI.interactable = false;
        tutorialUI.blocksRaycasts = false;
    }

    public void ShowPause()
    {
        pauseUI.alpha = 1f;
        pauseUI.interactable = true;
        pauseUI.blocksRaycasts = true;

        Time.timeScale = 0f;
    }

    public void HidePause()
    {
        pauseUI.alpha = 0;
        pauseUI.interactable = false;
        pauseUI.blocksRaycasts = false;

        Time.timeScale = 1f;
    }

    public void HideCombo()
    {
        animatorCombo.Play(StateComboAnimator.ScoreHide.ToString(), 0, 0f);
    }

    public void UpdateBarHealth(float value)
    {
        healthFill.fillAmount = value;
    }

    public void UpdateBarUltimate(float value)
    {
        ultimateFill.fillAmount = value;
    }

    public void UpdateTextScore(int value)
    {
        textScore.text = value.ToString();
    }

    public void UpdateTextCombo(int value)
    {
        textCombo.text = value.ToString();
    }
}
