using UnityEngine;

// Suggestion: Media - El TP exige "3 tipos de volumen: Background, VFX y UI", pero esta clase y UIOptions controlan Master/Sfx/Music. Falta el control independiente de UI volume;
// Suggestion: Media - AudioManager hace tanto lógica de eventos (suscripciones a CarController, eventos de muerte) como sonido. Mezcla responsabilidades.
public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameEventSO enemyDiedEvent;
    [SerializeField] private GameEventSO civilianDiedEvent;

    [Header("AudioClips")]
    [SerializeField] private AudioClip[] music;
    // Warning: Baja - Typo "Killled" (tres L) en los nombres de campo
    [SerializeField] private AudioClip enemyKillledSfx;
    [SerializeField] private AudioClip civilianKillledSfx;
    [SerializeField] private AudioClip playerHurtSfx;
    [SerializeField] private AudioClip playerDiedSfx;
    [SerializeField] private AudioClip playerShootM1Sfx;
    [SerializeField] private AudioClip playerShootM2Sfx;
    [SerializeField] private AudioClip levelCompletedSfx;
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip gameOverSfx;
    [SerializeField] private AudioClip enemyShootSfx;
    [SerializeField] private AudioClip buttonClickedAudio;

    [Header("AudioSources")]
    [SerializeField] private AudioSource audioSourceMusic;
    [SerializeField] private AudioSource audioSourceSfx;

    private CarController _carController;
    private int _lastMusicIndex = -1;
    private bool _isEndless;

    private void Awake()
    {
        enemyDiedEvent?.Subscribe(PlayEnemyDiedSfx);
        civilianDiedEvent?.Subscribe(PlayCivilianDiedSfx);
        CarSpawner.OnCarSpawned += HandleCarSpawned;
        UIButton.onButtonClicked += UIButton_onButtonClicked;
    }

    private void Start()
    {
        _isEndless = EndlessModeManager.Instance != null;

        if (_isEndless)
            EndlessModeManager.Instance.OnGameOver += HandleGameOver;
        else
            CompetitionManager.Instance.OnLevelEnded += HandleLevelEnded;

        PlayRandomMusic();
    }

    // Warning: Media - Verificar isPlaying en Update CADA frame es un acceso nativo a Unity. Mejor solución: AudioSource.loop = false + coroutine que espera audioSourceMusic.clip.length y agenda el próximo PlayRandomMusic. Reduce overhead y es más limpio.
    private void Update()
    {
        if (!audioSourceMusic.isPlaying)
            PlayRandomMusic();
    }

    private void OnDestroy()
    {
        CarSpawner.OnCarSpawned -= HandleCarSpawned;

        if (_carController != null)
        {
            _carController.onPlayerDied -= PlayerMovement_playerDied;
            _carController.onPlayerCrashed -= CarController_onPlayerCrashed;
            _carController.onPlayerHurt -= PlayerController_onPlayerHurt;
            _carController.onPlayerShootM1 -= PlayerController_onPlayerShootM1;
            _carController.onPlayerShootM2 -= PlayerController_onPlayerShootM2;
        }

        enemyDiedEvent?.Unsubscribe(PlayEnemyDiedSfx);
        civilianDiedEvent?.Unsubscribe(PlayCivilianDiedSfx);
        UIButton.onButtonClicked -= UIButton_onButtonClicked;

        if (_isEndless)
        {
            if (EndlessModeManager.Instance != null)
                EndlessModeManager.Instance.OnGameOver -= HandleGameOver;
        }
        else
        {
            if (CompetitionManager.Instance != null)
                CompetitionManager.Instance.OnLevelEnded -= HandleLevelEnded;
        }
    }

    private void HandleCarSpawned(GasSystem gas, HealthSystemV2 health)
    {
        _carController = gas.GetComponent<CarController>();

        _carController.onPlayerDied += PlayerMovement_playerDied;
        _carController.onPlayerCrashed += CarController_onPlayerCrashed;
        _carController.onPlayerHurt += PlayerController_onPlayerHurt;
        _carController.onPlayerShootM1 += PlayerController_onPlayerShootM1;
        _carController.onPlayerShootM2 += PlayerController_onPlayerShootM2;
    }

    private void HandleGameOver()
    {
        audioSourceMusic.Stop();
        audioSourceSfx.PlayOneShot(gameOverSfx);
    }

    private void HandleLevelEnded(CompetitionScoreSystem.CompetitionResult result)
    {
        audioSourceSfx.PlayOneShot(result.Passed ? levelCompletedSfx : gameOverSfx);
    }

    // Error: Baja - Método vacío suscrito al evento onPlayerCrashed.
    private void CarController_onPlayerCrashed(float arg1, Transform arg2) { }
    private void PlayEnemyShootSfx() => audioSourceSfx.PlayOneShot(enemyShootSfx);
    private void PlayEnemyDiedSfx() => audioSourceSfx.PlayOneShot(enemyKillledSfx);
    private void PlayCivilianDiedSfx() => audioSourceSfx.PlayOneShot(civilianKillledSfx);
    private void PlayerController_onPlayerHurt() => audioSourceSfx.PlayOneShot(playerHurtSfx);
    private void PlayerController_onPlayerShootM1() => audioSourceSfx.PlayOneShot(playerShootM1Sfx);
    private void PlayerController_onPlayerShootM2() => audioSourceSfx.PlayOneShot(playerShootM2Sfx);
    private void UIButton_onButtonClicked() => audioSourceSfx.PlayOneShot(buttonClickedAudio);
    private void PlayerMovement_playerDied() => audioSourceSfx.PlayOneShot(playerDiedSfx);

    public void ReproduceClip(AudioClip audioClip) => audioSourceSfx.PlayOneShot(audioClip);

    private void PlayRandomMusic()
    {
        if (music.Length == 0) return;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, music.Length);
        }
        while (randomIndex == _lastMusicIndex && music.Length > 1);

        _lastMusicIndex = randomIndex;
        audioSourceMusic.clip = music[randomIndex];
        audioSourceMusic.Play();
    }
}