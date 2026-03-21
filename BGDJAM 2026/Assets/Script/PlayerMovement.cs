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
                Debug.Log("Air Jump digunakan!");
            }
        }

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
                // Ambil script ganti state
                ItemStateChanger stateChanger = obj.GetComponent<ItemStateChanger>();

                if (stateChanger != null)
                {
                    stateChanger.AktifkanPerubahan();
                    
                    // Logika khusus jika itu Bel
                    if (obj.CompareTag("Bel"))
                    {
                        canAirJump = true;
                        Debug.Log("Bel dipukul! Air Jump aktif.");
                    }
                }
                else
                {
                    // Jika tidak ada script ganti state, hancurkan saja (fallback)
                    Destroy(obj.gameObject);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitObj = collision.gameObject;
        ContactPoint2D contact = collision.contacts[0];

        if (hitObj.CompareTag("Trampolin"))
        {
            if (contact.normal.y > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, trampolineJumpForce);
                return; 
            }
        }

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
        // Logika Kopi (Ditabrak, Ganti Art + Suara, Kecepatan nambah, GAK ILANG)
        if (collision.CompareTag("Kopi"))
        {
            currentSpeed *= 1.2f; // Nambah speed dikit
            
            ItemStateChanger stateChanger = collision.GetComponent<ItemStateChanger>();
            if (stateChanger != null)
            {
                stateChanger.AktifkanPerubahan();
            }
        }

        if (collision.CompareTag("Dosen"))
        {
            StartCoroutine(WaitAndNextLevel(1.5f)); 
        }
    }

    IEnumerator WaitAndNextLevel(float delay)
    {
        currentSpeed = 0;
        rb.linearVelocity = Vector2.zero;

        TimerScript objekTimer = FindFirstObjectByType<TimerScript>();
        if (objekTimer != null) objekTimer.timerBerjalan = false;

        yield return new WaitForSeconds(delay);
        NextLevel();
    }

    void NextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
    }

    void TurnAround()
    {
        moveDirection *= -1;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;
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