using UnityEngine;

public class RocketAnimation : MonoBehaviour
{
    [Header("Pengaturan Guncangan (Shake)")]
    public float shakeIntensity = 0.2f;     // Intensitas guncangan (dikurangi untuk lebih santai)
    public float shakeSpeed = 8f;           // Kecepatan guncangan (dikurangi untuk lebih santai)
    [Tooltip("Faktor pengurangan getaran pada sumbu X (0-1). 0 = tidak bergerak horizontal, 1 = full.")]
    public float horizontalShakeFactor = 0.4f; // Kurangi getaran di X
    
    [Header("Pengaturan Rotasi")]
    public float rotationAmount = 1.5f;     // Sudut rotasi maksimal (dikurangi untuk lebih santai)
    public float rotationSpeed = 5f;        // Kecepatan rotasi (dikurangi untuk lebih santai)
    
    [Header("Pengaturan Gerakan Vertikal (Naik-Turun)")]
    public float verticalAmplitude = 1f;    // Seberapa tinggi naik-turun (dikurangi untuk lebih santai)
    public float verticalSpeed = 2f;        // Kecepatan naik-turun (dikurangi untuk lebih santai)
    
    [Header("Opsi Tambahan")]
    public bool enableShake = true;         // Aktifkan guncangan
    public bool enableRotation = true;      // Aktifkan rotasi
    public bool enableVerticalMove = true;  // Aktifkan gerakan vertikal
    [Tooltip("Skala tambahan untuk rotasi Z (0-1). Gunakan nilai kecil untuk mengurangi efek rotasi yang membuat kamera bergetar")]
    public float rotationFactor = 0.5f;     // Kurangi efek rotasi
    [Header("Rotasi ke Child (opsional)")]
    [Tooltip("Jika di-set, rotasi akan diterapkan pada transform visual ini, bukan pada root. Membantu mencegah pengaruh rotasi terhadap posisi/physics.")]
    public Transform visualTransform;       // Jika ingin hanya merotate visual child
    
    [Header("Pengaturan Credit Scene (30 detik)")]
    public float creditDuration = 30f;      // Durasi total credit scene
    public float constantIntensity = 0.3f;  // Intensitas tetap sepanjang scene (30% - getaran sedikit)
    public float stopAnimationTime = 25f;   // Waktu untuk berhenti (detik ke-25)
    
    // --- Variabel Internal ---
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Quaternion originalVisualRotation;
    
    // Untuk randomisasi guncangan
    private float shakeOffsetX;
    private float shakeOffsetY;
    
    // Untuk animasi progresif
    private float elapsedTime = 0f;
    private float currentIntensityMultiplier = 0.3f; // Tetap di 30%

    void Start()
    {
        // Simpan posisi dan rotasi asli
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        if (visualTransform != null)
        {
            originalVisualRotation = visualTransform.localRotation;
        }
        
        // Buat offset random untuk variasi guncangan
        shakeOffsetX = Random.Range(0f, 100f);
        shakeOffsetY = Random.Range(0f, 100f);
        
        // Set intensitas tetap dari awal
        currentIntensityMultiplier = constantIntensity;
        elapsedTime = 0f;
    }

