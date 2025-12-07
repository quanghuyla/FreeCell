using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuManager : MonoBehaviour
{
    public Button restartButton;
    public Button mainMenuButton;
    public Button undoButton;
    public Button redoButton;

    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (undoButton != null)
        {
            undoButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UndoMove();
                }
            });
        }

        if (redoButton != null)
        {
            redoButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RedoMove();
                }
            });
        }
    }

    void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NewGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
