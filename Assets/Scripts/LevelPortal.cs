using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string destinationScene = "TitleScreen";
    [SerializeField] private bool recordOnEnter = false;

    [Header("Portal Sound")]
    [SerializeField] private AudioClip portalEnterSound;

    private bool isLoading;

    public void GoToScene(string targetScene)
    {
        if (isLoading)
            return;

        isLoading = true;

        if (targetScene == "Game1")
        {
            GameSession.NewGame();
        }

        SceneManager.LoadScene(targetScene);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (recordOnEnter)
        {
            GameSession.SaveHighScore();
        }

        if (SoundManager.Instance != null &&
            portalEnterSound != null)
        {
            SoundManager.Instance.PlaySFX(portalEnterSound);
        }

        GoToScene(destinationScene);
    }
}
