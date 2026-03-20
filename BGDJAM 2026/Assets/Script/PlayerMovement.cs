using UnityEngine;
using UnityEngine.SceneManagement; // Tambahkan ini untuk pindah scene
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Pengaturan Gerakan (Auto-Run)")]
    public float baseSpeed = 10f;
    private float currentSpeed;
    private int moveDirection = 1; // 1 = Kanan, -1 = Kiri

    [Header("Pengaturan Lompat")]
    public float highJumpForce = 16f;
    public float tapJumpMultiplier = 0.5f;
    public float trampolineJumpForce = 20f;
    private bool isGrounded;
    private bool canAirJump; // Bonus jump dari Bel

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
        // Gerak otomatis horizontal konstan
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        // Logika Lompat
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                // Lompat normal dari tanah
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
            }
            else if (canAirJump)
            {
                // Double Jump (hanya jika sudah pukul Bel)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
                canAirJump = false; // Reset bonus setelah digunakan di udara
                Debug.Log("Air Jump digunakan!");
            }
        }

        // Variabel Jump (Tap vs Hold) - Memotong kecepatan Y jika tombol dilepas
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * tapJumpMultiplier);
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Deteksi objek di sekitar attack point
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, destructibleLayer);

            foreach (Collider2D obj in hitObjects)
            {
                if (obj.CompareTag("Bel"))
                {
                    canAirJump = true; // Aktifkan bonus double jump
                    Debug.Log("Bel hancur! Bonus Air Jump aktif.");
                }
                
                Destroy(obj.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitObj = collision.gameObject;
        ContactPoint2D contact = collision.contacts[0];

        // 1. Logika KHUSUS Trampolin
        if (hitObj.CompareTag("Trampolin"))
        {
            if (contact.normal.y > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
                return; 
            }
        }

        // 2. Logika TABRAKAN SAMPING (Balik Arah)
        bool isWall = ((1 << hitObj.layer) & destructibleLayer) != 0;
        bool isObstacle = hitObj.CompareTag("Halangan") || hitObj.CompareTag("Trampolin");

        if (isWall || isObstacle)
        {
            float collisionDot = Vector2.Dot(contact.normal, new Vector2(moveDirection, 0));

            if (collisionDot < -0.5f) 
            {
                TurnAround();
            }
        }
    }

private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Kopi"))
        {
            currentSpeed *= 1.5f; 
            Destroy(collision.gameObject); 
        }

        // MODIFIKASI: Panggil Coroutine untuk memberi jeda
        if (collision.CompareTag("Dosen"))
        {
            StartCoroutine(WaitAndNextLevel(1.5f)); // Angka 1.5f adalah durasi jeda dalam detik
        }
    }

    // FUNGSI BARU: Coroutine untuk menunggu sebelum pindah scene
    IEnumerator WaitAndNextLevel(float delay)
{
    currentSpeed = 0;
    rb.linearVelocity = Vector2.zero;

    // Cari script TimerScript di dalam game dan hentikan jalannya
    TimerScript objekTimer = FindFirstObjectByType<TimerScript>();
    if (objekTimer != null)
    {
        objekTimer.timerBerjalan = false;
    }

    yield return new WaitForSeconds(delay);
    NextLevel();
}
    void NextLevel()
    {
        // Mengambil index scene sekarang dan ditambah 1
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Pastikan scene berikutnya terdaftar di Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Selamat! Ini adalah level terakhir.");
            // Kamu bisa tambahkan load scene "Main Menu" atau "End Credits" di sini
        }
    }

    void TurnAround()
    {
        moveDirection *= -1;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;
        rb.position += new Vector2(moveDirection * 0.1f, 0);
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}