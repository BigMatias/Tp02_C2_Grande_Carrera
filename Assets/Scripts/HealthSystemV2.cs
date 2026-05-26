using System;
using UnityEngine;

// Suggestion: Alta - El TP especifica que "todos los valores duros del auto (Vida Máxima)" deben venir del CarConfigurationSO. Acá la vida máxima se hardcodea con [SerializeField] int = 100 en cada prefab. Debería inyectarse desde el SO al inicializarse (ResetLife(maxLife) llamado por CarController).
// Suggestion: Media - El campo se llama maxLife y es int; el life interno es float. Inconsistencia.
public class HealthSystemV2 : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxLife = 100;

    public event Action<float, float> onLifeUpdated; // <currentLife, maxLife>
    public event Action onDie;
    public event Action<float, float> onDamage;

    private float life = 100;

    private void Start()
    {
        life = maxLife;
        onLifeUpdated?.Invoke(life, maxLife);
    }

    public void ResetLife()
    {
        life = maxLife;
        onLifeUpdated?.Invoke(life, maxLife);
    }

    // Warning: Baja - "damage < 0" descarta silenciosamente daños negativos; debería al menos loguear un warning 
    public void TakeDamage(float damage)
    {
        if (damage < 0)
        {
            return;
        }

        life -= damage;

        if (life <= 0)
        {
            life = 0;
            onLifeUpdated?.Invoke(life, maxLife);
            onDie?.Invoke();
        }
        else
        {   
            onDamage?.Invoke(life, maxLife);
            onLifeUpdated?.Invoke(life, maxLife);
        }
        // Warning: Baja - Debug.Log con un float pelado (sin etiqueta) en cada TakeDamage. Spam de consola en runtime sin saber de lo que es ese numero.
        Debug.Log(damage);
    }

    public void Heal(float plus)
    {
        if (plus < 0)
        {
            return;
        }

        life += plus;

        if (life > maxLife)
            life = maxLife;

        onLifeUpdated?.Invoke(life, maxLife);
    }
}