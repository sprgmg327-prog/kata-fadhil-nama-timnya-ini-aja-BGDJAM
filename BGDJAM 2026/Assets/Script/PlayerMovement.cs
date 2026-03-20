using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Pengaturan Gerakan (Auto-Run)")]
    public float baseSpeed = 10f; // 10 tiles per detik
    private float currentSpeed;
    private int moveDirection = 1; // 1 = Kanan, -1 = Kiri

    [Header("Pengaturan Lompat")]
    public float highJumpForce = 16f; // Kekuatan lompat ditahan (tinggi)
    public float tapJumpMultiplier = 0.5f; // Pengurangan kecepatan jika tombol dilepas cepat
    public float trampolineJumpForce = 20f; // Kekuatan mental trampolin
    private bool isGrounded;
    private bool canAirJump; // Untuk mekanik Bel (lompat di udara)

    [Header("Deteksi Lantai")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Pengaturan Pukul (Serangan)")]
    public Transform attackPoint;
    public float attackRange = 0.8f;
    public LayerMask destructibleLayer; // Layer khusus objek yang bisa dihancurkan

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
        
        // Mengatur gravitasi agar jatuh lebih cepat (khas game platformer agar tidak melayang lambat)
        rb.gravityScale = 3.5f; 
    }

    void Update()
    {
        CheckGrounded();
        HandleJump();
        HandleAttack();
    }

    void FixedUpdate()
    {
        // Auto-run: Gerak konstan di sumbu X, biarkan sumbu Y dipengaruhi gravitasi
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        // Saat tombol Spasi DITEKAN
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                // Lompat dari tanah
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
            }
            else if (canAirJump)
            {
                // Lompat di udara (didapat dari mukul Bel)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
                canAirJump = false; // Jatah lompat udara habis
            }
        }

        // Saat tombol Spasi DILEPAS (Mekanik Tap vs Hold)
        // Jika dilepas saat player masih bergerak naik, potong kecepatan naiknya jadi setengah
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * tapJumpMultiplier);
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Buat lingkaran deteksi di titik pukulan
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, destructibleLayer);

            foreach (Collider2D obj in hitObjects)
            {
                // Kalau yang dipukul adalah Bel
                if (obj.CompareTag("Bel"))
                {
                    canAirJump = true; // Beri jatah 1x lompat di udara
                    Debug.Log("Bel dipukul! Dapat 1x lompat udara.");
                }
                
                // Hancurkan objeknya (berlaku untuk Pintu, Banner, Kursi, Temen, Tempat Sampah, Bel)
                Destroy(obj.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nabrak Trampolin -> Mental tinggi
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
            return; // Hentikan pengecekan di bawahnya
        }

        // Nabrak Objek/Tembok -> Berbalik arah
        // Jika nabrak objek berlayer "Destructible" (karena telat mukul) ATAU tag "Halangan" (seperti Meja)
        if (((1 << collision.gameObject.layer) & destructibleLayer) != 0 || collision.gameObject.CompareTag("Halangan"))
        {
            // Pastikan kita nabrak dari samping, bukan numpang di atasnya
            ContactPoint2D contact = collision.contacts[0];
            if (Mathf.Abs(contact.normal.x) > 0.5f) 
            {
                TurnAround();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Menyentuh Vending Machine (Power-up Kopi)
        if (collision.CompareTag("Kopi"))
        {
            currentSpeed *= 1.5f; // Kecepatan jadi 1.5x lipat
            Destroy(collision.gameObject); // Vending machine hilang setelah diambil
        }
    }

    void TurnAround()
    {
        moveDirection *= -1;
        
        // Balik visual karakter (kiri-kanan)
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    void CheckGrounded()
    {
        // Ngecek apakah lingkaran di kaki menyentuh layer Ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Reset speed jika butuh, tapi di sini kecepatan kopi permanen kecuali diatur ulang
    }

    // Untuk menampilkan lingkaran deteksi kaki dan pukulan di Editor (Warna Merah & Hijau)
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