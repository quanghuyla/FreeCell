using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject themePanel;
    public GameObject rulesPanel;

    [Header("Buttons")]
    public Button startGameButton;
    public Button themeButton;
    public Button rulesButton;
    public Button quitButton;

    [Header("Theme Panel Buttons")]
    public Button closeThemeButton;
    public Button greenThemeButton;
    public Button blueThemeButton;
    public Button redThemeButton;

    [Header("Rules Panel Buttons")]
    public Button closeRulesButton;

    void Start()
    {
        ShowMainMenu();

        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        if (themeButton != null)
            themeButton.onClick.AddListener(ShowThemePanel);

        if (rulesButton != null)
            rulesButton.onClick.AddListener(ShowRulesPanel);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (closeThemeButton != null)
            closeThemeButton.onClick.AddListener(ShowMainMenu);

        if (greenThemeButton != null)
            greenThemeButton.onClick.AddListener(() => SelectTheme("Green"));

        if (blueThemeButton != null)
            blueThemeButton.onClick.AddListener(() => SelectTheme("Blue"));

        if (redThemeButton != null)
            redThemeButton.onClick.AddListener(() => SelectTheme("Red"));

        if (closeRulesButton != null)
            closeRulesButton.onClick.AddListener(ShowMainMenu);
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (themePanel != null) themePanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(false);
    }

    void ShowThemePanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (themePanel != null) themePanel.SetActive(true);
        if (rulesPanel != null) rulesPanel.SetActive(false);
    }

    void ShowRulesPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (themePanel != null) themePanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(true);
    }

    void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    void SelectTheme(string themeName)
    {
        PlayerPrefs.SetString("Theme", themeName);
        PlayerPrefs.Save();
    }

    void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
