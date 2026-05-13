using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    [Header("Puntaje")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI requiredScoreText;
    [SerializeField] private TextMeshProUGUI totalScoreText; 
    [SerializeField] private TextMeshProUGUI levelNumberText; 

    [Header("Tiempo")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBackground;
    [SerializeField] private Color timerNormalColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    [SerializeField] private Color timerWarningColor = new Color(0.8f, 0.2f, 0.1f, 0.8f);
    [SerializeField] private float timerWarningThreshold = 20f;

    [Header("Feedback de Puntos")]
    [SerializeField] private TextMeshProUGUI pointsFeedbackText;
    [SerializeField] private float feedbackDuration = 1.5f;
    [SerializeField] private Color positiveColor = new Color(0.2f, 1f, 0.4f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.25f, 0.2f);

    [Header("Countdown")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Estadísticas Kill")]
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI civiliansKilledText;

    private CompetitionScoreSystem competitionScoreSystem;
    
    private Coroutine _feedbackCoroutine;
    private bool _isEndless;
    

    private void Start()
    {
        competitionScoreSystem = CompetitionScoreSystem.Instance;
        Debug.Log($"[GameHUD] competitionScoreSystem: {competitionScoreSystem}");

        competitionScoreSystem.OnScoreChanged += HandleScoreChanged;
        competitionScoreSystem.OnEnemyKilled += HandleEnemyKilled;
        competitionScoreSystem.OnCivilianKilled += HandleCivilianKilled;
        competitionScoreSystem.OnTimeBonusAwarded += HandleTimeBonus;

        _isEndless = EndlessModeManager.Instance != null;
        Debug.Log($"[GameHUD] isEndless: {_isEndless}");
        Debug.Log($"[GameHUD] CompetitionManager: {CompetitionManager.Instance}");
        Debug.Log($"[GameHUD] EndlessModeManager: {EndlessModeManager.Instance}");

        if (_isEndless)
            InitEndless();
        else
            InitCompetition();

        Debug.Log($"[GameHUD] Estado actual: {(_isEndless ? EndlessModeManager.Instance?.State.ToString() : CompetitionManager.Instance?.State.ToString())}");

        pointsFeedbackText.gameObject.SetActive(false);
        countdownPanel.SetActive(false);
        UpdateScoreUI(0);
    }

    private void OnDestroy()
    {
        if (competitionScoreSystem != null)
        {
            competitionScoreSystem.OnScoreChanged -= HandleScoreChanged;
            competitionScoreSystem.OnEnemyKilled -= HandleEnemyKilled;
            competitionScoreSystem.OnCivilianKilled -= HandleCivilianKilled;
            competitionScoreSystem.OnTimeBonusAwarded -= HandleTimeBonus;
        }
        
        if (_isEndless)
        {
            if (EndlessModeManager.Instance == null) return;
            EndlessModeManager.Instance.OnTimerUpdated -= HandleTimerUpdated;
            EndlessModeManager.Instance.OnStateChanged -= HandleEndlessStateChanged;
            EndlessModeManager.Instance.OnTotalScoreUpdated -= HandleTotalScoreUpdated;
        }
        else
        {
            if (CompetitionManager.Instance == null) return;
            CompetitionManager.Instance.OnTimerUpdated -= HandleTimerUpdated;
            CompetitionManager.Instance.OnStateChanged -= HandleCompetitionStateChanged;
        }
    }

    private void InitCompetition()
    {
        CompetitionManager.Instance.OnTimerUpdated += HandleTimerUpdated;
        CompetitionManager.Instance.OnStateChanged += HandleCompetitionStateChanged;

        if (requiredScoreText)
            requiredScoreText.text = CompetitionManager.Instance.CurrentConfig.requiredScore.ToString();

        if (totalScoreText) totalScoreText.gameObject.SetActive(false);
        if (levelNumberText) levelNumberText.gameObject.SetActive(false);

        HandleCompetitionStateChanged(CompetitionManager.Instance.State);
        UpdateScoreUI(competitionScoreSystem.CurrentScore);
    }

    private void InitEndless()
    {
        EndlessModeManager.Instance.OnTimerUpdated += HandleTimerUpdated;
        EndlessModeManager.Instance.OnStateChanged += HandleEndlessStateChanged;
        EndlessModeManager.Instance.OnTotalScoreUpdated += HandleTotalScoreUpdated;

        if (requiredScoreText) requiredScoreText.gameObject.SetActive(false);

        HandleEndlessStateChanged(EndlessModeManager.Instance.State);
        UpdateScoreUI(competitionScoreSystem.CurrentScore);
        UpdateLevelUI();
    }

    // ── Score ──────────────────────────────────────────────────────

    private void HandleScoreChanged(int newScore)
    {
        UpdateScoreUI(newScore);
    }

    private void HandleTotalScoreUpdated(int total)
    {
        if (totalScoreText)
            totalScoreText.text = total.ToString("N0");
        UpdateLevelUI();
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText)
            scoreText.text = score.ToString("N0");
    }

    private void UpdateLevelUI()
    {
        if (levelNumberText && EndlessModeManager.Instance != null)
            levelNumberText.text = $"Nivel {EndlessModeManager.Instance.CurrentLevelNumber}";
    }

    // ── Kills ──────────────────────────────────────────────────────

    private void HandleEnemyKilled(int points)
    {
        ShowPointsFeedback($"+{points}", positiveColor);
        if (enemiesKilledText && CompetitionScoreSystem.Instance != null)
            enemiesKilledText.text = $"x {CompetitionScoreSystem.Instance.EnemiesKilled}";
    }

    private void HandleCivilianKilled(int penalty)
    {
        ShowPointsFeedback($"{penalty}", negativeColor);
        if (civiliansKilledText && CompetitionScoreSystem.Instance != null)
            civiliansKilledText.text = $"x {CompetitionScoreSystem.Instance.CiviliansKilled}";
    }

    private void HandleTimeBonus(int bonus)
    {
        ShowPointsFeedback($"Time +{bonus} BONUS", positiveColor);
    }

    // ── Timer ──────────────────────────────────────────────────────

    private void HandleTimerUpdated(float timeRemaining)
    {
        UpdateTimerUI(timeRemaining);
    }

    private void UpdateTimerUI(float timeRemaining)
    {
        if (timerText)
        {
            float minutes = Mathf.FloorToInt(timeRemaining / 60f);
            float seconds = timeRemaining % 60f;
            timerText.text = $"{minutes:00}:{seconds:00.0}";
        }

        if (timerBackground)
        {
            bool isWarning = timeRemaining <= timerWarningThreshold;
            if (isWarning)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.PI * 2f);
                timerBackground.color = Color.Lerp(timerNormalColor, timerWarningColor, pulse);
            }
            else
            {
                timerBackground.color = timerNormalColor;
            }
        }
    }

    // ── States ─────────────────────────────────────────────────────

    private void HandleCompetitionStateChanged(CompetitionState state)
    {
        switch (state)
        {
            case CompetitionState.Countdown:
                countdownPanel.SetActive(true);
                StartCoroutine(RunCountdownAnimation());
                break;
            case CompetitionState.Racing:
                countdownPanel.SetActive(false);
                break;
        }
    }

    private void HandleEndlessStateChanged(EndlessState state)
    {
        switch (state)
        {
            case EndlessState.Countdown:
                countdownPanel.SetActive(true);
                StartCoroutine(RunCountdownAnimation());
                break;
            case EndlessState.Racing:
                countdownPanel.SetActive(false);
                break;
        }
    }
    

    // ── Feedback ───────────────────────────────────────────────────

    private void ShowPointsFeedback(string text, Color color)
    {
        if (pointsFeedbackText == null) return;
        if (_feedbackCoroutine != null)
            StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(FeedbackAnimation(text, color));
    }

    private IEnumerator FeedbackAnimation(string text, Color color)
    {
        pointsFeedbackText.gameObject.SetActive(true);
        pointsFeedbackText.text = text;
        pointsFeedbackText.color = color;
        pointsFeedbackText.transform.localScale = Vector3.one * 1.4f;

        float elapsed = 0f;
        Vector3 startPos = pointsFeedbackText.transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 60f;

        while (elapsed < feedbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / feedbackDuration;
            pointsFeedbackText.transform.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, t * 3f);
            pointsFeedbackText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);

            if (t > 0.5f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
                pointsFeedbackText.color = new Color(color.r, color.g, color.b, alpha);
            }
            yield return null;
        }

        pointsFeedbackText.gameObject.SetActive(false);
        pointsFeedbackText.transform.localPosition = startPos;
    }

    private IEnumerator RunCountdownAnimation()
    {
        if (countdownText == null) yield break;

        string[] steps = { "3", "2", "1", "¡GO!" };
        foreach (string step in steps)
        {
            countdownText.text = step;
            countdownText.transform.localScale = Vector3.one * 1.5f;

            float t = 0f;
            while (t < 0.85f)
            {
                t += Time.unscaledDeltaTime / 0.85f;
                countdownText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.15f);
        }

        if (countdownPanel) countdownPanel.SetActive(false);
    }
}