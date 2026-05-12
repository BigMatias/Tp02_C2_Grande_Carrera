using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasStation : MonoBehaviour
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;
    private List<GasSystem> carsInsideStation = new List<GasSystem>();

    private IEnumerator rechargingGasCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        GasSystem carController = other.GetComponent<GasSystem>();
        if (carController != null)
        {
            carsInsideStation.Add(carController);
        }

        if (rechargingGasCoroutine == null)
        {
            rechargingGasCoroutine = RechargingGas();
            StartCoroutine(rechargingGasCoroutine);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GasSystem carController = other.GetComponent<GasSystem>();
        if (carController != null)
        {
            carsInsideStation.Remove(carController);
        }
    }

    private IEnumerator RechargingGas()
    {
        while(carsInsideStation.Count > 0)
        {
            foreach (GasSystem car in carsInsideStation)
            {
                car.RecoverGas(carConfigurationSO.GasRecoveredBySecond * Time.deltaTime);
            }
            yield return null;
        }
        rechargingGasCoroutine = null;
    }
}
