using TMPro;
using UnityEngine;

public class UIPlayerHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedMeterTxt;
    [SerializeField] private CarController carController;


    void Update()
    {
        speedMeterTxt.text = carController.CurrentSpeed().ToString("0");
    }


}