    void Update()
    {
        // --- UPDATE INTENSITAS BERDASARKAN WAKTU ---
        elapsedTime += Time.deltaTime;
        
        // Cek apakah sudah mencapai waktu berhenti (detik ke-25)
        if (elapsedTime >= stopAnimationTime)
        {
            // Fade out animasi secara smooth menuju 0
            float fadeOutDuration = 2f; // 2 detik untuk fade out
            float fadeOutProgress = Mathf.Clamp01((elapsedTime - stopAnimationTime) / fadeOutDuration);
            currentIntensityMultiplier = Mathf.Lerp(constantIntensity, 0f, fadeOutProgress);
            
            // Jika sudah benar-benar 0, hentikan animasi
            if (fadeOutProgress >= 1f)
            {
                currentIntensityMultiplier = 0f;
                // Kembalikan ke posisi original secara smooth
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5f);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, Time.deltaTime * 5f);
                return; // Skip animasi
            }
        }
        else
        {
            // Intensitas TETAP sepanjang scene (tidak meningkat)
            currentIntensityMultiplier = constantIntensity;
        }
        
        // Reset ke posisi dan rotasi dasar
        Vector3 newPosition = originalPosition;
        Vector3 newRotation = originalRotation.eulerAngles;
        
        // --- 1. GUNCANGAN (SHAKE) ---
        if (enableShake)
        {
            // Gunakan Perlin Noise untuk guncangan yang lebih natural
            float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed + shakeOffsetX, 0f) - 0.5f) * 2f * shakeIntensity * currentIntensityMultiplier * horizontalShakeFactor;
            float shakeY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed + shakeOffsetY) - 0.5f) * 2f * shakeIntensity * currentIntensityMultiplier;
            
            newPosition.x += shakeX;
            newPosition.y += shakeY;
        }
        
        // --- 2. ROTASI KIRI-KANAN ---
        if (enableRotation)
        {
            // Gunakan Sin untuk rotasi smooth bolak-balik
            // Terapkan rotationFactor untuk mengurangi efek rotasi yang menyebabkan kamera "bergetar"
            float rotation = Mathf.Sin(Time.time * rotationSpeed) * rotationAmount * currentIntensityMultiplier * rotationFactor;

            if (visualTransform != null)
            {
                // Terapkan rotasi hanya pada visual child agar root (yang di-follow camera) tidak berotasi
                visualTransform.localRotation = Quaternion.Euler(originalVisualRotation.eulerAngles + new Vector3(0f, 0f, rotation));
            }
            else
            {
                newRotation.z += rotation;
            }
        }
        
        // --- 3. GERAKAN VERTIKAL (NAIK-TURUN) ---
        if (enableVerticalMove)
        {
            // Gerakan naik-turun menggunakan Sin
            float verticalOffset = Mathf.Sin(Time.time * verticalSpeed) * verticalAmplitude * currentIntensityMultiplier;
            newPosition.y += verticalOffset;
        }
        
        // Terapkan transformasi
        transform.localPosition = newPosition;
        // Jika rotasi diterapkan pada visualTransform, tetap kembalikan root ke rotasi asli
        if (visualTransform != null)
        {
            transform.localRotation = originalRotation;
        }
        else
        {
            transform.localRotation = Quaternion.Euler(newRotation);
        }
    }
    
    // --- FUNGSI HELPER (Opsional) ---
    
    /// <summary>
    /// Mulai animasi roket
    /// </summary>
    public void StartAnimation()
    {
        enabled = true;
        elapsedTime = 0f;
        currentIntensityMultiplier = constantIntensity;
    }
    
    /// <summary>
    /// Hentikan animasi roket
    /// </summary>
    public void StopAnimation()
    {
        enabled = false;
        // Reset ke posisi awal
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
    
    /// <summary>
    /// Set intensitas guncangan secara dinamis
    /// </summary>
    public void SetShakeIntensity(float intensity)
    {
        shakeIntensity = intensity;
    }
    
    /// <summary>
    /// Set kecepatan animasi secara keseluruhan
    /// </summary>
    public void SetAnimationSpeed(float speedMultiplier)
    {
        shakeSpeed *= speedMultiplier;
        rotationSpeed *= speedMultiplier;
        verticalSpeed *= speedMultiplier;
    }
    
    /// <summary>
    /// Dapatkan progress animasi (0-1)
    /// </summary>
    public float GetAnimationProgress()
    {
        return Mathf.Clamp01(elapsedTime / creditDuration);
    }
    
    /// <summary>
    /// Dapatkan intensitas saat ini (0-1)
    /// </summary>
    public float GetCurrentIntensity()
    {
        return currentIntensityMultiplier;
    }
}
