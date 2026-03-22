using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class EndGameCutsceneManager : MonoBehaviour
{
    [Header("Urutan Gambar (Wajib 12)")]
    public List<Image> daftarGambar; 
    
    [Header("Pengaturan Waktu")]
    public float durasiFade = 1.0f;   
    public float jedaAntarGambar = 0.8f; 
    [Tooltip("Waktu tunggu setelah 4 gambar ngumpul, sebelum layar dibersihkan buat 4 gambar berikutnya.")]
    public float jedaGantiHalaman = 2.5f; 

    [Header("UI Tambahan")]
    public GameObject teksPetunjuk; 
    public string namaSceneMainMenu = "MainMenu"; 

    private bool siapPindah = false;
    private bool sedangProsesLoad = false;

    void Start()
    {
        if (daftarGambar.Count != 12)
        {
            Debug.LogError("Masukin 12 gambar dulu di Inspector ngab!");
            return;
        }

        // Set transparan semua di awal
        foreach (Image img in daftarGambar)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = 0;
                img.color = c;
                img.enabled = true; 
            }
        }

        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);

        StartCoroutine(SequenceAnimasiPage());
    }

    void Update()
    {
        if (siapPindah && !sedangProsesLoad)
        {
            // Kalau udah selesai semua, tekan Enter pindah scene
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                sedangProsesLoad = true;
                SceneManager.LoadScene(namaSceneMainMenu);
            }
        }
    }

    IEnumerator SequenceAnimasiPage()
    {
        for (int i = 0; i < daftarGambar.Count; i++)
        {
            // 1. Munculin gambar ke-i
            yield return StartCoroutine(FadeInImage(daftarGambar[i]));

            // 2. Cek apakah ini gambar ke-4, ke-8, atau ke-12 (index 3, 7, 11)
            if ((i + 1) % 4 == 0)
            {
                // Kasih waktu agak lama biar pemain bisa baca/lihat 4 gambar yang udah kumpul
                yield return new WaitForSeconds(jedaGantiHalaman);

                // Kalau BUKAN gambar terakhir (ke-12), bersihkan layar untuk 4 gambar berikutnya
                if (i != daftarGambar.Count - 1)
                {
                    ClearHalaman(i - 3, i); // Hapus 4 gambar barusan
                    yield return new WaitForSeconds(0.5f); // Jeda bentar pas layar kosong
                }
            }
            else
            {
                // Kalau belum 4 gambar, kasih jeda biasa sebelum gambar selanjutnya muncul
                yield return new WaitForSeconds(jedaAntarGambar);
            }
        }

        // 3. Semua 12 gambar udah beres, munculin teks kedip
        if (teksPetunjuk != null) 
        {
            teksPetunjuk.SetActive(true);
        }
        siapPindah = true;
    }

    IEnumerator FadeInImage(Image img)
    {
        if (img == null) yield break;
        float counter = 0;
        while (counter < durasiFade)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, counter / durasiFade);
            Color c = img.color;
            c.a = alpha;
            img.color = c;
            yield return null; 
        }
        // Pastikan alpha beneran mentok di 1
        Color finalColor = img.color;
        finalColor.a = 1;
        img.color = finalColor;
    }

    void ClearHalaman(int startIndex, int endIndex)
    {
        // Looping buat ngembaliin alpha 4 gambar sebelumnya jadi 0 (hilang)
        for (int i = startIndex; i <= endIndex; i++)
        {
            Color c = daftarGambar[i].color;
            c.a = 0;
            daftarGambar[i].color = c;
        }
    }
}