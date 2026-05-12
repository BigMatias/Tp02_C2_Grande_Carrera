using UnityEngine;

[CreateAssetMenu(fileName = "CitizenSettings", menuName = "Citizen/Data")]

public class CitizenDataSO : ScriptableObject
{
    [Header("General Configs")]
    public float Speed;
    public float MaxSpeed;
    [Header("Enemy Settings")]
    public float EnemyProjectileDamage;
    public float EnemyProjectileSpeed;
    public float EnemyProjectileDuration;
    public float EnemyThrowCD;
    public float ProjectileInstantiateQuantity;
    public float EnemySpawnQuantity;
    [Header("Civilian Settings")]
    public float CivilianSpawnQuantity;

}
