using UnityEngine;

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
    
    // ANTI-NYANGKUT & ANTI-LOMPAT DOUBLE
    private float jumpLockTimer; 
    private const float JUMP_LOCK_DURATION = 0.2f; // Kunci lompat selama 0.2 detik setelah balik badan

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
        
        // Kurangi timer setiap frame
        if (jumpLockTimer > 0) jumpLockTimer -= Time.deltaTime;

        HandleJump();
        HandleAttack();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        // Tambahan syarat: jumpLockTimer harus <= 0 agar bisa loncat
        if (Input.GetKeyDown(KeyCode.Space) && jumpLockTimer <= 0)
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (canAirJump)
            {
                Jump();
                canAirJump = false; 
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * tapJumpMultiplier);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, highJumpForce);
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, destructibleLayer);
            foreach (Collider2D obj in hitObjects)
            {
                if (obj.CompareTag("Bel")) canAirJump = true; 
                Destroy(obj.gameObject);
            }
        }
    }

    // Menggunakan OnCollisionStay2D agar lebih "galak" deteksi temboknya jika nyangkut
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trampolin")) return;

        bool isObstacle = ((1 << collision.gameObject.layer) & destructibleLayer) != 0 || 
                           collision.gameObject.CompareTag("Halangan");

        if (isObstacle)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Cek apakah tembok ada di depan arah lari
                float collisionDot = Vector2.Dot(contact.normal, new Vector2(moveDirection, 0));

                if (collisionDot < -0.5f) 
                {
                    TurnAround();
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
        }
    }

    void TurnAround()
    {
        moveDirection *= -1;
        
        // Reset lari dan kunci lompatan sebentar
        jumpLockTimer = JUMP_LOCK_DURATION; 

        // Balik visual
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;

        // Paksa geser sedikit agar collider tidak tumpang tindih (cegah nyangkut)
        rb.position += new Vector2(moveDirection * 0.2f, 0);
        
        // Hentikan momentum Y sebentar agar tidak "loncat" karena gesekan
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, 0);

        Debug.Log("Balik arah! Input lompat dikunci sebentar.");
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && rb.linearVelocity.y <= 0.1f) canAirJump = false;
    }
}