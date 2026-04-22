using TMPro;
using UnityEngine;

public class UIPlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedMeterTxt;
    [SerializeField] private TextMeshProUGUI pointsTxt;
    [SerializeField] private TextMeshProUGUI enemiesLeftTxt;
    [SerializeField] private CarController carController;


    private void Awake()
    {
        
    }

    void Start()
    {
    }

    void Update()
    {
        speedMeterTxt.text = carController.CurrentSpeed().ToString("0");
    }

    private void OnDestroy()
    {
    }

}
