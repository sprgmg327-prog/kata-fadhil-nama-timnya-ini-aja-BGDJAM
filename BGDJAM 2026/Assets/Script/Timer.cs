using UnityEngine;
using TMPro; // Penting untuk TextMeshPro

public class TimerScript : MonoBehaviour
{
    public float waktuTersisa = 10f; // Set durasi dalam detik
    public bool timerBerjalan = false;
    public TextMeshProUGUI teksTimer; // Drag komponen Text ke sini di Inspector

    void Start()
    {
        // Mulai timer otomatis saat game dimulai
        timerBerjalan = true;
    }

    void Update()
    {
        if (timerBerjalan)
        {
            if (waktuTersisa > 0)
            {
                waktuTersisa -= Time.deltaTime;
                UpdateTampilanWaktu(waktuTersisa);
            }
            else
            {
                Debug.Log("Waktu Habis!");
                waktuTersisa = 0;
                timerBerjalan = false;
                // Kamu bisa panggil fungsi Game Over di sini
            }
        }
    }

    void UpdateTampilanWaktu(float waktu)
    {
        // Menghitung menit dan detik
        float menit = Mathf.FloorToInt(waktu / 60);
        float detik = Mathf.FloorToInt(waktu % 60);

        // Format string agar tampil 00:00
        teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
    }
}