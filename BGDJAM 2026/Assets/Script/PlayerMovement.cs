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
    public float wallJumpBoost = 1.2f; // Tambahan tenaga pas mantul tembok (1.2x)
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
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        // Menggunakan GetKey (bukan GetKeyDown) supaya kalau di-hold tetap respon
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump(highJumpForce);
            }
            else if (canAirJump)
            {
                Jump(highJumpForce);
                canAirJump = false; 
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * tapJumpMultiplier);
        }
    }

    void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            Jump(trampolineJumpForce);
            return;
        }

        bool isObstacle = ((1 << collision.gameObject.layer) & destructibleLayer) != 0 || 
                           collision.gameObject.CompareTag("Halangan");

        if (isObstacle)
        {
            ContactPoint2D contact = collision.contacts[0];
            float collisionDot = Vector2.Dot(contact.normal, new Vector2(moveDirection, 0));

            if (collisionDot < -0.5f) 
            {
                TurnAround();
            }
        }
    }

    void TurnAround()
    {
        moveDirection *= -1;
        
        // --- INI KUNCINYA ---
        // Kalau player lagi nahan Spasi PAS nabrak tembok, kasih dorongan loncat instan
        if (Input.GetKey(KeyCode.Space))
        {
            Jump(highJumpForce * wallJumpBoost); 
            Debug.Log("Wall Jump Berhasil!");
        }

        // Balik visual
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;

        // Geser posisi biar gak nyangkut di dalam collider tembok
        rb.position += new Vector2(moveDirection * 0.2f, 0);
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && rb.linearVelocity.y <= 0.1f) canAirJump = false;
    }
}