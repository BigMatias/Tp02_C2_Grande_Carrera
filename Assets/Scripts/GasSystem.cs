using System;
using UnityEngine;

public class GasSystem : MonoBehaviour
{
    [SerializeField] private float maxGas = 100;

    public event Action<float, float> onGasUpdated; // <currentLife, maxLife>
    public event Action onGasDepleted;
    public event Action onGasConsumed;

    private float gas = 100;

    private void Start()
    {
        gas = maxGas;
        onGasUpdated?.Invoke(gas, maxGas);
    }

    public void ResetGas()
    {
        gas = maxGas;
        onGasUpdated?.Invoke(gas, maxGas);
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
            onGasUpdated?.Invoke(this.gas, maxGas);
            onGasDepleted?.Invoke();
        }
        else
        {
            onGasConsumed?.Invoke();
            onGasUpdated?.Invoke(this.gas, maxGas);
        }
        Debug.Log(gas);

    }

    public void RecoverGas(float plus)
    {
        Debug.Log(plus);
        if (plus < 0)
        {
            return;
        }

        gas += plus;

        if (gas > maxGas)
            gas = maxGas;

        onGasUpdated?.Invoke(gas, maxGas);
    }
}