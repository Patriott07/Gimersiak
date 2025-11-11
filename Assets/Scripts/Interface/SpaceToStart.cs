using UnityEngine;
using UnityEngine.SceneManagement; // Untuk pindah scene
using TMPro; // Untuk TextMeshPro

public class SpaceToStart : MonoBehaviour
{
    [Header("Pengaturan Scene")]
    public string sceneToLoad = "GameScene"; // Nama scene yang akan dimuat
    
    [Header("Pengaturan Efek Denyut (Pulsing)")]
    public float pulseMinScale = 0.9f;  // Skala minimum (mengecil)
    public float pulseMaxScale = 1.1f;  // Skala maksimum (membesar)
    public float pulseSpeed = 2f;       // Kecepatan denyut

    [Header("Pengaturan Warna (Opsional)")]
    public bool enableColorPulse = true;    // Aktifkan efek warna
    public Color pulseColorMin = Color.white;   // Warna minimum
    public Color pulseColorMax = Color.yellow;  // Warna maksimum

    // --- Variabel Internal ---
    private TextMeshProUGUI textMesh;
    private Vector3 originalScale;
    private bool isWaitingForInput = true;

    void Awake()
    {
        // Ambil komponen TextMeshPro
        textMesh = GetComponent<TextMeshProUGUI>();
        
        // Simpan skala asli
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isWaitingForInput)
        {
            // --- EFEK DENYUT (PULSING) ---
            // Menggunakan Mathf.Sin untuk membuat gerakan naik-turun smooth
            // Mathf.PingPong juga bisa dipakai, tapi Sin lebih smooth
            
            // Hitung nilai oscillation antara 0 dan 1
            float oscillation = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            
            // 1. Animasi Skala
            float currentScale = Mathf.Lerp(pulseMinScale, pulseMaxScale, oscillation);
            transform.localScale = originalScale * currentScale;
            
            // 2. Animasi Warna (jika diaktifkan)
            if (enableColorPulse && textMesh != null)
            {
                textMesh.color = Color.Lerp(pulseColorMin, pulseColorMax, oscillation);
            }

            // --- CEK INPUT SPACE ---
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
        }
    }

    void StartGame()
    {
        isWaitingForInput = false;
        
        // Reset skala dan warna ke normal sebelum pindah scene (opsional)
        transform.localScale = originalScale;
        if (textMesh != null)
        {
            textMesh.color = pulseColorMin;
        }
        
        // Load scene yang ditentukan
        SceneManager.LoadScene(sceneToLoad);
    }
}
