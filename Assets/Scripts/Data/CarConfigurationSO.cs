using UnityEngine;

[CreateAssetMenu(fileName = "CarSettings", menuName = "Car/Data")]

public class CarConfigurationSO : ScriptableObject
{
    [Header("Stats: ")]
    public float TotalGas;
    public float CurrentGas;
    [Header("General Configs")]
    public float MouseSens;
    public float ThirdPersonCameraDistance;
    public float MotorForce;
    public float DirectionForce;
    public float BreakForce;
    public float GasConsumedBySecond;
    public float GasRecoveredBySecond;
    public float HealthRecoveredBySecond;
    [Header("Damage Depending on Speed")]
    public float FirstSpeedThreshold;
    public float SecondSpeedThreshold;
    public float ThirdSpeedThreshold;
    public float CrashDamage1;
    public float CrashDamage2;
    public float CrashDamage3;

}
