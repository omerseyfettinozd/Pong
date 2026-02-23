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
        // Cihazın ekran yenileme hızını (Hz) alıp FPS limitini ona göre ayarlıyoruz (Mobil, Windows vb. tüm platformlar için) / Sets target FPS to match screen refresh rate
        Application.targetFrameRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        
        // Eğer cihazın yenileme hızı alınamazsa (örneğin bazı sistemlerde 0 dönebilir), limiti devre dışı bırakıyoruz (-1 ile)
        if (Application.targetFrameRate <= 0)
        {
            Application.targetFrameRate = -1;
        }

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
