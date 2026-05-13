using System;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private CarSelectionDataSO carSelectionData;
    [SerializeField] private GameObject[] cars;

    public static event Action<GasSystem, HealthSystemV2> OnCarSpawned;

    private GasSystem _spawnedGas;
    private HealthSystemV2 _spawnedHealth;

    private void Awake()
    {
        foreach (GameObject car in cars)
            car.SetActive(false);

        int index = carSelectionData.selectedCarIndex;
        if (index >= 0 && index < cars.Length)
        {
            cars[index].SetActive(true);
            _spawnedGas = cars[index].GetComponent<GasSystem>();
            _spawnedHealth = cars[index].GetComponent<HealthSystemV2>();
        }
    }

    private void Start()
    {
        OnCarSpawned?.Invoke(_spawnedGas, _spawnedHealth);
    }
}