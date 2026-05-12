using Mono.Cecil.Cil;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private FsmManager fsmManager;
    [SerializeField] private GameEventSO enemyShootEvent;
    [SerializeField] private GameEventSO enemyDiedEvent; 
    [SerializeField] private GameEventSO civilianDiedEvent;

    [Header("AudioClips")]
    [SerializeField] private AudioClip[] music;
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

    private int lastMusicIndex = -1;

    private void Awake()
    {
        enemyShootEvent?.Subscribe(PlayEnemyShootSfx);
        enemyDiedEvent?.Subscribe(PlayEnemyDiedSfx);
        civilianDiedEvent?.Subscribe(PlayCivilianDiedSfx);

        //LevelManager.onLevelFinished += LevelManager_onLevelFinished;
        //LevelManager.onGameOver += LevelManager_onGameOver;
        if (carController != null)
        {
            carController.onPlayerDied += PlayerMovement_playerDied;
            carController.onPlayerCrashed += CarController_onPlayerCrashed;
            carController.onPlayerHurt += PlayerController_onPlayerHurt;
            carController.onPlayerShootM1 += PlayerController_onPlayerShootM1;
            carController.onPlayerShootM2 += PlayerController_onPlayerShootM2;
        }

        UIButton.onButtonClicked += UIButton_onButtonClicked;

    }

    private void Start()
    {
        PlayRandomMusic();
    }

    private void Update()
    {
        if (!audioSourceMusic.isPlaying)
        {
            PlayRandomMusic();
        }
    }
    private void OnDestroy()
    {
        if (carController != null)
        {
            carController.onPlayerDied -= PlayerMovement_playerDied;
            carController.onPlayerHurt -= PlayerController_onPlayerHurt;
            carController.onPlayerShootM1 -= PlayerController_onPlayerShootM1;
            carController.onPlayerShootM2 -= PlayerController_onPlayerShootM2;
        }

        enemyShootEvent?.Unsubscribe(PlayEnemyShootSfx);
        enemyDiedEvent?.Unsubscribe(PlayEnemyDiedSfx);
        civilianDiedEvent?.Unsubscribe(PlayCivilianDiedSfx);

        UIButton.onButtonClicked -= UIButton_onButtonClicked;
    }

    private void CarController_onPlayerCrashed(float arg1, Transform arg2)
    {
        throw new System.NotImplementedException();
    }

    private void LevelManager_onGameOver(bool won)
    {
        audioSourceMusic.Stop();
        if (won)
        {
            audioSourceSfx.PlayOneShot(victorySfx);
        }
        else
        {
            audioSourceSfx.PlayOneShot(gameOverSfx);
        }
    }

    private void PlayEnemyShootSfx()
    {
        audioSourceSfx.PlayOneShot(enemyShootSfx);
    }
    private void PlayEnemyDiedSfx()
    {
        audioSourceSfx.PlayOneShot(enemyKillledSfx);
    }

    private void PlayCivilianDiedSfx()
    {
        audioSourceSfx.PlayOneShot(civilianKillledSfx);
    }

    private void LevelManager_onLevelFinished()
    {
        audioSourceSfx.PlayOneShot(levelCompletedSfx);
    }

    private void PlayerController_onPlayerHurt()
    {
        audioSourceSfx.PlayOneShot(playerHurtSfx);
    }

    private void PlayerController_onPlayerShootM2()
    {
        audioSourceSfx.PlayOneShot(playerShootM2Sfx);
    }

    private void PlayerController_onPlayerShootM1()
    {
        audioSourceSfx.PlayOneShot(playerShootM1Sfx);
    }

    private void UIButton_onButtonClicked()
    {
        audioSourceSfx.PlayOneShot(buttonClickedAudio);
    }

    private void PlayerMovement_playerDied()
    {
        audioSourceSfx.PlayOneShot(playerDiedSfx);
    }

    private void FsmManager_onCivilianDied()
    {
        audioSourceSfx.PlayOneShot(civilianKillledSfx);
    }

    public void ReproduceClip(AudioClip audioClip)
    {
        audioSourceSfx.PlayOneShot(audioClip);
    }
    private void PlayRandomMusic()
    {
        if (music.Length == 0) return;

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, music.Length);
        }
        while (randomIndex == lastMusicIndex && music.Length > 1);

        lastMusicIndex = randomIndex;

        audioSourceMusic.clip = music[randomIndex];
        audioSourceMusic.Play();
    }
}