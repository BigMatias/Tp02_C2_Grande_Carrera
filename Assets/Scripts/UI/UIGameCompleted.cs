using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameCompleted : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button mainMenuBtn;

    private void Awake()
    {
        retryBtn.onClick.AddListener(OnYesBtnClicked);
        mainMenuBtn.onClick.AddListener(OnNoBtnClicked);
    }

    private void Start()
    {
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

    private void OnNoBtnClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
