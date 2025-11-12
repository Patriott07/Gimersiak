using UnityEngine;
using UnityEngine.EventSystems; // Wajib untuk IPointer...Handler
using TMPro; // Wajib untuk TextMeshPro

public class TextHoverCombinedEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pengaturan Warna")]
    public Color hoverColor = Color.yellow; // Warna target saat di-hover

    [Header("Pengaturan Skala")]
    public float hoverScaleFactor = 1.1f; // Target pembesaran (1.1 = 110%)

    [Header("Pengaturan Animasi")]
    public float animationSpeed = 10f; // Kecepatan transisi (scale dan warna)

    // --- Variabel Internal ---
    private TextMeshProUGUI textMesh;

    // Nilai Asli
    private Color originalColor;
    private Vector3 originalScale;

    // Nilai Target (untuk animasi)
    private Color targetColor;
    private Vector3 targetScale;

    void Awake()
    {
        // 1. Ambil komponen TextMeshPro
        textMesh = GetComponent<TextMeshProUGUI>();

        // 2. Simpan nilai-nilai aslinya
        if (textMesh != null)
        {
            originalColor = textMesh.color;
        }
        originalScale = transform.localScale;

        // 3. Set target awal (agar tidak ada animasi saat game baru mulai)
        targetColor = originalColor;
        targetScale = originalScale;
    }

    void Update()
    {
        // Fungsi ini akan berjalan setiap frame
        // Kita akan gerakkan nilai saat ini (current) menuju nilai target (target)

        float t = Time.deltaTime * animationSpeed;

        // Animasikan skala secara halus (Lerp)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);

        // Animasikan warna secara halus (Lerp)
        if (textMesh != null)
        {
            textMesh.color = Color.Lerp(textMesh.color, targetColor, t);
        }
    }

    // Fungsi ini dipanggil saat mouse MASUK area teks
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HomeSceneSc.Instance != null)
            HomeSceneSc.Instance.PlayAudio();

        if (ScoreSceneSc.Instance != null)
            ScoreSceneSc.Instance.PlayAudio();
        // Set nilai TARGET ke nilai hover
        targetScale = originalScale * hoverScaleFactor;
        targetColor = hoverColor;
    }

    // Fungsi ini dipanggil saat mouse KELUAR area teks
    public void OnPointerExit(PointerEventData eventData)
    {
        // Set nilai TARGET kembali ke nilai asli
        targetScale = originalScale;
        targetColor = originalColor;
    }
}