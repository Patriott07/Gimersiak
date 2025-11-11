using UnityEngine;
using UnityEngine.EventSystems; // Wajib untuk IPointer...Handler

public class HoverFloatEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pengaturan Skala")]
    public float hoverScaleFactor = 1.1f; // Target pembesaran (1.1 = 110%)
    public float scaleSpeed = 8f;         // Kecepatan transisi skala

    [Header("Pengaturan Mengambang (Floating)")]
    public float floatAmplitude = 5f;  // Seberapa JAUH naik-turunnya (dalam pixel)
    public float floatSpeed = 4f;      // Seberapa CEPAT naik-turunnya
    
    // --- Variabel Internal ---
    
    // Untuk Skala
    private Vector3 originalScale;
    private Vector3 targetScale;

    // Untuk Posisi (Floating)
    private RectTransform rectTransform;
    private Vector2 originalAnchorPos;
    
    // Status
    private bool isHovering = false;

    void Awake()
    {
        // 1. Ambil komponen RectTransform
        rectTransform = GetComponent<RectTransform>();

        // 2. Simpan nilai-nilai asli
        originalScale = rectTransform.localScale;
        originalAnchorPos = rectTransform.anchoredPosition;

        // 3. Set target awal
        targetScale = originalScale;
    }

    // Fungsi ini dipanggil saat mouse MASUK
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScaleFactor; // Set target skala membesar
    }    // Fungsi ini dipanggil saat mouse KELUAR
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale; // Set target skala kembali normal
    }

    void Update()
    {
        // --- 1. PROSES SKALA ---
        // Animasikan skala secara halus (Lerp) menuju targetScale
        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            targetScale, 
            Time.deltaTime * scaleSpeed
        );

        // --- 2. PROSES FLOATING (POSISI) ---
        if (isHovering)
        {
            // Jika sedang di-hover:
            // Hitung offset Y menggunakan gelombang Sin (Mathf.Sin)
            // Time.time membuat gelombang terus bergerak
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            
            // Terapkan offset ke posisi Y aslinya
            Vector2 targetPos = originalAnchorPos + new Vector2(0, yOffset);
            
            // Set posisi (kita set langsung agar float-nya presisi, tidak 'lagging')
            rectTransform.anchoredPosition = targetPos;
        }
        else
        {
            // Jika tidak di-hover:
            // Kembalikan posisi ke posisi aslinya secara halus (Lerp)
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition, 
                originalAnchorPos, 
                Time.deltaTime * scaleSpeed // Bisa pakai speed yg sama dgn scale
            );
        }
    }
}