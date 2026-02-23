using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Panels / Paneller")]
    [Tooltip("Main menu panel (1vs1, 1vsAI buttons) / Ana menü paneli")]
    [SerializeField] private GameObject mainMenuPanel;

    [Tooltip("Difficulty selection panel (Easy, Normal, Hard) / Zorluk seçim paneli")]
    [SerializeField] private GameObject difficultyPanel;

    [Tooltip("In-game UI panel (score, back button) / Oyun içi UI paneli")]
    [SerializeField] private GameObject inGamePanel;

    [Tooltip("Score text panel / Skor yazı paneli")]
    [SerializeField] private GameObject scoreTextPanel;

    [Header("References / Referanslar")]
    [Tooltip("Top paddle (AI or Player 2) / Üst raket (AI veya 2. Oyuncu)")]
    [SerializeField] private PaddleController paddleTop;

    [Tooltip("Bottom paddle (Player 1) / Alt raket (1. Oyuncu)")]
    [SerializeField] private PaddleController paddleBottom;

    [SerializeField] private BallController ball;
    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        // Show main menu at start / Başlangıçta ana menüyü göster
        ShowMainMenu();
    }

    // Shows the main menu / Ana menüyü gösterir
    public void ShowMainMenu()
    {
        // Stop the game / Oyunu durdur
        Time.timeScale = 0f;

        mainMenuPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        inGamePanel.SetActive(false);
        if (scoreTextPanel != null) scoreTextPanel.SetActive(false);

        // Reset ball and scores / Topu ve skorları sıfırla
        if (ball != null) ball.ResetBall();
        if (gameManager != null) gameManager.RestartGame();
    }

    // Called when "1vs1" button is pressed / "1vs1" butonuna basılınca çağrılır
    public void OnPvPButtonClicked()
    {
        // Set top paddle to Player controlled / Üst raketi oyuncu kontrolüne al
        if (paddleTop != null)
        {
            paddleTop.SetAIEnabled(false);
        }

        StartGame();
    }

    // Called when "1vsAI" button is pressed / "1vsAI" butonuna basılınca çağrılır
    public void OnPvAIButtonClicked()
    {
        // Show difficulty selection / Zorluk seçimini göster
        mainMenuPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // Called when Easy button is pressed / Kolay butonuna basılınca çağrılır
    public void OnEasyButtonClicked()
    {
        SetAIDifficultyAndStart(AIDifficulty.Easy);
    }

    // Called when Normal button is pressed / Normal butonuna basılınca çağrılır
    public void OnNormalButtonClicked()
    {
        SetAIDifficultyAndStart(AIDifficulty.Normal);
    }

    // Called when Hard button is pressed / Zor butonuna basılınca çağrılır
    public void OnHardButtonClicked()
    {
        SetAIDifficultyAndStart(AIDifficulty.Hard);
    }

    // Sets AI difficulty and starts the game / Yapay zeka zorluğunu ayarlayıp oyunu başlatır
    private void SetAIDifficultyAndStart(AIDifficulty difficulty)
    {
        if (paddleTop != null)
        {
            paddleTop.SetAIEnabled(true);
            paddleTop.SetDifficulty(difficulty);
        }

        StartGame();
    }

    // Starts the game / Oyunu başlatır
    private void StartGame()
    {
        mainMenuPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        inGamePanel.SetActive(true);
        if (scoreTextPanel != null) scoreTextPanel.SetActive(true);

        // Reset and start / Sıfırla ve başlat
        if (gameManager != null) gameManager.RestartGame();
        if (ball != null) ball.ResetBall();

        // Unpause / Oyunu devam ettir
        Time.timeScale = 1f;
    }

    // Called when back button is pressed during game / Oyun sırasında geri butonuna basılınca çağrılır
    public void OnBackToMenuButtonClicked()
    {
        ShowMainMenu();
    }
}
