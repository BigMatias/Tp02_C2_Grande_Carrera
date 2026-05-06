using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameDataSO gameDataSO;
    [SerializeField] private GameEventSO enemyDiedEvent;
    [SerializeField] private GameEventSO civilianDiedEvent;
    [SerializeField] private CarController carController;
    [SerializeField] private GasSystem gasSystem;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject gameOverMenu;

    private bool gamePaused = false;
    private int currentPlayerScore = 0;

    public event Action onLevelCompleted;
    public event Action<int> onScoreUpdated;

    private void Awake()
    {
        enemyDiedEvent?.Subscribe(onEnemyDied);
        civilianDiedEvent?.Subscribe(onCivilianDied);
        carController.onPlayerDied += CarController_onPlayerDied;
        gasSystem.onGasDepleted += GasSystem_onGasDepleted;

        Time.timeScale = 1f;
    }

    private void Start()
    {
        currentPlayerScore = 0;
        onScoreUpdated?.Invoke(currentPlayerScore);

        SetCursorState(locked: true);

        CompetitionManager.Instance.OnStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(CompetitionManager.Instance.State);
    }

    private void OnDestroy()
    {
        enemyDiedEvent?.Unsubscribe(onEnemyDied);
        civilianDiedEvent?.Unsubscribe(onCivilianDied);
        carController.onPlayerDied -= CarController_onPlayerDied;

        CompetitionManager.Instance.OnStateChanged -= HandleGameStateChanged;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CompetitionManager.Instance != null &&
                CompetitionManager.Instance.State == CompetitionState.Racing)
            {
                TogglePause();
            }
        }
    }

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleGameStateChanged(CompetitionState state)
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

    private void onEnemyDied()
    {
        currentPlayerScore += gameDataSO.EnemyKilledScore;
        onScoreUpdated?.Invoke(currentPlayerScore);
    }

    private void onCivilianDied()
    {
        currentPlayerScore -= gameDataSO.CivilianKilledScore;
        onScoreUpdated?.Invoke(currentPlayerScore);
    }

    private void CarController_onPlayerDied()
    {
        Time.timeScale = 0f;
        SetCursorState(locked: false);
        gameOverMenu.gameObject.SetActive(true);
        if (CompetitionManager.Instance != null)
        {
            CompetitionManager.Instance.EndLevelWithFailure("Player Died");
        }
    }
    private void GasSystem_onGasDepleted()
    {
        if (CompetitionManager.Instance != null)
        {
            CompetitionManager.Instance.EndLevelWithFailure("Out of Gas");
        }
    }

    public void TogglePause()
    {
        gamePaused = !gamePaused;

        if (gamePaused)
        {
            Time.timeScale = 0f;
            SetCursorState(locked: false);
            pauseMenu.gameObject.SetActive(true);
            optionsMenu.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            SetCursorState(locked: true);
            pauseMenu.gameObject.SetActive(false);
            optionsMenu.gameObject.SetActive(false);
        }
    }
}
