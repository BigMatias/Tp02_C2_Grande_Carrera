using System;
using UnityEngine;

public class GasSystem : MonoBehaviour
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;

    public event Action<float, float> onGasUpdated; // <currentLife, maxLife>
    public event Action onGasDepleted;
    public event Action onGasConsumed;

    private float gas;

    private void Start()
    {
        gas = carConfigurationSO.TotalGas;
        onGasUpdated?.Invoke(gas, carConfigurationSO.TotalGas);
    }

    public void ResetGas()
    {
        gas = carConfigurationSO.TotalGas;
        onGasUpdated?.Invoke(gas, carConfigurationSO.TotalGas);
    }

    public void ConsumeGas(float gas)
    {

        if (gas < 0)
        {
            return;
        }

        this.gas -= gas;

        if (this.gas <= 0)
        {
            this.gas = 0;
            onGasUpdated?.Invoke(this.gas, carConfigurationSO.TotalGas);
            onGasDepleted?.Invoke();
        }
        else
        {
            onGasConsumed?.Invoke();
            onGasUpdated?.Invoke(this.gas, carConfigurationSO.TotalGas);
        }
    }

    public void RecoverGas(float plus)
    {
        if (plus < 0)
        {
            return;
        }

        gas += plus;

        if (gas > carConfigurationSO.TotalGas)
            gas = carConfigurationSO.TotalGas;

        onGasUpdated?.Invoke(gas, carConfigurationSO.TotalGas);
    }
}