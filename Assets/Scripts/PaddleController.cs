using UnityEngine;
using UnityEngine.InputSystem;

// AI Difficulty Levels / Yapay Zeka Zorluk Seviyeleri
public enum AIDifficulty
{
    Easy,   // Kolay
    Normal, // Orta
    Hard,   // Zor
    Expert  // Uzman
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
    private Rigidbody2D ballRb;       // Ball's Rigidbody for velocity / Topun Rigidbody'si hız için
    private Camera mainCamera;
    private float targetX;
    private float aiSmoothing;

    // AI difficulty parameters / Yapay zeka zorluk parametreleri
    private float aiErrorRange;       // How much AI misses the ball / YZ'nın topu kaçırma miktarı
    private float aiReactionDelay;    // Delay before AI reacts / YZ'nın tepki gecikmesi
    private float aiReturnSpeed;      // Speed of returning to center / Merkeze dönüş hızı

    private float reactionTimer;
    private float currentError;       // Cached error value / Saklanan hata değeri
    private float errorTimer;         // Timer for updating error / Hata güncelleme zamanlayıcısı
    private const float ERROR_UPDATE_INTERVAL = 0.5f; // Update error every 0.5s / Hatayı 0.5 saniyede bir güncelle
    private const float DEAD_ZONE = 0.08f; // Prevents micro-jitter / Mikro titreşimi önler

