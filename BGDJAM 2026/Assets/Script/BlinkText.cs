using UnityEngine;
using TMPro; // Gunakan ini jika pakai TextMeshPro
using UnityEngine.UI; // Gunakan ini jika pakai Text UI standar

public class BlinkText : MonoBehaviour
{
    [Tooltip("Kecepatan kedip dalam detik")]
    public float interval = 0.5f; 

    private GameObject visualText;

    void Start()
    {
        // Menyimpan referensi ke objek ini sendiri
        visualText = this.gameObject;

        // Menjalankan fungsi ToggleText secara berulang
        // 0 = mulai sekarang, interval = diulang setiap X detik
        InvokeRepeating("ToggleText", 0, interval);
    }

    void ToggleText()
    {
        // Membalikkan status aktif objek (Jika aktif jadi mati, jika mati jadi aktif)
        visualText.SetActive(!visualText.activeSelf);
    }
}