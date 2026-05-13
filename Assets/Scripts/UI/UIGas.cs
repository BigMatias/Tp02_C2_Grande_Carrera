using UnityEngine;
using UnityEngine.UI;

public class UIGas : MonoBehaviour
{
    [SerializeField] private Image barGas;

    private GasSystem _target;

    private void Awake()
    {
        CarSpawner.OnCarSpawned += HandleCarSpawned;
    }

    private void OnDestroy()
    {
        CarSpawner.OnCarSpawned -= HandleCarSpawned;

        if (_target != null)
        {
            _target.onGasUpdated -= Target_onGasUpdated;
            _target.onGasDepleted -= Target_onGasDepleted;
        }
    }

    private void HandleCarSpawned(GasSystem gas, HealthSystemV2 health)
    {
        _target = gas;
        _target.onGasUpdated += Target_onGasUpdated;
        _target.onGasDepleted += Target_onGasDepleted;
        barGas.fillAmount = 1f;
    }

    private void Target_onGasDepleted()
    {
        barGas.fillAmount = 0;
    }

    private void Target_onGasUpdated(float current, float max)
    {
        barGas.fillAmount = current / max;
    }
}