using UnityEngine;
using System.Collections; // <--- WAJIB ADA BARIS INI
using TMPro;

public class BlinkText : MonoBehaviour
{
    public float interval = 0.5f;
    private CanvasGroup cg;

    void Awake()
    {
        // Menggunakan CanvasGroup agar kedipan lebih stabil
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    // Fungsi ini akan jalan otomatis saat IntroCutscene memanggil SetActive(true)
    void OnEnable() 
    {
        StartCoroutine(DoBlink());
    }

    // Fungsi IEnumerator untuk logika kedip
    IEnumerator DoBlink()
    {
        while (true)
        {
            // Jika alpha 0 jadi 1, jika 1 jadi 0
            cg.alpha = (cg.alpha == 0) ? 1 : 0;
            yield return new WaitForSeconds(interval);
        }
    }
}