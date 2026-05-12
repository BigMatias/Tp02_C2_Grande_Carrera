using UnityEngine;

[CreateAssetMenu(fileName = "TurretSettings", menuName = "Turret/Data")]

public class TurretDataSO : ScriptableObject
{
    [Header("Configs: ")]
    public float M1Damage;
    public float M1ShootRange;
    public float M2Damage;
    public float LaserDuration;
}
