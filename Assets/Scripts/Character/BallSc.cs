using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BallSc : MonoBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] private float enemyDamage = 25f;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    IEnumerator OnTouchBrick(GameObject noteblock)
    {
        Camera.main.DOShakePosition(0.2f, 0.5f, 10, 60, false);
        NoteBalok noteBalokSc = noteblock.GetComponent<NoteBalok>();

        noteBalokSc.Life--;
        StartCoroutine(noteBalokSc.HitEffect());

        GameManager.Instance.AddScore();
        GameManager.Instance.Ultimate += 0.02f;
        GameManager.Instance.combo++;
        HUDManager.Instance.ShowCombo(GameManager.Instance.combo);

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 0.7f;
        yield return new WaitForSecondsRealtime(0.5f);
        Camera.main.transform.position = new Vector3(-1.05f, 0, -10);

        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
    }

    IEnumerator OnTouchEnemy(GameObject enemy)
    {
        Camera.main.DOShakePosition(0.15f, 0.3f, 10, 60, false);
        
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null && enemyHealth.IsAlive)
        {
            enemyHealth.TakeDamage(enemyDamage);
            
            GameManager.Instance.AddScore();
            GameManager.Instance.Ultimate += 0.015f;
            GameManager.Instance.combo++;
            HUDManager.Instance.ShowCombo(GameManager.Instance.combo);
        }

        Time.timeScale = 0.8f;
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1f;
    }

    IEnumerator OnTouchWall()
    {
        Time.timeScale = 0.6f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.CompareTag("wall"))
        {
            StartCoroutine(OnTouchWall());
        }

        if (collider.gameObject.CompareTag("brick"))
        {
            if (!GameManager.Instance.isPlay) return;
            Debug.Log("kena brick");
            GameManager.Instance.SpawnVfxExplode(transform.position);
            StartCoroutine(OnTouchBrick(collider.gameObject));
        }

        if (collider.gameObject.CompareTag("Enemy"))
        {
            if (!GameManager.Instance.isPlay) return;
            Debug.Log("kena enemy");
            GameManager.Instance.SpawnVfxExplode(transform.position);
            StartCoroutine(OnTouchEnemy(collider.gameObject));
        }

        if (collider.gameObject.CompareTag("shield"))
        {
            if (!GameManager.Instance.isPlay) return;
            ShieldScript shieldScript = collider.gameObject.GetComponent<ShieldScript>();
            shieldScript.shield--;
        }

        if (collider.gameObject.CompareTag("ship"))
        {
            if (!GameManager.Instance.isPlay) return;

            PPScript.Instance.HitEffect();

            Camera.main.DOShakePosition(0.2f, 0.5f, 10, 60, false);
            GameManager.Instance.health--;
            HUDManager.Instance.UpdateBarHealth(GameManager.Instance.health / 5f);

            rb.AddForce(new Vector2(0, 90f));

            HUDManager.Instance.HideCombo();
            HUDManager.Instance.textCombo.text = 0.ToString();
            GameManager.Instance.combo = 0;
        }
    }
}