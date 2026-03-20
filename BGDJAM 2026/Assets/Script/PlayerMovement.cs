using UnityEngine;

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
        
        // Gravitasi tinggi agar game terasa snappy (tidak melayang)
        rb.gravityScale = 3.5f; 
        
        // Pastikan rotasi Z terkunci agar player tidak terguling saat nabrak
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
        // Gerak otomatis konstan
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

        // Variabel Jump (Tap vs Hold)
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
                    Debug.Log("Bel dipukul! Air Jump aktif.");
                }
                
                Destroy(obj.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Cek Trampolin (Prioritas utama)
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
            return;
        }

        // 2. Cek Tabrakan Tembok (Layer Destructible atau Tag Halangan)
        bool isObstacle = ((1 << collision.gameObject.layer) & destructibleLayer) != 0 || 
                           collision.gameObject.CompareTag("Halangan");

        if (isObstacle)
        {
            ContactPoint2D contact = collision.contacts[0];
            
            // Logika Dot Product: 
            // Jika kita bergerak ke kanan (1,0) dan normal tembok ke kiri (-1,0), 
            // hasilnya adalah -1 (Tabrakan depan sempurna).
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
    }

    void TurnAround()
    {
        moveDirection *= -1;
        
        // Balik visual
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;

        // Sedikit dorongan (offset) agar tidak overlap dengan collider tembok setelah putar balik
        rb.position += new Vector2(moveDirection * 0.15f, 0);
        
        Debug.Log("Mantul! Arah sekarang: " + (moveDirection == 1 ? "Kanan" : "Kiri"));
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Reset air jump jika menyentuh tanah (opsional, tergantung desain game kamu)
        if (isGrounded) canAirJump = false;
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