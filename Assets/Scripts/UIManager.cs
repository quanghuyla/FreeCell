using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    public TextMeshProUGUI moveCountText;
    public GameObject winPanel;
    public Button newGameButton;
    public Button playAgainButton;
    
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
            playAgainButton.onClick.AddListener(() => {
                winPanel.SetActive(false);
                GameManager.Instance.NewGame();
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
}