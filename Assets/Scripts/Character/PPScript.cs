using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using DG.Tweening; // opsional, kalau kamu pakai DOTween

public class PPScript : MonoBehaviour
{
    [Header("Post Processing Volume")]
    public Volume volume;

    [Header("Effect Settings")]
    public float effectDuration = 0.5f;
    public float maxIntensity = 0.45f;

    private Vignette vignette;
    private Color defaultColor = Color.black;

    public static PPScript Instance;

    void Start()
    {
        Instance = this;
        // Ambil vignette dari profile
        volume.profile.TryGet(out vignette);
        if (vignette != null)
        {
            defaultColor = vignette.color.value;
            // vignette.intensity.value = 0f;
        }
    }

    // ⚡ Efek saat kena hit
    public void HitEffect()
    {
        if (vignette == null) return;

        StopAllCoroutines();
        StartCoroutine(VignetteFlash(Color.red));
    }

    // 🌿 Efek saat heal
    public void HealEffect()
    {
        if (vignette == null) return;

        StopAllCoroutines();
        StartCoroutine(VignetteFlash(Color.green));
    }

    // Coroutine efek vignette
    private IEnumerator VignetteFlash(Color flashColor)
    {
        vignette.color.value = flashColor;

        // Fade in
        vignette.intensity.value = maxIntensity;

        // Tahan sebentar
        yield return new WaitForSeconds(effectDuration / 2f);

        // Fade out
        // float t = 0;
        // while (t < effectDuration)
        // {
        //     t += Time.deltaTime;
        //     vignette.intensity.value = Mathf.Lerp(maxIntensity, 0f, t / effectDuration);
        //     yield return null;
        // }

        vignette.color.value = defaultColor;
        vignette.intensity.value = maxIntensity;
    }
}
