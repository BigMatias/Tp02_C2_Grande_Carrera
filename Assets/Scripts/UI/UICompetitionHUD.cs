using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CompetitionHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CompetitionScoreSystem competitionScoreSystem;

    [Header("Puntaje")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI requiredScoreText;

    [Header("Tiempo")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBackground;
    [SerializeField] private Color timerNormalColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    [SerializeField] private Color timerWarningColor = new Color(0.8f, 0.2f, 0.1f, 0.8f);
    [SerializeField] private float timerWarningThreshold = 20f;

    [Header("Feedback de Puntos (popup flotante)")]
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

    private Coroutine _feedbackCoroutine;
    private float _currentTimeRemaining;
    private int _currentScore;

    private void OnEnable()
    {
        competitionScoreSystem.OnScoreChanged += HandleScoreChanged;
        competitionScoreSystem.OnEnemyKilled += HandleEnemyKilled;
        competitionScoreSystem.OnCivilianKilled += HandleCivilianKilled;
        competitionScoreSystem.OnTimeBonusAwarded += HandleTimeBonus;
    }

    private void Start()
    {
        CompetitionManager.Instance.OnTimerUpdated += HandleTimerUpdated;
        CompetitionManager.Instance.OnStateChanged += HandleStateChanged;
        var config = CompetitionManager.Instance.CurrentConfig;
        requiredScoreText.text = config.requiredScore.ToString();

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

        CompetitionManager.Instance.OnTimerUpdated -= HandleTimerUpdated;
        CompetitionManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleScoreChanged(int newScore)
    {
        _currentScore = newScore;
        UpdateScoreUI(newScore);
    }

    private void HandleEnemyKilled(int points)
    {
        ShowPointsFeedback($"+{points}", positiveColor);
        if (CompetitionScoreSystem.Instance != null && enemiesKilledText)
            enemiesKilledText.text = $"x {CompetitionScoreSystem.Instance.EnemiesKilled}";
    }

    private void HandleCivilianKilled(int penalty)
    {
        ShowPointsFeedback($"{penalty}", negativeColor);
        if (CompetitionScoreSystem.Instance != null && civiliansKilledText)
            civiliansKilledText.text = $"x {CompetitionScoreSystem.Instance.CiviliansKilled}";
    }

    private void HandleTimeBonus(int bonus)
    {
        ShowPointsFeedback($"Time +{bonus} BONUS", positiveColor);
    }

    private void HandleTimerUpdated(float timeRemaining)
    {
        _currentTimeRemaining = timeRemaining;
        UpdateTimerUI(timeRemaining);
    }

    private void HandleStateChanged(CompetitionState state)
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

    private void UpdateScoreUI(int score)
    {
        if (scoreText)
            scoreText.text = score.ToString("N0");
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
            timerBackground.color = isWarning ? timerWarningColor : timerNormalColor;

            if (isWarning)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.PI * 2f);
                timerBackground.color = Color.Lerp(timerNormalColor, timerWarningColor, pulse);
            }
        }
    }

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