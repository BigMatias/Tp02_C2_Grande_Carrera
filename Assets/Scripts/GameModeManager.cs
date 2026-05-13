using UnityEngine;

public class GameModeBootstrapper : MonoBehaviour
{
    [SerializeField] private GameModeDataSO gameModeData;
    [SerializeField] private CarSelectionDataSO carSelectionData;
    [SerializeField] private TrackDataSO[] tracks;
    [SerializeField] private CompetitionManager competitionManager;
    [SerializeField] private EndlessModeManager endlessModeManager;

    private void Awake()
    {
        bool isEndless = gameModeData.selectedMode == GameMode.Endless;
        competitionManager.gameObject.SetActive(!isEndless);
        endlessModeManager.gameObject.SetActive(isEndless);

        if (isEndless)
        {
            endlessModeManager.SetInitialTrack(carSelectionData.selectedTrackIndex);
        }
    }
}