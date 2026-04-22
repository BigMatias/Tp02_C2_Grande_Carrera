using UnityEngine;
using UnityEngine.UI;

public class UIGas: MonoBehaviour
{
    [SerializeField] private GasSystem target;
    [SerializeField] private Image barLife;

    private void Awake()
    {
        target.onGasUpdated += HealthSystem_onLifeUpdated;
        target.onGasDepleted += HealthSystem_onDie;
    }

    private void Start()
    {
        barLife.fillAmount = 100;
    }

    private void OnDestroy()
    {
        target.onGasUpdated -= HealthSystem_onLifeUpdated;
        target.onGasDepleted -= HealthSystem_onDie;
    }

    public void HealthSystem_onLifeUpdated(float current, float max)
    {
        float lerp = current / (float)max;
        barLife.fillAmount = lerp;
    }

    private void HealthSystem_onDie()
    {
        barLife.fillAmount = 0;
    }
}