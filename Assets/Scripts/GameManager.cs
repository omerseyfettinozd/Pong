using UnityEngine;
using TMPro; // TextMeshPro namespace

public class GameManager : MonoBehaviour
{
    [Header("UI References / UI Referansları")]
    [Tooltip("Text elements for Player 1 score (can be multiple) / 1. Oyuncu skoru için yazı elementleri (birden fazla olabilir)")]
    [SerializeField] private TextMeshProUGUI[] player1ScoreTexts;

    [Tooltip("Text elements for Player 2 (AI) score (can be multiple) / 2. Oyuncu (AI) skoru için yazı elementleri (birden fazla olabilir)")]
    [SerializeField] private TextMeshProUGUI[] player2ScoreTexts;

    [Header("Game References / Oyun Referansları")]
    [SerializeField] private BallController ball;

    private int player1Score = 0;
    private int player2Score = 0;

    // Prevents double scoring / Çift skor verilmesini engeller
    private bool canScore = true;

    // Singleton pattern for easy access / Kolay erişim için Singleton deseni
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // --- FPS SETTINGS (Android optimized) / FPS AYARLARI (Android için optimize) ---
        // Max 120 FPS cap / Maksimum 120 FPS limiti
        int screenRefreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        if (screenRefreshRate > 0)
        {
            Application.targetFrameRate = Mathf.Min(screenRefreshRate, 120);
        }
        else
        {
            Application.targetFrameRate = 120; // Default cap / Varsayılan limit
        }

        // Disable VSync for consistent frame pacing on mobile / Mobilde tutarlı frame hızı için VSync kapat
        QualitySettings.vSyncCount = 0;

        // --- PHYSICS SETTINGS / FİZİK AYARLARI ---
        // Higher physics tick rate for smoother ball at high speeds / Yüksek hızlarda daha pürüzsüz top için fizik tick hızını artır
        Time.fixedDeltaTime = 1f / 120f; // 120 Hz physics / 120 Hz fizik

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Called when the ball enters a score zone / Top gol bölgesine girdiğinde çağrılır
    public void Scored(bool player1Scored)
    {
        // Block if scoring is locked / Skor kilitliyse engelle
        if (!canScore) return;

        // Lock scoring immediately / Skoru hemen kilitle
        canScore = false;

        if (player1Scored)
        {
            player1Score++;
            Debug.Log("Player 1 (Sen) puan kazandı! Skor: " + player1Score);
        }
        else
        {
            player2Score++;
            Debug.Log("Player 2 (Rakip) puan kazandı! Skor: " + player2Score);
        }

        UpdateScoreUI();

        // Reset the ball / Topu resetle
        if (ball != null)
        {
            ball.ResetBall();
        }
    }

    // Called by BallController when the ball is launched / Top fırlatıldığında BallController tarafından çağrılır
    public void EnableScoring()
    {
        canScore = true;
    }

    // Updates all score texts on screen / Ekrandaki tüm skor yazılarını günceller
    private void UpdateScoreUI()
    {
        // Update all Player 1 score texts / Tüm 1. Oyuncu skor yazılarını güncelle
        foreach (TextMeshProUGUI text in player1ScoreTexts)
        {
            if (text != null)
                text.text = player1Score.ToString();
        }

        // Update all Player 2 score texts / Tüm 2. Oyuncu skor yazılarını güncelle
        foreach (TextMeshProUGUI text in player2ScoreTexts)
        {
            if (text != null)
                text.text = player2Score.ToString();
        }
    }
    
    // Can be used to restart the game / Oyunu yeniden başlatmak için kullanılabilir
    public void RestartGame()
    {
        player1Score = 0;
        player2Score = 0;
        canScore = true;
        UpdateScoreUI();
        if (ball != null) ball.ResetBall();
    }
}
