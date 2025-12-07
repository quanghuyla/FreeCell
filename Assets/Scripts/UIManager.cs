using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    public TextMeshProUGUI moveCountText;
    public GameObject winPanel;
    public Button newGameButton;
    public Button playAgainButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button undoButton;
    public Button redoButton;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(() =>
            {
                HideWinPanel();
                GameManager.Instance.NewGame();
            });
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(() =>
            {
                HideWinPanel();
                GameManager.Instance.NewGame();
            });
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                HideWinPanel();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartRound();
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            });
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenu");
            });
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
    
    public void UpdateMoveCount(int moves)
    {
        if (moveCountText != null)
        {
            moveCountText.text = "Moves: " + moves;
        }
    }
    
    public void ShowWinScreen(int moves)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            TextMeshProUGUI winText = winPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (winText != null)
            {
                winText.text = "You Win!\nMoves: " + moves;
            }
        }
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }
}
