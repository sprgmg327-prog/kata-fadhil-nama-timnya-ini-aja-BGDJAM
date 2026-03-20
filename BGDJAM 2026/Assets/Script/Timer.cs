using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; // Tambahkan ini untuk mengatur Scene

public class TimerScript : MonoBehaviour
{
    public float waktuTersisa = 10f; 
    public bool timerBerjalan = false;
    public TextMeshProUGUI teksTimer; 

    void Start()
    {
        timerBerjalan = true;
    }

  void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }

        if (timerBerjalan)
        {
            if (waktuTersisa > 0)
            {
                waktuTersisa -= Time.deltaTime;
                
                // PERBAIKAN: Gunakan Mathf.Max agar angka tidak minus sebelum ditampilkan
                UpdateTampilanWaktu(Mathf.Max(0, waktuTersisa));
            }
            else
            {
                Debug.Log("Waktu Habis!");
                waktuTersisa = 0;
                timerBerjalan = false;
                
                // Update tampilan terakhir ke 00:00 sebelum restart
                UpdateTampilanWaktu(0); 

                RestartLevel();
            }
        }
    }

    void UpdateTampilanWaktu(float waktu)
    {
        float menit = Mathf.FloorToInt(waktu / 60);
        float detik = Mathf.FloorToInt(waktu % 60);
        teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
    }

    // Fungsi bantuan untuk memuat ulang scene yang sedang aktif
    void RestartLevel()
    {
        // Mengambil index scene yang aktif sekarang lalu memuatnya ulang
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}