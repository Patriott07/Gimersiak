using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenSc : MonoBehaviour
{
    [SerializeField] string nextScene;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] AudioSource audioSource;

    bool isStart = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isStart)
        {
            isStart = true;

            StartCoroutine(NextSceneLoad(nextScene));
        }
    }

    IEnumerator NextSceneLoad(string name)
    {
        yield return null;
        if (audioSource != null) audioSource.Stop();
        canvasGroup.DOFade(0, 0.9f).OnComplete(() =>
        {
            SceneManager.LoadScene(nextScene);
        });

    }
}
