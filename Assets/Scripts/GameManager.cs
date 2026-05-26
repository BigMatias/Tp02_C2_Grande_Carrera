using System;
using UnityEngine;

// Suggestion: Media - GameManager está acoplado a CompetitionManager y EndlessModeManager mediante if/else (_isEndless) repartidos por toda la clase. Debería abstraerse en una interfaz IGameModeManager con polimorfismo, evitando los branches en HandleCompetitionStateChanged / HandleEndlessStateChanged (lógica casi idéntica).
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameDataSO gameDataSO;
    [SerializeField] private GameEventSO enemyDiedEvent;
    [SerializeField] private GameEventSO civilianDiedEvent;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;

    private CarController _carController;
    private GasSystem _gasSystem;
    private bool _gamePaused = false;
    private bool _isEndless;

    private void Awake()
    {
        enemyDiedEvent?.Subscribe(onEnemyDied);
        civilianDiedEvent?.Subscribe(onCivilianDied);
        CarSpawner.OnCarSpawned += HandleCarSpawned;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        _isEndless = EndlessModeManager.Instance != null;
        SetCursorState(locked: true);

        if (_isEndless)
        {
            EndlessModeManager.Instance.OnStateChanged += HandleEndlessStateChanged;
            HandleEndlessStateChanged(EndlessModeManager.Instance.State);
        }
        else
        {
            CompetitionManager.Instance.OnStateChanged += HandleCompetitionStateChanged;
            HandleCompetitionStateChanged(CompetitionManager.Instance.State);
        }
    }

    private void OnDestroy()
    {
        enemyDiedEvent?.Unsubscribe(onEnemyDied);
        civilianDiedEvent?.Unsubscribe(onCivilianDied);
        CarSpawner.OnCarSpawned -= HandleCarSpawned;

        if (_carController != null)
            _carController.onPlayerDied -= CarController_onPlayerDied;

        if (_gasSystem != null)
            _gasSystem.onGasDepleted -= GasSystem_onGasDepleted;

        if (_isEndless)
        {
            if (EndlessModeManager.Instance != null)
                EndlessModeManager.Instance.OnStateChanged -= HandleEndlessStateChanged;
        }
        else
        {
            if (CompetitionManager.Instance != null)
                CompetitionManager.Instance.OnStateChanged -= HandleCompetitionStateChanged;
        }
    }

    // Warning: Media - Recibir el CarController vía GetComponent del gasSystem mezcla responsabilidades. 
    // Bug: Media - No se valida si gas o health son null (CarSpawner.Awake los obtiene de GetComponent, podrían no existir en el prefab).
    private void HandleCarSpawned(GasSystem gas, HealthSystemV2 health)
    {
        _gasSystem = gas;
        _carController = gas.GetComponent<CarController>();

        _carController.onPlayerDied += CarController_onPlayerDied;
        _gasSystem.onGasDepleted += GasSystem_onGasDepleted;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        bool isRacing = _isEndless
            ? EndlessModeManager.Instance?.State == EndlessState.Racing
            : CompetitionManager.Instance?.State == CompetitionState.Racing;

        if (isRacing) TogglePause();
    }

    private void HandleCompetitionStateChanged(CompetitionState state)
    {
        switch (state)
        {
            case CompetitionState.Countdown:
                Time.timeScale = 0f;
                SetCursorState(locked: true);
                break;
            case CompetitionState.Racing:
                Time.timeScale = 1f;
                SetCursorState(locked: true);
                break;
            case CompetitionState.EvaluatingResult:
            case CompetitionState.ShowingResult:
                Time.timeScale = 0f;
                SetCursorState(locked: false);
                break;
        }
    }

    private void HandleEndlessStateChanged(EndlessState state)
    {
        switch (state)
        {
            case EndlessState.Countdown:
                Time.timeScale = 0f;
                SetCursorState(locked: true);
                break;
            case EndlessState.Racing:
                Time.timeScale = 1f;
                SetCursorState(locked: true);
                break;
            case EndlessState.EvaluatingResult:
            case EndlessState.GameOver:
                Time.timeScale = 0f;
                SetCursorState(locked: false);
                break;
        }
    }

    private void onEnemyDied()
    {
        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.RegisterEnemyKill();
    }

    private void onCivilianDied()
    {
        if (CompetitionScoreSystem.Instance != null)
            CompetitionScoreSystem.Instance.RegisterCivilianKill();
    }

    private void CarController_onPlayerDied()
    {
        Time.timeScale = 0f;
        SetCursorState(locked: false);

        if (!_isEndless)
            CompetitionManager.Instance?.EndLevelWithFailure("Player Died");
    }

    private void GasSystem_onGasDepleted()
    {
        if (!_isEndless)
            CompetitionManager.Instance?.EndLevelWithFailure("Out of Gas");
    }

    // Suggestion: Baja - pauseMenu y optionsMenu son GameObject; el ".gameObject" en pauseMenu.gameObject.SetActive es redundante.
    public void TogglePause()
    {
        _gamePaused = !_gamePaused;

        Time.timeScale = _gamePaused ? 0f : 1f;
        SetCursorState(locked: !_gamePaused);
        pauseMenu.gameObject.SetActive(_gamePaused);
        if (!_gamePaused) optionsMenu.gameObject.SetActive(false);
    }

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}