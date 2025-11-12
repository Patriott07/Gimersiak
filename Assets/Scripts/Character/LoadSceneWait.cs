using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneWait : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float delay;
    [SerializeField] string sceneName;
    void Start()
    {
        StartCoroutine(LoadSceneButWait());
    }

    IEnumerator LoadSceneButWait()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
