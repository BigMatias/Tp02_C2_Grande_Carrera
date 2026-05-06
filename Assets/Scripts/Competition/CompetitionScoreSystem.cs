using UnityEngine;
using System;

public class CompetitionScoreSystem : MonoBehaviour
{
    [SerializeField] private GameDataSO gameDataSO;

    [Serializable]
    public class CompetitionResult
    {
        public string LevelName;
        public int FinalScore;
        public int RequiredScore;
        public int EnemiesKilled;
        public int CiviliansKilled;
        public int TimeBonus;
        public float TotalTime;
        public bool Passed;

        public string FormattedTime => $"{Mathf.FloorToInt(TotalTime / 60f):00}:{TotalTime % 60f:00.00}";
    }

    public static CompetitionScoreSystem Instance { get; private set; }
    public event Action<int> OnScoreChanged;
    public event Action<int> OnEnemyKilled;
    public event Action<int> OnCivilianKilled;
    public event Action<int> OnTimeBonusAwarded;

    private int _currentScore = 0;
    private int _enemiesKilled = 0;
    private int _civiliansKilled = 0;
    private int _timeBonus = 0;
    private CompetitionLevelConfigSO _config;

    public int CurrentScore => _currentScore;
    public int EnemiesKilled => _enemiesKilled;
    public int CiviliansKilled => _civiliansKilled;
    public int TimeBonus => _timeBonus;
    public int RequiredScore => _config != null ? _config.requiredScore : 0;
    public bool HasPassedLevel => _currentScore >= RequiredScore;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Initialize(CompetitionLevelConfigSO config)
    {
        _config = config;
        _currentScore = 0;
        _enemiesKilled = 0;
        _civiliansKilled = 0;
        _timeBonus = 0;
        OnScoreChanged?.Invoke(_currentScore);
    }

    public void RegisterEnemyKill()
    {
        if (_config == null) return;
        int points = gameDataSO.EnemyKilledScore;
        AddScore(points);
        _enemiesKilled++;
        OnEnemyKilled?.Invoke(points);

        Debug.Log($"[Score] Enemigo eliminado +{points} | Total: {_currentScore}");
    }

    public void RegisterCivilianKill()
    {
        if (_config == null) return;
        int penalty = -gameDataSO.CivilianKilledScore;
        AddScore(penalty);
        _civiliansKilled++;
        OnCivilianKilled?.Invoke(penalty);

        Debug.Log($"[Score] ¡Civil eliminado! {penalty} | Total: {_currentScore}");
    }

    public void RegisterLapCompletion(float timeRemaining)
    {
        if (_config == null) return;
        _timeBonus = _config.CalculateTimeBonus(timeRemaining);
        if (_timeBonus > 0)
        {
            AddScore(_timeBonus);
            OnTimeBonusAwarded?.Invoke(_timeBonus);
            Debug.Log($"[Score] Bonus de tiempo: +{_timeBonus} | Total: {_currentScore}");
        }
    }

    private void AddScore(int amount)
    {
        _currentScore = Mathf.Max(0, _currentScore + amount);
        OnScoreChanged?.Invoke(_currentScore);
    }

    public CompetitionResult GetResult(bool levelPassed, float totalTime)
    {
        return new CompetitionResult
        {
            LevelName = _config != null ? _config.levelName : "Carrera",
            FinalScore = _currentScore,
            RequiredScore = RequiredScore,
            EnemiesKilled = _enemiesKilled,
            CiviliansKilled = _civiliansKilled,
            TimeBonus = _timeBonus,
            TotalTime = totalTime,
            Passed = levelPassed
        };
    }
}

