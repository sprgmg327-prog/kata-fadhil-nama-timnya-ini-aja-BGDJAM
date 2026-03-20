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
        
        // Mengatur gravitasi agar jatuh lebih cepat (khas game platformer)
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
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
            }
            else if (canAirJump) // Lompat dari udara karena mukul Bel
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
                canAirJump = false; 
            }
        }

        // Saat tombol Spasi DILEPAS (Mekanik Tap vs Hold)
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * tapJumpMultiplier);
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, destructibleLayer);

            foreach (Collider2D obj in hitObjects)
            {
                if (obj.CompareTag("Bel"))
                {
                    canAirJump = true; 
                    Debug.Log("Bel dipukul! Dapat 1x lompat udara.");
                }
                
                Destroy(obj.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // CCTV 1: Tampilkan di Console nama dan tag objek yang ditabrak
        Debug.Log("Menabrak objek: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag);

        // Nabrak Trampolin -> Mental tinggi
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
            return; 
        }

        // Nabrak Tembok Tilemap (Halangan) ATAU rintangan yang lupa dipukul (Destructible)
        if (((1 << collision.gameObject.layer) & destructibleLayer) != 0 || collision.gameObject.CompareTag("Halangan"))
        {
            ContactPoint2D contact = collision.contacts[0];
            
            // CCTV 2: Tampilkan nilai pantulan
            Debug.Log("Nilai Normal X: " + contact.normal.x);

            // SYARAT DIPERBARUI: Diturunkan jadi > 0.1f agar lebih toleran terhadap bentuk Tilemap
            if (Mathf.Abs(contact.normal.x) > 0.1f) 
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
            currentSpeed *= 1.5f; 
            Destroy(collision.gameObject); 
        }
    }

    void TurnAround()
    {
        moveDirection *= -1;
        
        // Balik visual karakter (kiri-kanan)
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;

        // TAMBAHAN ANTI NYANGKUT: Dorong player sedikit ke arah jalan yang baru
        transform.position = new Vector2(transform.position.x + (moveDirection * 0.1f), transform.position.y);
        
        Debug.Log("Putar balik berhasil! Sekarang menghadap ke: " + (moveDirection == 1 ? "Kanan" : "Kiri"));
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