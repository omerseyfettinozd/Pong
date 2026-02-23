using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    [Tooltip("If true, Player 1 gets the point. If false, Player 2 gets it. / Doğruysa puanı 1. Oyuncu alır. Yanlışsa 2. Oyuncu alır.")]
    [SerializeField] private bool givePointToPlayer1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is the ball / Nesnenin top olup olmadığını kontrol et
        if (collision.CompareTag("Ball"))
        {
            // Notify GameManager (it handles double-score prevention itself)
            // GameManager'a haber ver (çift skor önlemeyi kendisi yapar)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Scored(givePointToPlayer1);
            }
        }
    }
}
