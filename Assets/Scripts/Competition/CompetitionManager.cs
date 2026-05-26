using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CompetitionScoreSystem;

// Suggestion: Alta - El TP exige "lograr una X cantidad de vueltas completas (configurable antes de empezar el nivel)". Acá la "carrera" se gana cruzando UNA vez la meta. No hay sistema de vueltas múltiples.
// Suggestion: Media - CompetitionManager y EndlessModeManager comparten ~80% de lógica (countdown, racing, evaluación, GameOver). Debería existir una clase base abstracta RaceModeManager para evitar duplicar código.
public class CompetitionManager : MonoBehaviour
{
    [Header("Configuración de Niveles")]
    [SerializeField] private CompetitionLevelConfigSO[] levelConfigs = new CompetitionLevelConfigSO[3];

    [Header("Escenas")]
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    [Header("Timing")]
    [SerializeField] private float countdownBeforeStart = 3f;
    [Tooltip("Delay")]
    [SerializeField] private float resultScreenDelay = 1.5f;
    public static CompetitionManager Instance { get; private set; }

    private CompetitionState _state = CompetitionState.Idle;
    private int _currentLevelIndex = 0;
    private float _lapTimer = 0f;
    private bool _lapRunning = false;
    private CompetitionResult _lastResult;

    public event Action<CompetitionState> OnStateChanged;
    public event Action<float> OnTimerUpdated;
    public event Action<CompetitionResult> OnLevelEnded;

    public CompetitionState State => _state;
    public int CurrentLevelIndex => _currentLevelIndex;
    public float TimeRemaining => Mathf.Max(0f, CurrentConfig.lapTimeLimit - _lapTimer);
    // Bug: Media - CurrentConfig no valida que _currentLevelIndex esté dentro de bounds. 
    public CompetitionLevelConfigSO CurrentConfig => levelConfigs[_currentLevelIndex];
    public bool IsLastLevel => _currentLevelIndex >= levelConfigs.Length - 1;
    public CompetitionResult LastResult => _lastResult;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartLevel(_currentLevelIndex);
    }

    private void Update()
    {
        if (!_lapRunning) return;

        _lapTimer += Time.deltaTime;
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (_lapTimer >= CurrentConfig.lapTimeLimit)
        {
            _lapRunning = false;
            HandleTimeOut();
        }
    }

    public void StartLevel(int levelIndex)
    {
        if (levelIndex >= levelConfigs.Length)
        {
            return;
        }

        _currentLevelIndex = levelIndex;
        _lapTimer = 0f;
        _lapRunning = false;

        SetState(CompetitionState.LoadingLevel);

        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.Initialize(CurrentConfig);

        StartCoroutine(LoadLevelAndCountdown());
    }

    private IEnumerator LoadLevelAndCountdown()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(CurrentConfig.sceneName, LoadSceneMode.Single);
        while (!load.isDone)
            yield return null;

        yield return null;

        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.Initialize(CurrentConfig);

        SetState(CompetitionState.Countdown);
        yield return new WaitForSecondsRealtime(countdownBeforeStart);

        BeginLap();
    }

    private void BeginLap()
    {
        _lapTimer = 0f;
        _lapRunning = true;
        SetState(CompetitionState.Racing);
    }

    public void OnPlayerFinishedLap()
    {
        if (!_lapRunning) return;
        _lapRunning = false;

        float timeUsed = _lapTimer;
        float timeRemaining = TimeRemaining;

        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.RegisterLapCompletion(timeRemaining);

        StartCoroutine(EvaluateLapResult(timeUsed));
    }

    public void EndLevelWithFailure(string reason = "Failure")
    {
        if (_state != CompetitionState.Racing) return;

        Debug.Log($"[CompetitionManager] Carrera fallida: {reason}");
        _lapRunning = false;

        StartCoroutine(ShowResultWithDelay(BuildResult(false, _lapTimer)));
    }

    public void OnPlayerDied()
    {
        EndLevelWithFailure("Player Died");
    }

    private void HandleTimeOut()
    {
        Debug.Log("[CompetitionManager] �Tiempo agotado!");
        StartCoroutine(EvaluateLapResult(_lapTimer));
    }

    private IEnumerator EvaluateLapResult(float totalTime)
    {
        SetState(CompetitionState.EvaluatingResult);
        yield return new WaitForSecondsRealtime(resultScreenDelay);

        bool passed = CompetitionScoreSystem.Instance != null
            ? CompetitionScoreSystem.Instance.HasPassedLevel
            : false;

        CompetitionResult result = BuildResult(passed, totalTime);
        yield return StartCoroutine(ShowResultWithDelay(result));
    }

    private IEnumerator ShowResultWithDelay(CompetitionResult result)
    {
        _lastResult = result;
        SetState(CompetitionState.ShowingResult);
        OnLevelEnded?.Invoke(result);

        yield break;
    }

    private CompetitionResult BuildResult(bool passed, float totalTime)
    {
        if (CompetitionScoreSystem.Instance != null)
            return CompetitionScoreSystem.Instance.GetResult(passed, totalTime);

        return new CompetitionResult
        {
            LevelName = CurrentConfig.levelName,
            FinalScore = 0,
            RequiredScore = CurrentConfig.requiredScore,
            Passed = passed,
            TotalTime = totalTime
        };
    }

    public void ProceedToNextLevel()
    {
        if (!IsLastLevel)
        {
            StartLevel(_currentLevelIndex + 1);
        }
        else
        {
            SetState(CompetitionState.CompetitionComplete);
        }
    }

    public void RestartCurrentLevel()
    {
        StartLevel(_currentLevelIndex);
    }

    // Bug: Alta - Llamar Destroy(gameObject) y luego SceneManager.LoadScene puede dejar el singleton "Instance" colgando. Raro destruir el objeto.
    // Warning: Media - Time.timeScale puede haber quedado en 0 (pause/result); al ir al main menu sin restaurarlo a 1 en un propio scenemanager
    public void ReturnToMainMenu()
    {
        SetState(CompetitionState.Idle);
        Destroy(gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }

    private void SetState(CompetitionState newState)
    {
        _state = newState;
        OnStateChanged?.Invoke(_state);
    }
}