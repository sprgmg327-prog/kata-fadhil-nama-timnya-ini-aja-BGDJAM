using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class IntroCutscene : MonoBehaviour
{
    [Header("Urutan Gambar")]
    public List<Image> daftarGambar; 
    public float jedaMuncul = 0.8f; // Kecepatan muncul gambar (0.8 detik)

    [Header("UI Tambahan")]
    public GameObject teksPetunjuk; // Objek teks "Press ENTER to Start"
    public string namaSceneTujuan = "Level1";

    private bool siapPindah = false;
    private bool sedangProses = false;

    void Start()
    {
        // Setup awal: Sembunyikan semua gambar & teks petunjuk
        foreach (Image img in daftarGambar)
        {
            img.enabled = false;
        }

        if (teksPetunjuk != null) teksPetunjuk.SetActive(false);

        // Mulai urutan muncul gambar
        StartCoroutine(SequenceAnimasi());
    }

    void Update()
    {
        // Cek input ENTER hanya jika semua gambar sudah muncul
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
        for (int i = 0; i < daftarGambar.Count; i++)
        {
            yield return new WaitForSeconds(jedaMuncul);
            daftarGambar[i].enabled = true;
            
            // Efek suara muncul (opsional)
            // AudioSource.PlayClipAtPoint(suaraMuncul, transform.position);
        }

        // Setelah gambar terakhir muncul
        siapPindah = true;
        if (teksPetunjuk != null) teksPetunjuk.SetActive(true);
    }

    void MulaiGame()
    {
        sedangProses = true;
        SceneManager.LoadScene("LVL 1");
    }
}