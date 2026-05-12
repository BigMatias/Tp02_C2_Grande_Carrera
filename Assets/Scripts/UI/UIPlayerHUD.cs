using TMPro;
using UnityEngine;

public class UIPlayerHUD : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI speedMeter;
    [SerializeField] private TextMeshProUGUI pointsTxt;
    [SerializeField] private TextMeshProUGUI enemiesLeftTxt;
    [SerializeField] private GameObject levelFinishedCanvas;
    [SerializeField] private CarController carController;

    private void Awake()
    {
        gameManager.onScoreUpdated += GameManager_onScoreUpdated;
    }

    void Update()
    {
        speedMeter.text = carController.CurrentSpeed().ToString("0");
    }

    private void OnDestroy()
    {
        gameManager.onScoreUpdated -= GameManager_onScoreUpdated;
    }
    private void GameManager_onScoreUpdated(int currentScore)
    {
        pointsTxt.text = currentScore.ToString();
    }

    private void LevelManager_onLevelFinished()
    {
        //gameDataSO.CurrentScore = totalPoints;
        levelFinishedCanvas.SetActive(true);
    }

}
