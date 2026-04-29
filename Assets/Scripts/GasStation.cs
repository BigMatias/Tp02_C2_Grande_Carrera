using System;
using UnityEngine;

public class GasStation : MonoBehaviour
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;
    public event Action<float> onGasStationEntered;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == (int)Layers.Player)
        {
            onGasStationEntered?.Invoke(carConfigurationSO.GasRecoveredBySecond * Time.deltaTime);
        }
    }

}
