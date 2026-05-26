using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Suggestion: Alta - Como en CompetitionManager: duplicación masiva de código (~80%). Una clase abstracta RaceModeManager con StartLevel(), OnPlayerFinishedLap(), HandleTimeOut() en común y polimorfismo para "qué hacer al terminar nivel" resolvería el smell.
public class EndlessModeManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private CompetitionLevelConfigSO[] levelConfigs;
    [SerializeField] private string mainMenuScene = "MainMenuScene";

    [Header("Timing")]
    [SerializeField] private float countdownBeforeStart = 3f;
    [SerializeField] private float resultScreenDelay = 1.5f;

    public static EndlessModeManager Instance { get; private set; }
    
    private CarController _carController;
    private EndlessState _state = EndlessState.Idle;
    private int _totalScore = 0;
    private int _currentLevelNumber = 0;
    private float _lapTimer = 0f;
    private bool _lapRunning = false;
    private CompetitionLevelConfigSO _currentConfig;
    private List<int> _shuffledIndexes = new List<int>();
    private int _shufflePointer = 0;

    public event Action<EndlessState> OnStateChanged;
    public event Action<float> OnTimerUpdated;
    public event Action<int> OnTotalScoreUpdated;
    public event Action OnGameOver;

    public EndlessState State => _state;
    public int TotalScore => _totalScore;
    public int CurrentLevelNumber => _currentLevelNumber;
    public float TimeRemaining => _currentConfig != null
        ? Mathf.Max(0f, _currentConfig.lapTimeLimit - _lapTimer)
        : 0f;
    public CompetitionLevelConfigSO CurrentConfig => _currentConfig;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CarSpawner.OnCarSpawned += HandleCarSpawned;
    }

    private void Start()
    {
        ShuffleIndexes();
        LoadNextLevel();
    }

    private void OnDestroy()
    {
        CarSpawner.OnCarSpawned -= HandleCarSpawned;

        if (_carController != null)
            _carController.onPlayerDied -= HandlePlayerDied;
    }

    private void Update()
    {
        if (!_lapRunning) return;

        _lapTimer += Time.deltaTime;
        OnTimerUpdated?.Invoke(TimeRemaining);

        if (_lapTimer >= _currentConfig.lapTimeLimit)
        {
            _lapRunning = false;
            HandleTimeOut();
        }
    }
    
    private void HandleCarSpawned(GasSystem gas, HealthSystemV2 health)
    {
        if (_carController != null)
            _carController.onPlayerDied -= HandlePlayerDied;

        _carController = gas.GetComponent<CarController>();
        _carController.onPlayerDied += HandlePlayerDied;
    }
    
    private void ShuffleIndexes()
    {
        _shuffledIndexes.Clear();
        for (int i = 0; i < levelConfigs.Length; i++)
            _shuffledIndexes.Add(i);

        for (int i = _shuffledIndexes.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (_shuffledIndexes[i], _shuffledIndexes[j]) = (_shuffledIndexes[j], _shuffledIndexes[i]);
        }

        _shufflePointer = 0;
    }

    private CompetitionLevelConfigSO GetNextConfig()
    {
        if (_shufflePointer >= _shuffledIndexes.Count)
            ShuffleIndexes();

        return levelConfigs[_shuffledIndexes[_shufflePointer++]];
    }

    private void LoadNextLevel()
    {
        _currentConfig = GetNextConfig();
        _lapTimer = 0f;
        _lapRunning = false;
        _currentLevelNumber++;

        SetState(EndlessState.LoadingLevel);

        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.Initialize(_currentConfig);

        StartCoroutine(LoadLevelAndCountdown());
    }

    private IEnumerator LoadLevelAndCountdown()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(_currentConfig.sceneName, LoadSceneMode.Single);
        while (!load.isDone)
            yield return null;

        yield return null;

        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.Initialize(_currentConfig);

        SetState(EndlessState.Countdown);
        yield return new WaitForSecondsRealtime(countdownBeforeStart);

        _lapTimer = 0f;
        _lapRunning = true;
        SetState(EndlessState.Racing);
    }

    public void OnPlayerFinishedLap()
    {
        if (!_lapRunning) return;
        _lapRunning = false;

        if (CompetitionScoreSystem.Instance != null)
        {
            CompetitionScoreSystem.Instance.RegisterLapCompletion(TimeRemaining);
            AccumulateScore();
        }

        StartCoroutine(ProceedAfterDelay());
    }

    private void HandleTimeOut()
    {
        if (CompetitionScoreSystem.Instance != null)
            AccumulateScore();

        StartCoroutine(ProceedAfterDelay());
    }
    
    private void HandlePlayerDied()
    {
        if (_state != EndlessState.Racing) return;

        _lapRunning = false;

        if (CompetitionScoreSystem.Instance != null)
            AccumulateScore();

        SetState(EndlessState.GameOver);
        OnGameOver?.Invoke();
    }

    private IEnumerator ProceedAfterDelay()
    {
        SetState(EndlessState.EvaluatingResult);
        yield return new WaitForSecondsRealtime(resultScreenDelay);
        LoadNextLevel();
    }

    private void AccumulateScore()
    {
        if (CompetitionScoreSystem.Instance == null) return;
        _totalScore += CompetitionScoreSystem.Instance.CurrentScore;
        OnTotalScoreUpdated?.Invoke(_totalScore);
    }

    public void ReturnToMainMenu()
    {
        SetState(EndlessState.Idle);
        Destroy(gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }

    private void SetState(EndlessState newState)
    {
        _state = newState;
        OnStateChanged?.Invoke(_state);
    }
    
    public void SetInitialTrack(int trackIndex)
    {
        _shufflePointer = trackIndex;
    }
}


public enum EndlessState
{
    Idle,
    LoadingLevel,
    Countdown,
    Racing,
    EvaluatingResult,
    GameOver
}