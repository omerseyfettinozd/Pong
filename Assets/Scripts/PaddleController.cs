using UnityEngine;
using UnityEngine.InputSystem;

// AI Difficulty Levels / Yapay Zeka Zorluk Seviyeleri
public enum AIDifficulty
{
    Easy,   // Kolay
    Normal, // Orta
    Hard    // Zor
}

public class PaddleController : MonoBehaviour
{
    [Header("Settings / Ayarlar")]
    [Tooltip("Is this paddle controlled by AI? / Bu raket yapay zeka tarafından mı kontrol ediliyor?")]
    [SerializeField] private bool isAI = false;

    [Tooltip("Which half of the screen controls this paddle? True=Top, False=Bottom / Ekranın hangi yarısı bu raketi kontrol eder? True=Üst, False=Alt")]
    [SerializeField] private bool isTopSide = false;

    [Tooltip("Smoothing for movement / Hareketin yumuşaklığı")]
    [SerializeField] private float smoothing = 15f;

    [Header("AI Settings / Yapay Zeka Ayarları")]
    [Tooltip("AI Difficulty / Yapay Zeka Zorluk Seviyesi")]
    [SerializeField] private AIDifficulty difficulty = AIDifficulty.Normal;

    private Transform ball;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private float targetX;
    private float aiSmoothing;

    // AI difficulty parameters / Yapay zeka zorluk parametreleri
    private float aiErrorRange;       // How much AI misses the ball / YZ'nın topu kaçırma miktarı
    private float aiReactionDelay;    // Delay before AI reacts / YZ'nın tepki gecikmesi

