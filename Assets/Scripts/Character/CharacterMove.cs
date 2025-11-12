using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    public static CharacterMove Instance;
    // move
    public float moveSpeed = 5f;
    public Vector2 movement;

    public Rigidbody2D rb;

    // blink / dash
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    bool isDashing = false;
    float dashTime = 0.3f;
    float dashCooldownTimer = 1f;

    public bool isCanMove = true;

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
         if (!GameManager.Instance.isPlay) return;
        if (!isCanMove)
            return;
        // Ambil input player
        movement.x = Input.GetAxisRaw("Horizontal");
        // movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        // Dash input
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.L)) && !isDashing && dashCooldownTimer <= 0)
        {
            isDashing = true;
            dashTime = dashDuration;
            dashCooldownTimer = dashCooldown + dashDuration;
        }

        // Timer untuk dash dan cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isPlay) return;
        if (isDashing) // jika isDashing true
        {
            rb.linearVelocity = movement * getDashSpeed(moveSpeed);

            dashTime -= Time.fixedDeltaTime; // kurangi durasi 
            if (dashTime <= 0)
            {
                isDashing = false; // kalo time habis maka set is dashing false
            }
        }
        else
        {
            // basic move
            rb.linearVelocity = movement * moveSpeed;
        }
    }

    float getDashSpeed(float movespeedBase)
    {
        return movespeedBase * 4;
    }
}
