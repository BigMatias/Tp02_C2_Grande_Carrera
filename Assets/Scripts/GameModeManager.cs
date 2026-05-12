using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameModeDataSO gameModeData;
    [SerializeField] private CompetitionManager competitionManager;
    [SerializeField] private EndlessModeManager endlessModeManager;

    private void Awake()
    {
        bool isEndless = gameModeData.selectedMode == GameMode.Endless;
        competitionManager.gameObject.SetActive(!isEndless);
        endlessModeManager.gameObject.SetActive(isEndless);
    }
}