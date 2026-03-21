using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemStateChanger : MonoBehaviour
{
    [Header("Visual & Audio")]
    public Sprite spriteSetelahInteraksi; // Taruh art "setelah dipukul/diambil" di sini
    public AudioClip suaraInteraksi;      // Taruh file audio di sini

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private bool sudahBerinteraksi = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Setup AudioSource otomatis agar tidak lupa
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.playOnAwake = false;
    }

    public void AktifkanPerubahan()
    {
        if (sudahBerinteraksi) return; // Mencegah bunyi/ganti gambar berkali-kali

        sudahBerinteraksi = true;

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

        Debug.Log(gameObject.name + " berubah state!");
    }
}