using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Pengaturan Gerakan (Auto-Run)")]
    public float baseSpeed = 10f;
    private float currentSpeed;
    private int moveDirection = 1; 

    [Header("Pengaturan Lompat")]
    public float highJumpForce = 16f;
    public float tapJumpMultiplier = 0.5f;
    public float trampolineJumpForce = 20f;
    private bool isGrounded;
    private bool canAirJump; 

    [Header("Audio (SFX)")]
    public AudioSource audioSource; 
    public AudioClip jumpSound; 
    public AudioClip trampolineSound; 
    public AudioClip collisionSound; 
    public AudioClip winSound; 
    // attackSound sudah dihapus

    [Header("Audio (BGM)")]
    public AudioSource bgmSource; 
    public AudioClip backgroundMusic;

    [Header("Referensi Visual & Animasi")]
    public SpriteRenderer spriteRenderer; 
    public Animator animator;             
    public Sprite spriteMukul;          

    [Header("Deteksi Lantai")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Pengaturan Pukul (Serangan)")]
    public Transform attackPoint;
    public float attackRange = 0.8f;
    public LayerMask destructibleLayer;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
        rb.gravityScale = 3.5f; 
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        SetupBGM();
    }

    void SetupBGM()
    {
        if (bgmSource != null && backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;      
            bgmSource.playOnAwake = true; 
            bgmSource.Play();           
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleJump();
        HandleAttack();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
    }

    // --- LOGIKA GANTI SPRITE MANUAL (HANYA SAAT MUKUL) ---
    IEnumerator TemporarySpriteChange(Sprite targetSprite, float duration)
    {
        if (animator != null && spriteRenderer != null && targetSprite != null)
        {
            animator.enabled = false;          // Matikan animator agar sprite tidak tertimpa animasi
            spriteRenderer.sprite = targetSprite; 

            yield return new WaitForSeconds(duration); 

            animator.enabled = true;           // Nyalakan kembali animator
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
                PlaySFX(jumpSound);
            }
            else if (canAirJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
                canAirJump = false; 
                PlaySFX(jumpSound);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * tapJumpMultiplier);
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 1. Ganti Sprite ke frame memukul selama 0.3 detik
            StartCoroutine(TemporarySpriteChange(spriteMukul, 0.3f));

            // 2. Logika Hitbox (Deteksi objek yang dipukul)
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, destructibleLayer);

            foreach (Collider2D obj in hitObjects)
            {
                ItemStateChanger state = obj.GetComponent<ItemStateChanger>();
                if (state != null && state.BisaDiinteraksi())
                {
                    state.AktifkanPerubahan();
                    if (obj.CompareTag("Bel")) canAirJump = true;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitObj = collision.gameObject;
        ContactPoint2D contact = collision.contacts[0];

        if (hitObj.CompareTag("Trampolin") && contact.normal.y > 0.5f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
            PlaySFX(trampolineSound);
            return; 
        }

        bool isWall = ((1 << hitObj.layer) & destructibleLayer) != 0;
        bool isObstacle = hitObj.CompareTag("Halangan") || hitObj.CompareTag("Trampolin");

        if (isWall || isObstacle)
        {
            float collisionDot = Vector2.Dot(contact.normal, new Vector2(moveDirection, 0));
            if (collisionDot < -0.5f) 
            {
                PlaySFX(collisionSound);
                TurnAround();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kopi"))
        {
            ItemStateChanger state = collision.GetComponent<ItemStateChanger>();
            if (state != null && state.BisaDiinteraksi())
            {
                currentSpeed *= 1.5f; 
                state.AktifkanPerubahan();
            }
        }

        if (collision.CompareTag("Dosen"))
        {
            PlaySFX(winSound);
            if (bgmSource != null) bgmSource.Stop();
            StartCoroutine(WaitAndNextLevel(1.5f)); 
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    IEnumerator WaitAndNextLevel(float delay)
    {
        currentSpeed = 0;
        rb.linearVelocity = Vector2.zero;
        TimerScript timer = FindFirstObjectByType<TimerScript>();
        if (timer != null) timer.timerBerjalan = false;

        yield return new WaitForSeconds(delay);
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(nextIndex);
    }

    void TurnAround()
    {
        moveDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDirection;
        transform.localScale = scale;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        if (attackPoint != null) Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}