using UnityEngine;

[CreateAssetMenu(fileName = "CompetitionLevelConfig", menuName = "Competition/Level Config")]
public class CompetitionLevelConfigSO : ScriptableObject
{
    [Header("Identificación del Nivel")]
    public string levelName = "Carrera 1";
    public int levelIndex = 0;

    [Header("Escena")]
    [Tooltip("Nombre exacto de la escena de Unity para este nivel")]
    public string sceneName = "Track_01";

    [Header("Objetivo de Puntaje")]
    [Tooltip("Puntos requeridos para pasar este nivel")]
    public int requiredScore = 1000;

    [Header("Bonus de Tiempo")]
    [Tooltip("Tiempo límite del circuito en segundos. Completar antes da bonus.")]
    public float lapTimeLimit = 120f;
    [Tooltip("Puntos bonus máximos por completar con tiempo restante")]
    public int maxTimeBonusPoints = 500;
    [Tooltip("Multiplicador de bonus: puntosBonus = tiempoRestante * este valor")]
    public float timeBonusMultiplier = 5f;

    [Header("Dificultad de Enemigos")]
    [Tooltip("Velocidad base de los enemigos en este nivel")]
    public float enemyBaseSpeed = 10f;
    [Tooltip("Cantidad máxima de enemigos activos en escena (Object Pool)")]
    public int maxActiveEnemies = 5;
    [Tooltip("Cantidad máxima de civiles activos en escena (Object Pool)")]
    public int maxActiveCivilians = 8;

    [Header("Tráfico General")]
    public float trafficDensityMultiplier = 1f;

    public int CalculateTimeBonus(float timeRemaining)
    {
        if (timeRemaining <= 0f) return 0;
        float ratio = Mathf.Clamp01(timeRemaining / lapTimeLimit);
        return Mathf.RoundToInt(timeRemaining * timeBonusMultiplier * ratio);
    }
}