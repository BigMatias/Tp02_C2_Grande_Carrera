using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workshop : MonoBehaviour
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;
    private List<HealthSystemV2> carsInsideWorkshop = new List<HealthSystemV2>();

    private IEnumerator repairingCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        HealthSystemV2 healthSystem = other.GetComponent<HealthSystemV2>();

        if (healthSystem != null)
        {
            if (!carsInsideWorkshop.Contains(healthSystem))
            {
                carsInsideWorkshop.Add(healthSystem);
            }

            if (repairingCoroutine == null)
            {
                repairingCoroutine = RepairingCars();
                StartCoroutine(repairingCoroutine);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HealthSystemV2 healthSystem = other.GetComponent<HealthSystemV2>();

        if (healthSystem != null)
        {
            carsInsideWorkshop.Remove(healthSystem);
        }
    }

    private IEnumerator RepairingCars()
    {
        while (carsInsideWorkshop.Count > 0)
        {
            foreach (HealthSystemV2 car in carsInsideWorkshop)
            {
                car.Heal(carConfigurationSO.HealthRecoveredBySecond * Time.deltaTime);
            }
            yield return null;
        }
        repairingCoroutine = null;
    }
}