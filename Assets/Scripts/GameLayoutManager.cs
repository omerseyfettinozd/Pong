using UnityEngine;

/// <summary>
/// Dynamically positions walls, paddles, and score zones based on camera bounds.
/// Attach to any GameObject in the scene (e.g., GameManager).
/// Tüm oyun elemanlarını kamera sınırlarına göre dinamik olarak konumlandırır.
/// </summary>
public class GameLayoutManager : MonoBehaviour
{
    [Header("Walls / Duvarlar")]
    [Tooltip("Left wall / Sol duvar")]
    [SerializeField] private Transform wallLeft;

    [Tooltip("Right wall / Sağ duvar")]
    [SerializeField] private Transform wallRight;

    [Header("Paddles / Raketler")]
    [Tooltip("Top paddle / Üst raket")]
    [SerializeField] private Transform paddleTop;

    [Tooltip("Bottom paddle / Alt raket")]
    [SerializeField] private Transform paddleBottom;

    [Header("Score Zones / Gol Bölgeleri")]
    [Tooltip("Top score zone / Üst gol bölgesi")]
    [SerializeField] private Transform scoreZoneTop;

    [Tooltip("Bottom score zone / Alt gol bölgesi")]
    [SerializeField] private Transform scoreZoneBottom;

    [Header("Layout Settings / Yerleşim Ayarları")]
    [Tooltip("Paddle distance from screen edge / Raketin ekran kenarından uzaklığı")]
    [SerializeField] private float paddleEdgeOffset = 0.5f;

    [Tooltip("Score zone distance beyond paddles / Gol bölgesinin raketlerin ötesindeki mesafesi")]
    [SerializeField] private float scoreZoneOffset = 1.0f;

    [Tooltip("Wall thickness (scale X) / Duvar kalınlığı (scale X)")]
    [SerializeField] private float wallThickness = 1f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Wait one frame for CameraResizer to finish adjusting
        // CameraResizer'ın ayarlamasını bitirmesi için bir frame bekle
        Invoke(nameof(PositionAllElements), 0.01f);
    }

    // Positions all game elements based on camera bounds
    // Tüm oyun elemanlarını kamera sınırlarına göre konumlandırır
    private void PositionAllElements()
    {
        if (mainCamera == null) return;

        float camHalfHeight = mainCamera.orthographicSize;
        float camHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;

        // --- POSITION WALLS / DUVARLARI KONUMLANDIR ---
        if (wallLeft != null)
        {
            // Place at left edge / Sol kenara yerleştir
            wallLeft.position = new Vector3(-camHalfWidth - wallThickness / 2f, 0, 0);
            // Scale wall height to cover full screen / Duvar yüksekliğini ekranı kaplayacak şekilde ölçekle
            wallLeft.localScale = new Vector3(wallThickness, camHalfHeight * 2f + 2f, 1f);
        }

        if (wallRight != null)
        {
            // Place at right edge / Sağ kenara yerleştir
            wallRight.position = new Vector3(camHalfWidth + wallThickness / 2f, 0, 0);
            wallRight.localScale = new Vector3(wallThickness, camHalfHeight * 2f + 2f, 1f);
        }

        // --- POSITION PADDLES / RAKETLERİ KONUMLANDIR ---
        if (paddleTop != null)
        {
            Vector3 pos = paddleTop.position;
            pos.y = camHalfHeight - paddleEdgeOffset;
            paddleTop.position = pos;
        }

        if (paddleBottom != null)
        {
            Vector3 pos = paddleBottom.position;
            pos.y = -camHalfHeight + paddleEdgeOffset;
            paddleBottom.position = pos;
        }

        // --- POSITION SCORE ZONES / GOL BÖLGELERİNİ KONUMLANDIR ---
        if (scoreZoneTop != null)
        {
            Vector3 pos = scoreZoneTop.position;
            pos.y = camHalfHeight + scoreZoneOffset;
            scoreZoneTop.position = pos;
            // Stretch score zone width to match screen / Gol bölgesi genişliğini ekranla eşle
            scoreZoneTop.localScale = new Vector3(camHalfWidth * 2f + 2f, scoreZoneTop.localScale.y, 1f);
        }

        if (scoreZoneBottom != null)
        {
            Vector3 pos = scoreZoneBottom.position;
            pos.y = -camHalfHeight - scoreZoneOffset;
            scoreZoneBottom.position = pos;
            scoreZoneBottom.localScale = new Vector3(camHalfWidth * 2f + 2f, scoreZoneBottom.localScale.y, 1f);
        }

        Debug.Log($"[GameLayoutManager] Layout positioned for camera bounds: {camHalfWidth * 2f}x{camHalfHeight * 2f}");
    }
}
