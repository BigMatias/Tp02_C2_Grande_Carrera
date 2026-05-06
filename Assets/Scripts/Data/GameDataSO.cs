using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Data")]

public class GameDataSO : ScriptableObject
{
    [Header("Configs: ")]
    public int EnemyKilledScore;
    public int CivilianKilledScore;
}
