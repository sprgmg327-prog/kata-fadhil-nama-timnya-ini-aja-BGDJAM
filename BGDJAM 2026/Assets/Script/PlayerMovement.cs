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
    }

    void Update()
    {
        CheckGrounded();
        HandleJump();
        HandleAttack();
    }

    void FixedUpdate()
    {
        // Gerak lari otomatis
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
            }
            else if (canAirJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
                canAirJump = false; 
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
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, destructibleLayer);

            foreach (Collider2D obj in hitObjects)
            {
                ItemStateChanger state = obj.GetComponent<ItemStateChanger>();
                
                // Cek apakah item bisa dipukul dan belum pernah ditrigger
                if (state != null && state.BisaDiinteraksi())
                {
                    state.AktifkanPerubahan();
                    
                    if (obj.CompareTag("Bel"))
                    {
                        canAirJump = true;
                    }
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
            return; 
        }

        // Balik arah jika menabrak dinding
        bool isWall = ((1 << hitObj.layer) & destructibleLayer) != 0;
        bool isObstacle = hitObj.CompareTag("Halangan") || hitObj.CompareTag("Trampolin");

        if (isWall || isObstacle)
        {
            float collisionDot = Vector2.Dot(contact.normal, new Vector2(moveDirection, 0));
            if (collisionDot < -0.5f) TurnAround();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Efek Kopi: Nambah Speed 1.5x & Ganti Art (Sekali Saja)
        if (collision.CompareTag("Kopi"))
        {
            ItemStateChanger state = collision.GetComponent<ItemStateChanger>();
            
            if (state != null && state.BisaDiinteraksi())
            {
                currentSpeed *= 1.5f; 
                state.AktifkanPerubahan();
                Debug.Log("Speed naik! Sekarang: " + currentSpeed);
            }
        }

        if (collision.CompareTag("Dosen"))
        {
            StartCoroutine(WaitAndNextLevel(1.5f)); 
        }
    }

    // --- Fungsi Helper ---

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