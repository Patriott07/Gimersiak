using System.Collections;
using UnityEngine;

public class vfxScript : MonoBehaviour
{
    float durationDelay = 0.1f;
    IEnumerator UnActiveInSecond(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine(UnActiveInSecond(durationDelay));
    }
}
