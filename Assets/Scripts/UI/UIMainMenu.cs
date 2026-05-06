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

    private void Awake()
    {
        mainMenuButtons[0].onClick.AddListener(OnPlayBtnClicked);
        mainMenuButtons[1].onClick.AddListener(OnOptionsBtnClicked);
        mainMenuButtons[2].onClick.AddListener(OnCreditsBtnClicked);
        mainMenuButtons[3].onClick.AddListener(OnExitBtnClicked);
        creditsBackBtn.onClick.AddListener(OnCreditsBackBtnClicked);
    }
    private void OnDestroy()
    {
        mainMenuButtons[0].onClick.RemoveListener(OnPlayBtnClicked);
        mainMenuButtons[1].onClick.RemoveListener(OnOptionsBtnClicked);
        mainMenuButtons[2].onClick.RemoveListener(OnCreditsBtnClicked);
        mainMenuButtons[3].onClick.RemoveListener(OnExitBtnClicked);
        creditsBackBtn.onClick.RemoveListener(OnCreditsBackBtnClicked);
    }

    private void OnPlayBtnClicked()
    {
        gameObject.SetActive(false);
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
        //Sale del estado "Play" del editor si estamos en el editor, de lo contrario sale de la aplicación si esta es una build.  
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
