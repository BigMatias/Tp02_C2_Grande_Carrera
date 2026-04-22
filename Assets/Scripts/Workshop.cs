using System;
using UnityEngine;

public class Workshop : MonoBehaviour
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;
    public static event Action<float> onWorkshopEntered;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == (int)Layers.Player)
        {
            onWorkshopEntered?.Invoke(carConfigurationSO.HealthRecoveredBySecond * Time.deltaTime);
        }
    }
}
