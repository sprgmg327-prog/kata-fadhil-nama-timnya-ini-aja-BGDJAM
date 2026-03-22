using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using System.Collections; 

public class TimerScript : MonoBehaviour
{
    public float waktuTersisa = 10f; 
    public bool timerBerjalan = false;
    public TextMeshProUGUI teksTimer; 

    [Header("Audio Settings")]
    public AudioSource audioSource;    
    public AudioClip restartSound;    // Suara ini hanya untuk waktu habis

    void Start()
    {
        timerBerjalan = true;
    }

    void Update()
    {
        // 1. RESTART MANDIRI: Langsung pindah (Instan)
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
                // 2. WAKTU HABIS: Panggil Coroutine dengan suara & delay
                Debug.Log("Waktu Habis!");
                waktuTersisa = 0;
                timerBerjalan = false;
                UpdateTampilanWaktu(0); 

                StartCoroutine(PlaySoundAndRestart());
            }
        }
    }

    void UpdateTampilanWaktu(float waktu)
    {
        float menit = Mathf.FloorToInt(waktu / 60);
        float detik = Mathf.FloorToInt(waktu % 60);
        teksTimer.text = string.Format("{0:00}:{1:00}", menit, detik);
    }

    // Fungsi untuk restart langsung (tanpa suara/delay)
    void RestartLevelInstan()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Coroutine khusus saat waktu habis (dengan suara & delay)
    IEnumerator PlaySoundAndRestart()
    {
        timerBerjalan = false;

        if (audioSource != null && restartSound != null)
        {
            audioSource.PlayOneShot(restartSound);
            
            // Tunggu selama durasi suara agar tidak terpotong
            yield return new WaitForSeconds(0.8f); 
        }

        RestartLevelInstan();
    }
}