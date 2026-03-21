using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemStateChanger : MonoBehaviour
{
    [Header("Visual State (Sesudah)")]
    public Sprite spriteSetelahInteraksi; 

    [Header("Audio")]
    public AudioClip suaraInteraksi;

    [Header("Fisika")]
    [Tooltip("Jika dicentang, player bisa menembus benda ini setelah dipukul.")]
    public bool tembusSetelahInteraksi = true;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Collider2D col; // Referensi ke collider benda ini
    private bool sudahBerubah = false;

    public bool BisaDiinteraksi() => !sudahBerubah;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>(); // Mengambil collider (Box/Circle/Polygon)
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void AktifkanPerubahan()
    {
        if (sudahBerubah) return; 
        sudahBerubah = true;

        // 1. Ganti Gambar
        if (spriteSetelahInteraksi != null)
        {
            sr.sprite = spriteSetelahInteraksi;
        }

        // 2. Mainkan Suara
        if (suaraInteraksi != null)
        {
            audioSource.PlayOneShot(suaraInteraksi);
        }

        // 3. LOGIKA TEMBUS: Matikan collider atau jadikan trigger
        if (tembusSetelahInteraksi && col != null)
        {
            // Opsi paling aman: jadikan trigger agar tetap bisa dideteksi tapi tidak menghalangi jalan
            col.isTrigger = true; 
            
            // Atau kalau mau benar-benar hilang fisikanya:
            // col.enabled = false;
        }

        Debug.Log(gameObject.name + " sekarang bisa ditembus!");
    }
}