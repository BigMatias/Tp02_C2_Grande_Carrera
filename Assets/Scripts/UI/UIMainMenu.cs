using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private Button[] mainMenuButtons;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private Button creditsBackBtn;
    [SerializeField] private GameModeDataSO gameModeData;
    
    private void Awake()
    {
        mainMenuButtons[0].onClick.AddListener(OnCompetitionBtnClicked);
        mainMenuButtons[1].onClick.AddListener(OnEndlessBtnClicked);
        mainMenuButtons[2].onClick.AddListener(OnOptionsBtnClicked);
        mainMenuButtons[3].onClick.AddListener(OnCreditsBtnClicked);
        mainMenuButtons[4].onClick.AddListener(OnExitBtnClicked);
        creditsBackBtn.onClick.AddListener(OnCreditsBackBtnClicked);
    }
    
    private void OnDestroy()
    {
        mainMenuButtons[0].onClick.RemoveListener(OnCompetitionBtnClicked);
        mainMenuButtons[1].onClick.RemoveListener(OnEndlessBtnClicked);
        mainMenuButtons[2].onClick.RemoveListener(OnOptionsBtnClicked);
        mainMenuButtons[3].onClick.RemoveListener(OnCreditsBtnClicked);
        mainMenuButtons[4].onClick.RemoveListener(OnExitBtnClicked);
        creditsBackBtn.onClick.RemoveListener(OnCreditsBackBtnClicked);
    }
    
    private void OnCompetitionBtnClicked()
    {
        gameModeData.selectedMode = GameMode.Competition;
        SceneManager.LoadScene("GameScene");
    }

    private void OnEndlessBtnClicked()
    {
        gameModeData.selectedMode = GameMode.Competition;
        SceneManager.LoadScene("GameScene");
    }

    private void OnOptionsBtnClicked()
    {
        mainMenuPanel.SetActive(false);
        optionsMenu.gameObject.SetActive(true);
    }

    private void OnCreditsBtnClicked()
    {
        mainMenuPanel.gameObject.SetActive(false);
        creditsMenu.gameObject.SetActive(true);
    }
    private void OnCreditsBackBtnClicked()
    {
        creditsMenu.gameObject.SetActive(false);
        mainMenuPanel.gameObject.SetActive(true);
    }

    private void OnExitBtnClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
