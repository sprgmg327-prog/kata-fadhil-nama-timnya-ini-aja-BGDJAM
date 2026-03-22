using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Tambahkan ini jika menggunakan UI Image
using System.Collections; 

public class TimerScript : MonoBehaviour
{
    public float waktuTersisa = 16f; // Diubah ke 16 detik sesuai permintaan
    public bool timerBerjalan = false;
    public TextMeshProUGUI teksTimer; // Tetap ada jika ingin backup teks

    [Header("Sprite Timer Settings")]
    public Image tampilanSpriteUI;    // Slot untuk UI Image yang akan ganti sprite
    public Sprite[] daftarSprite;     // Masukkan 11 sprite kamu di sini (Urutan dari awal ke akhir)

    [Header("Audio Settings")]
    public AudioSource audioSource;    
    public AudioClip restartSound;    

    void Start()
    {
        timerBerjalan = true;
        UpdateTampilanWaktu(waktuTersisa);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevelInstan();
        }

        if (timerBerjalan)
        {
            if (waktuTersisa > 0)
            {
                waktuTersisa -= Time.deltaTime;
                UpdateTampilanWaktu(Mathf.Max(0, waktuTersisa));
            }
            else
            {
                waktuTersisa = 0;
                timerBerjalan = false;
                UpdateTampilanWaktu(0); 
                StartCoroutine(PlaySoundAndRestart());
            }
        }
    }

    void UpdateTampilanWaktu(float waktu)
    {
        // 1. Update Teks (Backup)
        float menit = Mathf.FloorToInt(waktu / 60);
        float detik = Mathf.FloorToInt(waktu % 60);
        if (teksTimer != null) 
            teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);

        // 2. Update Sprite berdasarkan progress waktu
        if (tampilanSpriteUI != null && daftarSprite.Length > 0)
        {
            // Total durasi 16 detik dibagi 11 sprite
            // Kita hitung index: (Waktu Berjalan / Total Waktu) * Jumlah Sprite
            float progress = 1f - (waktu / 16f); // 0 saat mulai, 1 saat habis
            int index = Mathf.FloorToInt(progress * daftarSprite.Length);
            
            // Batasi agar index tidak out of bounds
            index = Mathf.Clamp(index, 0, daftarSprite.Length - 1);
            
            tampilanSpriteUI.sprite = daftarSprite[index];
        }
    }

    void RestartLevelInstan()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator PlaySoundAndRestart()
    {
        timerBerjalan = false;
        if (audioSource != null && restartSound != null)
        {
            audioSource.PlayOneShot(restartSound);
            yield return new WaitForSeconds(0.8f); 
        }
        RestartLevelInstan();
    }
}