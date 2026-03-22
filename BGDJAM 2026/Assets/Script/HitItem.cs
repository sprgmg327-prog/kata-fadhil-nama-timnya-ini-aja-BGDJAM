using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemStateChanger : MonoBehaviour
{
    [Header("Visual State (Sesudah)")]
    public Sprite spriteSetelahInteraksi; 

    [Header("Animasi")]
    [Tooltip("Nama parameter Trigger di Animator (Contoh: 'KenaPukul')")]
    public string namaParameterAnimasi = "KenaPukul";

    [Header("Audio")]
    public AudioClip suaraInteraksi;

    [Header("Fisika")]
    [Tooltip("Jika dicentang, player bisa menembus benda ini setelah dipukul.")]
    public bool tembusSetelahInteraksi = true;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Collider2D col; 
    private Animator anim; // Tambahan: Referensi Animator
    private bool sudahBerubah = false;

    public bool BisaDiinteraksi() => !sudahBerubah;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>(); 
        anim = GetComponent<Animator>(); // Tambahan: Ambil komponen Animator jika ada
        
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

        // 3. Mainkan Animasi (Tambahan Baru)
        if (anim != null && !string.IsNullOrEmpty(namaParameterAnimasi))
        {
            anim.SetTrigger(namaParameterAnimasi);
        }

        // 4. LOGIKA TEMBUS
        if (tembusSetelahInteraksi && col != null)
        {
            col.isTrigger = true; 
        }

        Debug.Log(gameObject.name + " sekarang bisa ditembus dan animasi dimainkan!");
    }
}