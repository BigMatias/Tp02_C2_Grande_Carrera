using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Error: Media - El campo gameOverPanel y gameManager está serializado pero nunca se usa
public class UIGameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button mainMenuBtn;

    private void Awake()
    {
        retryBtn.onClick.AddListener(OnYesBtnClicked);
        mainMenuBtn.onClick.AddListener(OnNoBtnClicked);
    }

    private void OnDestroy()
    {
        retryBtn.onClick.RemoveListener(OnYesBtnClicked);
        mainMenuBtn.onClick.RemoveListener(OnNoBtnClicked);
    }

    private void OnYesBtnClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Bug: Alta - Inconsistencia: acá la escena de menú es "MainMenu" pero en otros scripts ("UIGameCompleted", "UIPauseMenu", "EndlessModeManager", "UISelectionMenu") es "MainMenuScene".
    private void OnNoBtnClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }


}
