using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using static CompetitionScoreSystem;

public class UICompetitionResult : MonoBehaviour
{
    [Header("References")]

    [Header("Panel principal")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Resultado (Encabezado)")]
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private Image resultBannerImage;
    [SerializeField] private Color passedColor = new Color(0.1f, 0.85f, 0.4f);
    [SerializeField] private Color failedColor = new Color(0.9f, 0.2f, 0.15f);

    [Header("Estadísticas Esenciales")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI totalTimeText; 

    [Header("Botones")]
    [SerializeField] private GameObject passedButtonsGroup;
    [SerializeField] private GameObject failedButtonsGroup;
    [SerializeField] private GameObject completedButtonsGroup;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButtonOnPass;
    [SerializeField] private Button menuButtonOnFail;
    [SerializeField] private Button menuButtonOnComplete;

    [Header("Animación")]
    [SerializeField] private float fadeInDuration = 0.5f;

    private void Awake()
    {
        nextLevelButton.onClick.AddListener(OnNextLevel);
        retryButton.onClick.AddListener(OnRetry);
        menuButtonOnPass.onClick.AddListener(OnMenu);
        menuButtonOnFail.onClick.AddListener(OnMenu);
        menuButtonOnComplete.onClick.AddListener(OnMenu);
    }

    private void Start()
    {
        CompetitionManager.Instance.OnStateChanged += HandleStateChanged;
        CompetitionManager.Instance.OnLevelEnded += ShowResult;

        resultPanel.SetActive(false);
    }

    private void OnDisable()
    {
        CompetitionManager.Instance.OnLevelEnded -= ShowResult;
        CompetitionManager.Instance.OnStateChanged -= HandleStateChanged;

        nextLevelButton.onClick.RemoveListener(OnNextLevel);
        retryButton.onClick.RemoveListener(OnRetry);
        menuButtonOnPass.onClick.RemoveListener(OnMenu);
        menuButtonOnFail.onClick.RemoveListener(OnMenu);
        menuButtonOnComplete.onClick.RemoveListener(OnMenu);
    }

    private void ShowResult(CompetitionResult result)
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        StartCoroutine(AnimateResultIn(result));
    }

    private IEnumerator AnimateResultIn(CompetitionResult result)
    {
        if (panelCanvasGroup)
        {
            panelCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                panelCanvasGroup.alpha = elapsed / fadeInDuration;
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
        }

        bool isCompetitionComplete = CompetitionManager.Instance != null && CompetitionManager.Instance.IsLastLevel && result.Passed;

        if (result.Passed)
        {
            if (resultTitleText) resultTitleText.text = isCompetitionComplete ? "¡TOURNEY COMPLETED!" : "¡LEVEL COMPLETED!";
            if (resultBannerImage) resultBannerImage.color = passedColor;
        }
        else
        {
            if (resultTitleText) resultTitleText.text = "RACE FAILED";
            if (resultBannerImage) resultBannerImage.color = failedColor;
        }

        if (totalTimeText) totalTimeText.text = result.FormattedTime.ToString();
        yield return StartCoroutine(CountUpScore(result.FinalScore, 0.8f));

        SetActiveButtons(result.Passed, isCompetitionComplete);
    }

    private IEnumerator CountUpScore(int targetScore, float duration)
    {
        if (finalScoreText == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            int display = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, elapsed / duration));
            finalScoreText.text = display.ToString("N0");
            yield return null;
        }
        finalScoreText.text = targetScore.ToString("N0");
    }

    private void SetActiveButtons(bool passed, bool isComplete)
    {
        if (passedButtonsGroup) passedButtonsGroup.SetActive(passed && !isComplete);
        if (failedButtonsGroup) failedButtonsGroup.SetActive(!passed);
        if (completedButtonsGroup) completedButtonsGroup.SetActive(isComplete);

        if (nextLevelButton && CompetitionManager.Instance != null)
            nextLevelButton.gameObject.SetActive(!CompetitionManager.Instance.IsLastLevel);
    }

    private void HandleStateChanged(CompetitionState state)
    {
        if (state == CompetitionState.LoadingLevel && resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void OnNextLevel() => CompetitionManager.Instance?.ProceedToNextLevel();
    private void OnRetry() => CompetitionManager.Instance?.RestartCurrentLevel();
    private void OnMenu() => CompetitionManager.Instance?.ReturnToMainMenu();
}