using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using static CompetitionScoreSystem;

public class UIGameResult : MonoBehaviour
{
    [Header("Panel principal")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Resultado (Encabezado)")]
    [SerializeField] private TextMeshProUGUI resultTitleText;
    [SerializeField] private Image resultBannerImage;
    [SerializeField] private Color passedColor = new Color(0.1f, 0.85f, 0.4f);
    [SerializeField] private Color failedColor = new Color(0.9f, 0.2f, 0.15f);

    [Header("Stats")]
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

    [Header("Endless (Game Over)")]
    [SerializeField] private GameObject endlessGameOverButtonsGroup;
    [SerializeField] private Button menuButtonOnGameOver;
    [SerializeField] private TextMeshProUGUI levelsCompletedText;

    [Header("Animación")]
    [SerializeField] private float fadeInDuration = 0.5f;

    private bool _isEndless;

    // Suggestion: Baja - Suscripción defensiva con ?.onClick es ok, pero la desuscripción en OnDisable. Asimetrico.
    private void Awake()
    {
        nextLevelButton?.onClick.AddListener(OnNextLevel);
        retryButton?.onClick.AddListener(OnRetry);
        menuButtonOnPass?.onClick.AddListener(OnMenu);
        menuButtonOnFail?.onClick.AddListener(OnMenu);
        menuButtonOnComplete?.onClick.AddListener(OnMenu);
        menuButtonOnGameOver?.onClick.AddListener(OnMenu);
    }

    private void Start()
    {
        _isEndless = EndlessModeManager.Instance != null;

        if (_isEndless)
        {
            EndlessModeManager.Instance.OnGameOver += HandleGameOver;
            EndlessModeManager.Instance.OnStateChanged += HandleEndlessStateChanged;
        }
        else
        {
            CompetitionManager.Instance.OnStateChanged += HandleCompetitionStateChanged;
            CompetitionManager.Instance.OnLevelEnded += ShowCompetitionResult;
        }

        resultPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (_isEndless)
        {
            if (EndlessModeManager.Instance == null) return;
            EndlessModeManager.Instance.OnGameOver -= HandleGameOver;
            EndlessModeManager.Instance.OnStateChanged -= HandleEndlessStateChanged;
        }
        else
        {
            if (CompetitionManager.Instance == null) return;
            CompetitionManager.Instance.OnLevelEnded -= ShowCompetitionResult;
            CompetitionManager.Instance.OnStateChanged -= HandleCompetitionStateChanged;
        }

        nextLevelButton?.onClick.RemoveListener(OnNextLevel);
        retryButton?.onClick.RemoveListener(OnRetry);
        menuButtonOnPass?.onClick.RemoveListener(OnMenu);
        menuButtonOnFail?.onClick.RemoveListener(OnMenu);
        menuButtonOnComplete?.onClick.RemoveListener(OnMenu);
        menuButtonOnGameOver?.onClick.RemoveListener(OnMenu);
    }

    // ── Competición ────────────────────────────────────────────────

    private void ShowCompetitionResult(CompetitionResult result)
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        StartCoroutine(AnimateCompetitionResultIn(result));
    }

    private IEnumerator AnimateCompetitionResultIn(CompetitionResult result)
    {
        yield return StartCoroutine(FadeIn());

        bool isComplete = CompetitionManager.Instance != null 
            && CompetitionManager.Instance.IsLastLevel 
            && result.Passed;

        if (resultTitleText)
            resultTitleText.text = result.Passed
                ? (isComplete ? "¡TOURNEY COMPLETED!" : "¡LEVEL COMPLETED!")
                : "RACE FAILED";

        if (resultBannerImage)
            resultBannerImage.color = result.Passed ? passedColor : failedColor;

        if (totalTimeText)
            totalTimeText.text = result.FormattedTime;

        yield return StartCoroutine(CountUpScore(result.FinalScore, 0.8f));

        SetActiveCompetitionButtons(result.Passed, isComplete);
    }

    private void HandleCompetitionStateChanged(CompetitionState state)
    {
        if (state == CompetitionState.LoadingLevel && resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void SetActiveCompetitionButtons(bool passed, bool isComplete)
    {
        if (passedButtonsGroup) passedButtonsGroup.SetActive(passed && !isComplete);
        if (failedButtonsGroup) failedButtonsGroup.SetActive(!passed);
        if (completedButtonsGroup) completedButtonsGroup.SetActive(isComplete);
        if (endlessGameOverButtonsGroup) endlessGameOverButtonsGroup.SetActive(false);

        if (nextLevelButton && CompetitionManager.Instance != null)
            nextLevelButton.gameObject.SetActive(!CompetitionManager.Instance.IsLastLevel);
    }

    // ── Endless ────────────────────────────────────────────────────

    private void HandleGameOver()
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        StartCoroutine(AnimateGameOverIn());
    }

    private IEnumerator AnimateGameOverIn()
    {
        yield return StartCoroutine(FadeIn());

        if (resultTitleText) resultTitleText.text = "GAME OVER";
        if (resultBannerImage) resultBannerImage.color = failedColor;
        if (totalTimeText) totalTimeText.gameObject.SetActive(false);

        if (levelsCompletedText)
            levelsCompletedText.text = $"Niveles completados: {EndlessModeManager.Instance.CurrentLevelNumber - 1}";

        yield return StartCoroutine(CountUpScore(EndlessModeManager.Instance.TotalScore, 0.8f));

        if (passedButtonsGroup) passedButtonsGroup.SetActive(false);
        if (failedButtonsGroup) failedButtonsGroup.SetActive(false);
        if (completedButtonsGroup) completedButtonsGroup.SetActive(false);
        if (endlessGameOverButtonsGroup) endlessGameOverButtonsGroup.SetActive(true);
    }

    private void HandleEndlessStateChanged(EndlessState state)
    {
        if (state == EndlessState.LoadingLevel && resultPanel != null)
            resultPanel.SetActive(false);
    }

    // ── Compartido ─────────────────────────────────────────────────

    private IEnumerator FadeIn()
    {
        if (panelCanvasGroup == null) yield break;

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

    private void OnNextLevel() => CompetitionManager.Instance?.ProceedToNextLevel();
    private void OnRetry() => CompetitionManager.Instance?.RestartCurrentLevel();
    private void OnMenu()
    {
        if (_isEndless)
            EndlessModeManager.Instance?.ReturnToMainMenu();
        else
            CompetitionManager.Instance?.ReturnToMainMenu();
    }
}