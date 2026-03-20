using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BiasedRunnerCamera : MonoBehaviour
{
    [Header("Target & Base Follow")]
    public Transform player; 
    public Vector2 baseOffset = new Vector2(3f, 1f); 
    
    [Header("Follow Smoothing")]
    public float xSmoothTime = 0.15f; 
    public float ySmoothTime = 0.1f; 
    
    [Header("Pengaturan Zoom (Berdasarkan Arah)")]
    public float minZoomSize = 5f; 
    public float maxZoomSize = 8f; 
    
    [Tooltip("Besarkan angka ini (misal 1.0 - 2.0) agar zoom-out terasa bertahap seiring player lari")]
    public float zoomSmoothTime = 1.2f; 

    [Tooltip("Seberapa jauh area didorong ke kanan saat zoom out (0 = tengah, 1 = sangat ke kanan)")]
    [Range(0f, 1f)]
    public float rightwardBiasFactor = 0.7f;

    private Camera cam;
    private Rigidbody2D playerRb; // Menggunakan fisika player untuk deteksi arah
    
    private float zoomVelocity;
    private float currentXVelocity;
    private float currentYVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = minZoomSize;

        if (player != null)
        {
            // Ambil komponen Rigidbody2D dari player
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void LateUpdate()
    {
        // Pastikan player dan Rigidbody-nya terbaca
        if (player == null || playerRb == null) return;

        // --- 1. DETEKSI ARAH & TARGET ZOOM ---
        float speedX = playerRb.linearVelocity.x;
        float targetZoom = minZoomSize;
        float targetBiasX = 0f;

        // Jika lari ke KANAN (kecepatan positif)
        if (speedX > 0.1f)
        {
            targetZoom = maxZoomSize;
            
            // Hitung dorongan ke kiri agar area kanan terlihat mekar (bias)
            float zoomExpansion = cam.orthographicSize - minZoomSize;
            targetBiasX = -(zoomExpansion * rightwardBiasFactor * cam.aspect);
        }
        // Jika lari ke KIRI atau diam (kecepatan negatif/nol)
        else if (speedX <= 0.1f)
        {
            targetZoom = minZoomSize; // Targetkan kembali mengecil
            targetBiasX = 0f; // Hilangkan dorongan bias
        }

        // --- 2. TERAPKAN ZOOM DENGAN MULUS ---
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);


        // --- 3. TERAPKAN POSISI KAMERA (X & Y) ---
        // Titik kumpul ideal: posisi player + offset dasar
        Vector2 targetPos2D = new Vector2(player.position.x + baseOffset.x, player.position.y + baseOffset.y);
        
        // Masukkan modifikasi bias ke posisi X
        float finalTargetX = targetPos2D.x + targetBiasX;

        // Lakukan smoothing pergerakan
        float smoothedX = Mathf.SmoothDamp(transform.position.x, finalTargetX, ref currentXVelocity, xSmoothTime);
        float smoothedY = Mathf.SmoothDamp(transform.position.y, targetPos2D.y, ref currentYVelocity, ySmoothTime);

        transform.position = new Vector3(smoothedX, smoothedY, transform.position.z);
    }
}