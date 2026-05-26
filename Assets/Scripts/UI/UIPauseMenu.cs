using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPauseMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        resumeBtn.onClick.AddListener(OnResumeBtnClicked);
        optionsBtn.onClick.AddListener(OnOptionsBtnClicked);
        exitBtn.onClick.AddListener(OnExitBtnClicked);
    }

    private void OnDestroy()
    {
        resumeBtn.onClick.RemoveListener(OnResumeBtnClicked);
        optionsBtn.onClick.RemoveListener(OnOptionsBtnClicked);
        exitBtn.onClick.RemoveListener(OnExitBtnClicked);
    }

    private void OnResumeBtnClicked()
    {
        gameObject.SetActive(false);
        gameManager.TogglePause();
    }

    private void OnOptionsBtnClicked()
    {
        gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(true);
    }

    // Bug: Alta - LoadScene directo a MainMenuScene sin pasar por CompetitionManager.ReturnToMainMenu() ni EndlessModeManager.ReturnToMainMenu(). A la próxima partida se duplican.
    // Bug: Media - Time.timeScale queda en 0 porque la pausa lo seteo; al cargar MainMenu el menú renderea con timeScale=0, animaciones congeladas.
    private void OnExitBtnClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }


}
