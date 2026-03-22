using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class EndGameCutsceneManager : MonoBehaviour
{
    [Header("Urutan Gambar (Total 9)")]
    public List<Image> daftarGambar; 
    
    [Header("Pengaturan Waktu")]
    public float durasiFade = 1.0f;   
    public float jedaAntarGambar = 0.8f; 
    public float jedaGantiHalaman = 2.5f; 

    [Header("UI Tambahan")]
    public GameObject teksPetunjuk; 

    private bool siapPindah = false;
    private bool sedangProsesLoad = false;

    void Start()
    {
        if (daftarGambar.Count != 9)
        {
            Debug.LogError("Masukin tepat 9 gambar di Inspector, Boss!");
            return;
        }

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
        // Deteksi input Enter kalau cutscene sudah beres
        if (siapPindah && !sedangProsesLoad)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                sedangProsesLoad = true;
                // Balik ke scene index 0
                SceneManager.LoadScene(0);
            }
        }
    }

    IEnumerator SequenceAnimasiPage()
    {
        for (int i = 0; i < daftarGambar.Count; i++)
        {
            yield return StartCoroutine(FadeInImage(daftarGambar[i]));

            if ((i + 1) % 4 == 0 && i != daftarGambar.Count - 1)
            {
                yield return new WaitForSeconds(jedaGantiHalaman);
                ClearHalaman(i - 3, i); 
                yield return new WaitForSeconds(0.5f);
            }
            else if (i == daftarGambar.Count - 1)
            {
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                yield return new WaitForSeconds(jedaAntarGambar);
            }
        }

        if (teksPetunjuk != null) teksPetunjuk.SetActive(true);
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
        Color finalColor = img.color;
        finalColor.a = 1;
        img.color = finalColor;
    }

    void ClearHalaman(int startIndex, int endIndex)
    {
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (daftarGambar[i] != null)
            {
                Color c = daftarGambar[i].color;
                c.a = 0;
                daftarGambar[i].color = c;
            }
        }
    }
}