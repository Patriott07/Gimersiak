using System.Collections;
using UnityEngine;
using DG.Tweening;
public class CharacterHit : MonoBehaviour
{
    [SerializeField] DetectorBall detectorBallSc;
    bool isCanHit = true, isHitAttack = false;
    float durationHitCooldown = 1f;
    public float strengthHit = 15f;


    void Start()
    {

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && !isHitAttack)
        {
            // do attacj
            AddForceToBall();
        }

        if (isHitAttack)
        {
            durationHitCooldown -= Time.deltaTime;
            if (durationHitCooldown <= 0)
            {
                isHitAttack = false;
            }
        }
    }

    void AddForceToBall()
    {
         if (!GameManager.Instance.isPlay) return;
        if (detectorBallSc.rbBall == null) return;
        if (!isCanHit) return;
        isCanHit = false;
        StartCoroutine(IenumaratorAddForceToBall());
    }
    IEnumerator IenumaratorAddForceToBall()
    {
        
        Time.timeScale = 1f;
        // Sebelum freeze, lakukan hit
        Debug.Log("Hitting toll 2");
        detectorBallSc.rbBall.AddForce(new Vector2(CharacterMove.Instance.movement.x * 15f, strengthHit), ForceMode2D.Impulse);
        Camera.main.DOShakePosition(0.2f, 0.3f, 10, 60, true);
        
        // Freeze time (pause effect)
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.2f); // Real time delay, tetap jalan walaupun timescale = 0

        // Kembalikan time scale ke normal
        Time.timeScale = 1f;

        // Delay lagi dengan efek normal (jika perlu, setelah time nyala)
        yield return new WaitForSeconds(0.2f);
        isCanHit = true;

    }
}
