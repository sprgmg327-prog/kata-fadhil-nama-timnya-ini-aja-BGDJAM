using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class IntroCutscene : MonoBehaviour
{
    [Header("Urutan Gambar")]
    public List<Image> daftarGambar; 
    
    [Tooltip("Waktu diam setelah satu gambar selesai muncul sebelum lanjut ke gambar berikutnya.")]
    public float jedaAntarGambar = 1.5f; 
    
    [Tooltip("Seberapa lambat gambar memudar dari transparan ke jelas.")]
    public float durasiFade = 2.0f;     

    [Header("UI Tambahan")]
    public GameObject teksPetunjuk; 

    private bool siapPindah = false;
    private bool sedangProses = false;

    void Start()
    {
        // 1. Persiapan Awal
        foreach (Image img in daftarGambar)
        {
            // Set semua gambar jadi transparan di awal
            Color c = img.color;
            c.a = 0;
            img.color = c;
            img.enabled = true; 
        }

        // Pastikan teks "Press ENTER" mati di awal
        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);

        // 2. Mulai urutan animasi
        StartCoroutine(SequenceAnimasi());
    }

    void Update()
    {
        // 3. Deteksi Input ENTER untuk pindah ke LVL 1
        if (siapPindah && !sedangProses)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                MulaiGame();
            }
        }
    }

    IEnumerator SequenceAnimasi()
    {
        // Loop untuk memunculkan gambar satu per satu
        for (int i = 0; i < daftarGambar.Count; i++)
        {
            // Munculkan gambar dengan efek Fade In dan TUNGGU sampai selesai
            yield return StartCoroutine(FadeInImage(daftarGambar[i]));
            
            // Beri jeda diam sebentar agar pemain bisa melihat gambarnya
            yield return new WaitForSeconds(jedaAntarGambar);
        }

        // Setelah semua gambar muncul, tunggu 1 detik baru munculkan teks petunjuk
        yield return new WaitForSeconds(1.0f);

        siapPindah = true;
        if (teksPetunjuk != null) 
        {
            teksPetunjuk.SetActive(true);
        }
    }

    // Fungsi internal untuk memproses pemudaran (Fade)
    IEnumerator FadeInImage(Image img)
    {
        float counter = 0;
        while (counter < durasiFade)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, counter / durasiFade);
            
            Color c = img.color;
            c.a = alpha;
            img.color = c;
            
            yield return null; // Tunggu ke frame berikutnya
        }
    }

    void MulaiGame()
    {
        sedangProses = true;
        
        // Pindah ke scene berikutnya sesuai urutan di Build Settings (Index 1)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Cek dulu apakah scene berikutnya ada, kalau ada baru pindah
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("Scene berikutnya tidak ditemukan di Build Settings!");
        }
    }
}