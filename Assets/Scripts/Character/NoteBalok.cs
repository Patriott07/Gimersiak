using System.Collections;
using UnityEngine;

public class NoteBalok : MonoBehaviour
{
    public int life = 1;
    public AudioSource audioSource;
    public AudioClip audioClip;
    public Color baseColor;

    bool isDestroying = false;

    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;

    public int Life
    {
        get => life;
        set
        {
            life = value;
            UpdateBlock();
        }
    }

    public void UpdateBlock()
    {
        Debug.Log("Hitt balock note");
        if (Life <= 0)
        {
            isDestroying = true;
            GameManager.Instance.totalBlock--;
            Destroy(gameObject);
            // StartCoroutine(DelayDestroy());
        }
    }


    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
        boxCollider2D = gameObject.GetComponent<BoxCollider2D>();
    }

    public IEnumerator HitEffect()
    {
        if (life <= 0)
        {
            GameManager.Instance.PlaySound();
            yield return false;
        }

        if (!isDestroying)
        {
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.volume = Random.Range(0.9f, 1.2f);
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.Play();

            // cek lagi sebelum mengembalikan warna
            spriteRenderer.color = new Color32(255, 206, 206, 255);
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = baseColor;
        }
    }
}
