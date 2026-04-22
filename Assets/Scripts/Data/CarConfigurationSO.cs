using UnityEngine;

[CreateAssetMenu(fileName = "CarSettings", menuName = "Car/Data")]

public class CarConfigurationSO : ScriptableObject
{
    [Header("Stats: ")]
    public float TotalGas;
    public float CurrentGas;
    [Header("General Configs")]
    public float MotorForce;
    public float DirectionForce;
    public float BreakForce;
    public float GasConsumedBySecond;
    [Header("Damage Depending on Speed")]
    public float FirstSpeedThreshold;
    public float SecondSpeedThreshold;
    public float ThirdSpeedThreshold;
    public float CrashDamage1;
    public float CrashDamage2;
    public float CrashDamage3;

}