    private float reactionTimer;
    private float currentError;       // Cached error value / Saklanan hata değeri
    private float errorTimer;         // Timer for updating error / Hata güncelleme zamanlayıcısı
    private const float ERROR_UPDATE_INTERVAL = 0.5f; // Update error every 0.5s / Hatayı 0.5 saniyede bir güncelle

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        ApplyDifficulty();
    }

    // Sets AI parameters based on difficulty / Zorluğa göre YZ parametrelerini ayarlar
    private void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:
                aiSmoothing = 3f;       // Slow reaction / Yavaş tepki
                aiErrorRange = 1.5f;    // Misses more / Daha çok kaçırır
                aiReactionDelay = 0.4f; // Delayed reaction / Gecikmeli tepki
                break;

            case AIDifficulty.Normal:
                aiSmoothing = 6f;       // Medium reaction / Orta tepki
                aiErrorRange = 0.5f;    // Sometimes misses / Bazen kaçırır
                aiReactionDelay = 0.15f; // Slight delay / Hafif gecikme
                break;

            case AIDifficulty.Hard:
                aiSmoothing = 12f;      // Fast reaction / Hızlı tepki
                aiErrorRange = 0.1f;    // Rarely misses / Nadiren kaçırır
                aiReactionDelay = 0f;   // No delay / Gecikme yok
                break;
        }
    }

    private void Start()
    {
        targetX = transform.position.x;

        // Find the ball object if AI is enabled
        // Eğer yapay zeka aktifse top nesnesini bul
        if (isAI)
        {
            GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
            if (ballObj != null)
            {
                ball = ballObj.transform;
            }
        }
    }

    private void Update()
    {
        // Only handle input in Update / Sadece girdi Update'te işlenir
        if (!isAI)
        {
            HandleTouchAndMouse();
        }
    }

    private void FixedUpdate()
    {
        if (isAI)
        {
            MoveAI();
        }
        else
        {
            MovePlayer();
        }
    }

    // Smoothly moves player paddle / Oyuncu raketini yumuşak hareket ettirir
    // Walls stop the paddle via colliders / Duvarlar raketi collider ile durdurur
    private void MovePlayer()
    {
        float newX = Mathf.Lerp(transform.position.x, targetX, smoothing * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(newX, transform.position.y));
    }

    // Handles all player input: Keyboard (PC), Touch (Mobile), Mouse (PC)
    // Tüm oyuncu girişlerini yönetir: Klavye (PC), Dokunmatik (Mobil), Fare (PC)
    private void HandleTouchAndMouse()
    {
        // --- KEYBOARD INPUT (PC 2-Player) / KLAVYE GİRİŞİ (PC 2 Kişilik) ---
        // Bottom player: A/D keys | Top player: Left/Right arrow keys
        // Alt oyuncu: A/D tuşları | Üst oyuncu: Sol/Sağ ok tuşları
        if (Keyboard.current != null)
        {
            float keyboardDir = 0f;

            if (!isTopSide)
            {
                // Player 1 (Bottom): A/D keys / Oyuncu 1 (Alt): A/D tuşları
                if (Keyboard.current.aKey.isPressed) keyboardDir = -1f;
                else if (Keyboard.current.dKey.isPressed) keyboardDir = 1f;
            }
            else
            {
                // Player 2 (Top): Arrow keys / Oyuncu 2 (Üst): Ok tuşları
                if (Keyboard.current.leftArrowKey.isPressed) keyboardDir = -1f;
                else if (Keyboard.current.rightArrowKey.isPressed) keyboardDir = 1f;
            }

            if (keyboardDir != 0f)
            {
                targetX = transform.position.x + keyboardDir * smoothing * Time.deltaTime;
                return; // Keyboard active, skip other input / Klavye aktif, diğer girişleri atla
            }
        }

        // --- MULTI-TOUCH INPUT (Mobile) / ÇOKLU DOKUNMATİK GİRİŞ (Mobil) ---
        // Supports 2 players on same device / Aynı cihazda 2 oyuncuyu destekler
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                {
                    Vector2 touchPos = touch.position.ReadValue();
                    ProcessScreenInput(touchPos);
                }
            }
            // If any touch was active, skip mouse / Dokunmatik varsa fareyi atla
            if (Touchscreen.current.primaryTouch.press.isPressed) return;
        }

        // --- MOUSE INPUT (PC single player) / FARE GİRİŞİ (PC tek oyuncu) ---
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                ProcessScreenInput(mousePos);
            }
        }
    }

    // Converts screen position to world target X
    // Ekran pozisyonunu dünya hedef X'ine dönüştürür
    private void ProcessScreenInput(Vector2 screenPos)
    {
        // Determine which half of the screen was touched (top or bottom)
        // Ekranın hangi yarısına dokunuldu (üst veya alt)
        bool touchedTopHalf = screenPos.y > Screen.height / 2f;

        // Only respond if touch is on our half / Sadece kendi yarımızdaki dokunuşa tepki ver
        if (touchedTopHalf == isTopSide)
        {
            // Convert screen position to world position / Ekran pozisyonunu dünya pozisyonuna çevir
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            targetX = worldPos.x;
        }
    }

    // Handles AI Movement with difficulty-based behavior
    // Zorluk seviyesine göre yapay zeka hareketi
    private void MoveAI()
    {
        if (ball == null) return;

        // Reaction delay: AI waits before responding / Tepki gecikmesi: YZ cevap vermeden önce bekler
        reactionTimer += Time.fixedDeltaTime;
        if (reactionTimer < aiReactionDelay)
        {
            return; // AI is "thinking" / YZ "düşünüyor"
        }

        // Update error value periodically, NOT every frame / Hata değerini her frame değil, belirli aralıklarla güncelle
        errorTimer += Time.fixedDeltaTime;
        if (errorTimer >= ERROR_UPDATE_INTERVAL)
        {
            currentError = Random.Range(-aiErrorRange, aiErrorRange);
            errorTimer = 0f;
        }

        float aiTargetX = ball.position.x + currentError;

        // Smoothly follow the ball / Topu yumuşak şekilde takip et
        float newX = Mathf.Lerp(transform.position.x, aiTargetX, aiSmoothing * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(newX, transform.position.y));
    }

    // Called externally to change difficulty at runtime / Çalışma zamanında zorluğu değiştirmek için dışarıdan çağrılır
    public void SetDifficulty(AIDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
        ApplyDifficulty();
        reactionTimer = 0f;
    }

    // Called externally to enable/disable AI mode / Yapay zeka modunu açıp kapatmak için dışarıdan çağrılır
    public void SetAIEnabled(bool enabled)
    {
        isAI = enabled;

        // If AI just enabled, find the ball / YZ yeni açıldıysa topu bul
        if (isAI && ball == null)
        {
            GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
            if (ballObj != null)
            {
                ball = ballObj.transform;
            }
        }
    }
}
