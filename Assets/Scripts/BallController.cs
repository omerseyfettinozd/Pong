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
    [SerializeField] private float maxSpeed = 60f;

    private Rigidbody2D rb;
    private Collider2D col;
    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        ResetBall();
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
        // Bounce sound or effect could go here / Çarpma sesi veya efekti buraya eklenebilir

        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Increase speed slightly on paddle hit / Rakete çarpınca hızı biraz artır
            if (currentSpeed < maxSpeed)
            {
                // Random speed increase / Rastgele hız artışı
                currentSpeed += Random.Range(minSpeedIncrease, maxSpeedIncrease);
                
                // Adjust velocity vector / Hız vektörünü güncelle
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
            
            // Add angle variation based on where it hit the paddle
            // Raketin neresine çarptığına göre açı varyasyonu ekle
            float xDifference = transform.position.x - collision.transform.position.x;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + xDifference * 2, rb.linearVelocity.y).normalized * currentSpeed;
        }
    }
}
