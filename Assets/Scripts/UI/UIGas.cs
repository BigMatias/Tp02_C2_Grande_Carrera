using UnityEngine;
using UnityEngine.UI;

public class UIGas: MonoBehaviour
{
    [SerializeField] private GasSystem target;
    [SerializeField] private Image barGas;

    private void Awake()
    {
        target.onGasUpdated += Target_onGasUpdated;
        target.onGasDepleted += Target_onGasDepleted;
    }

    private void Start()
    {
        barGas.fillAmount = 100;
    }

    private void OnDestroy()
    {
        target.onGasUpdated -= Target_onGasUpdated;
        target.onGasDepleted -= Target_onGasDepleted;
    }

    private void Target_onGasDepleted()
    {
        barGas.fillAmount = 0;
    }

    private void Target_onGasUpdated(float current, float max)
    {
        float lerp = current / (float)max;
        barGas.fillAmount = lerp;
    }

}