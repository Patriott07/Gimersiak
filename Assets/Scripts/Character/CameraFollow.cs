using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;          // Objek yang diikuti (misal bola atau player)
    
    [Header("Settings")]
    public float smoothSpeed = 5f;    // Semakin besar -> semakin cepat nyusul
    public Vector3 offset;            // Jarak dari target

    [Header("Shake Effect")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.2f;

    public static CameraFollow Instance;

    void Start()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Target posisi + offset
        Vector3 desiredPosition = target.position + offset;

        // Smooth damp pakai Lerp
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}
