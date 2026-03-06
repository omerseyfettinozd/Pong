using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Settings / Ayarlar")]
    [Tooltip("Initial speed of the ball / Topun başlangıç hızı")]
    [SerializeField] private float startSpeed = 5f;
    
    [Tooltip("Minimum speed increase per hit / Her vuruşta minimum hız artışı")]
    [SerializeField] private float minSpeedIncrease = 0.2f;

    [Tooltip("Maximum speed increase per hit / Her vuruşta maksimum hız artışı")]
    [SerializeField] private float maxSpeedIncrease = 1f;

    [Tooltip("Maximum speed limit / Maksimum hız limiti")]
    [SerializeField] private float maxSpeed = 100f;

    [Header("Wall Hit Speed / Duvar Vuruş Hızı")]
    [Tooltip("Minimum speed increase per wall hit / Her duvar vuruşunda minimum hız artışı")]
    [SerializeField] private float wallMinSpeedIncrease = 0.1f;

    [Tooltip("Maximum speed increase per wall hit / Her duvar vuruşunda maksimum hız artışı")]
    [SerializeField] private float wallMaxSpeedIncrease = 0.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private float currentSpeed;
    private Camera mainCamera;

    // Screen bounds / Ekran sınırları
    private float boundLeft;
    private float boundRight;
    private float boundTop;
    private float boundBottom;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        // --- PHYSICS OPTIMIZATION FOR HIGH SPEED / YÜKSEK HIZ İÇİN FİZİK OPTİMİZASYONU ---
        // Continuous Collision Detection: prevents ball from passing through walls/paddles at high speed
        // Sürekli Çarpışma Algılama: yüksek hızda topun duvar/raketlerden geçmesini önler
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Interpolation: smooths visual position between physics frames, prevents jitter
        // Enterpolasyon: fizik frame'leri arasında görsel pozisyonu yumuşatır, titreşimi önler
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Calculates screen bounds for ball clamping / Top sınırlaması için ekran sınırlarını hesaplar
    private void CalculateBounds()
    {
        if (mainCamera == null) return;
        float camHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float camHalfHeight = mainCamera.orthographicSize;
        float ballMargin = 0.3f; // Ball radius margin / Top yarıçap payı
        boundLeft = -camHalfWidth + ballMargin;
        boundRight = camHalfWidth - ballMargin;
        boundTop = camHalfHeight - ballMargin;
        boundBottom = -camHalfHeight + ballMargin;
    }

    private void Start()
    {
        CalculateBounds(); // After CameraResizer / CameraResizer'dan sonra
        ResetBall();
    }

    private void FixedUpdate()
    {
        // Clamp ball position within screen bounds / Topun pozisyonunu ekran sınırları içinde tut
        ClampPosition();
    }

    // Keeps ball inside screen bounds / Topu ekran sınırları içinde tutar
    private void ClampPosition()
    {
        Vector2 pos = transform.position;
        bool clamped = false;

        if (pos.x < boundLeft)
        {
            pos.x = boundLeft;
            rb.linearVelocity = new Vector2(Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
            clamped = true;
        }
        else if (pos.x > boundRight)
        {
            pos.x = boundRight;
            rb.linearVelocity = new Vector2(-Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y);
            clamped = true;
        }

        if (pos.y < boundBottom)
        {
            pos.y = boundBottom;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Abs(rb.linearVelocity.y));
            clamped = true;
        }
        else if (pos.y > boundTop)
        {
            pos.y = boundTop;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -Mathf.Abs(rb.linearVelocity.y));
            clamped = true;
        }

        if (clamped)
        {
            transform.position = pos;
            
            // Increase speed on screen bound hit / Kamera sınırına çarpınca hızı artır
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += Random.Range(wallMinSpeedIncrease, wallMaxSpeedIncrease);
                // We keep the calculated reflection direction but apply the new scalar speed
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }
    }

    // Resets ball position and launches it / Topun pozisyonunu sıfırlar ve fırlatır
    public void ResetBall()
    {
        // Cancel any pending launch / Bekleyen fırlatmayı iptal et
        CancelInvoke(nameof(Launch));

        // Disable collider to prevent any triggers / Tetiklemeleri önlemek için collider'ı kapat
        col.enabled = false;

        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;
        currentSpeed = startSpeed;
        
        // Launch after a short delay / Kısa bir gecikmeden sonra fırlat
        Invoke(nameof(Launch), 1f);
    }

    private void Launch()
    {
        // Re-enable collider / Collider'ı tekrar aç
        col.enabled = true;

        // Re-enable scoring in GameManager / GameManager'da skorlamayı tekrar aç
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnableScoring();
        }

        // Random direction / Rastgele yön
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(0, 2) == 0 ? -1 : 1;
        
        rb.linearVelocity = new Vector2(x * currentSpeed, y * currentSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Increase speed slightly on paddle hit / Rakete çarpınca hızı biraz artır
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += Random.Range(minSpeedIncrease, maxSpeedIncrease);
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }

            // Add angle variation based on where it hit the paddle
            // Raketin neresine çarptığına göre açı varyasyonu ekle
            float xDifference = transform.position.x - collision.transform.position.x;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + xDifference * 2, rb.linearVelocity.y).normalized * currentSpeed;

            // Safety: clamp position immediately after paddle hit
            ClampPosition();
        }
        else if (collision.gameObject.tag == "Wall")
        {
            // Increase speed on wall hit / Duvara çarpınca hızı artır
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += Random.Range(wallMinSpeedIncrease, wallMaxSpeedIncrease);
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }
    }
}