    // Screen bounds for clamping / Ekran sınırları için
    private float minX;
    private float maxX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        ApplyDifficulty();
    }

    // Calculates screen bounds for paddle movement / Raket hareketi için ekran sınırlarını hesaplar
    private void CalculateBounds()
    {
        if (mainCamera == null) return;
        float camHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float paddleHalfWidth = 0.8f; // Approximate paddle half-width / Yaklaşık raket yarı genişliği
        minX = -camHalfWidth + paddleHalfWidth;
        maxX = camHalfWidth - paddleHalfWidth;
    }

    // Sets AI parameters based on difficulty / Zorluğa göre YZ parametrelerini ayarlar
    private void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:
                aiSmoothing = 3f;       // Slow tracking / Yavaş takip
                aiErrorRange = 1.5f;    // Misses more / Daha çok kaçırır
                aiReactionDelay = 0.4f; // Delayed reaction / Gecikmeli tepki
                aiReturnSpeed = 1f;     // Slow return / Yavaş dönüş
                break;

            case AIDifficulty.Normal:
                aiSmoothing = 6f;       // Medium tracking / Orta takip
                aiErrorRange = 0.5f;    // Sometimes misses / Bazen kaçırır
                aiReactionDelay = 0.15f; // Slight delay / Hafif gecikme
                aiReturnSpeed = 2f;     // Medium return / Orta dönüş
                break;

            case AIDifficulty.Hard:
                aiSmoothing = 12f;      // Fast tracking / Hızlı takip
                aiErrorRange = 0.1f;    // Rarely misses / Nadiren kaçırır
                aiReactionDelay = 0f;   // No delay / Gecikme yok
                aiReturnSpeed = 3f;     // Fast return / Hızlı dönüş
                break;

            case AIDifficulty.Expert:
                aiSmoothing = 18f;      // Very fast tracking / Çok hızlı takip
                aiErrorRange = 0.02f;   // Almost never misses / Neredeyse hiç kaçırmaz
                aiReactionDelay = 0f;   // No delay / Gecikme yok
                aiReturnSpeed = 4f;     // Very fast return / Çok hızlı dönüş
                break;
        }
    }

    private void Start()
    {
        targetX = transform.position.x;
        CalculateBounds(); // After CameraResizer / CameraResizer'dan sonra

        // Find the ball object if AI is enabled
        // Eğer yapay zeka aktifse top nesnesini bul
        if (isAI)
        {
            FindBall();
        }
    }

    // Finds and caches the ball references / Top referanslarını bulur ve saklar
    private void FindBall()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
        if (ballObj != null)
        {
            ball = ballObj.transform;
            ballRb = ballObj.GetComponent<Rigidbody2D>();
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
    private void MovePlayer()
    {
        float newX = Mathf.Lerp(transform.position.x, targetX, smoothing * Time.fixedDeltaTime);
        newX = Mathf.Clamp(newX, minX, maxX); // Clamp to screen bounds / Ekran sınırlarına sıkıştır
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

    // --- PREDICTIVE AI SYSTEM / TAHMİNE DAYALI YAPAY ZEKA SİSTEMİ ---

    // Main AI movement logic / Ana YZ hareket mantığı
    private void MoveAI()
    {
        if (ball == null || ballRb == null) return;

        Vector2 ballVel = ballRb.linearVelocity;

        // Is the ball coming toward this paddle? / Top bu rakete doğru mu geliyor?
        bool ballComingToward = (isTopSide && ballVel.y > 0) || (!isTopSide && ballVel.y < 0);

        float aiTargetX;
        float effectiveSmoothing;

        if (ballComingToward && ballVel.sqrMagnitude > 0.1f)
        {
            // --- BALL APPROACHING: Predict and intercept / TOP YAKLAŞIYOR: Tahmin et ve karşıla ---

            // Reaction delay: AI waits before responding / Tepki gecikmesi
            reactionTimer += Time.fixedDeltaTime;
            if (reactionTimer < aiReactionDelay) return;

            // Update error value periodically / Hata değerini periyodik güncelle
            errorTimer += Time.fixedDeltaTime;
            if (errorTimer >= ERROR_UPDATE_INTERVAL)
            {
                currentError = Random.Range(-aiErrorRange, aiErrorRange);
                errorTimer = 0f;
            }

            // Predict where ball will land + add error / Topun düşeceği yeri tahmin et + hata ekle
            aiTargetX = PredictBallLandingX(ball.position, ballVel) + currentError;
            effectiveSmoothing = aiSmoothing;
        }
        else
        {
            // --- BALL GOING AWAY: Slowly return to center / TOP UZAKLAŞIYOR: Yavaşça merkeze dön ---
            reactionTimer = 0f; // Reset for next approach / Sonraki yaklaşım için sıfırla
            aiTargetX = 0f;     // Center of field / Alanın ortası
            effectiveSmoothing = aiReturnSpeed;
        }

        // Dead zone: don't move if already close enough / Ölü bölge: yeterince yakınsa hareket etme
        if (Mathf.Abs(transform.position.x - aiTargetX) < DEAD_ZONE) return;

        // Smooth movement toward target / Hedefe doğru yumuşak hareket
        float newX = Mathf.Lerp(transform.position.x, aiTargetX, effectiveSmoothing * Time.fixedDeltaTime);
        newX = Mathf.Clamp(newX, minX, maxX); // Clamp to screen bounds / Ekran sınırlarına sıkıştır
        rb.MovePosition(new Vector2(newX, transform.position.y));
    }

    // Predicts where the ball will reach this paddle's Y level, accounting for wall bounces
    // Topun bu raketin Y seviyesine ulaşacağı yeri tahmin eder, duvar sekmelerini hesaba katar
    private float PredictBallLandingX(Vector2 ballPos, Vector2 ballVel)
    {
        float paddleY = transform.position.y;

        // Safety check / Güvenlik kontrolü
        if (Mathf.Abs(ballVel.y) < 0.01f) return ballPos.x;

        // Time for ball to reach paddle's Y / Topun raket Y'sine ulaşma süresi
        float timeToReach = (paddleY - ballPos.y) / ballVel.y;
        if (timeToReach < 0) return ballPos.x;

        // Raw predicted X without wall bounces / Duvar sekmeleri olmadan ham tahmin X
        float predictedX = ballPos.x + ballVel.x * timeToReach;

        // Simulate wall bounces using PingPong / PingPong ile duvar sekmelerini simüle et
        float camHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float wallMargin = 0.5f; // Approximate wall+ball offset / Yaklaşık duvar+top payı
        float leftBound = -camHalfWidth + wallMargin;
        float rightBound = camHalfWidth - wallMargin;
        float fieldWidth = rightBound - leftBound;

        if (fieldWidth <= 0) return predictedX;

        // PingPong simulates the ball bouncing back and forth between walls
        // PingPong, topun duvarlar arasında sekme hareketini simüle eder
        float normalizedX = predictedX - leftBound;
        normalizedX = Mathf.PingPong(normalizedX, fieldWidth);

        return normalizedX + leftBound;
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
        if (isAI && (ball == null || ballRb == null))
        {
            FindBall();
        }
    }
}
