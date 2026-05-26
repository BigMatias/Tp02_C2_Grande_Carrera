using System;
using UnityEngine;

// Error: Alta - Esta clase está DUPLICADA con HealthSystemV2 y NO se usa en ningún script del proyecto 
public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxLife = 100;

    public event Action<float, float> onLifeUpdated; // <currentLife, maxLife>
    public event Action onDie;
    public event Action onDamageDealt;

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

    public void DoDamage(float damage)
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
            onDamageDealt?.Invoke();
            onLifeUpdated?.Invoke(life, maxLife);
        }
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
