using System;
using UnityEngine;

public class HealthSystemV2 : MonoBehaviour, IDamageable
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;

    public event Action<float, float> onLifeUpdated; // <currentLife, maxLife>
    public event Action onDie;
    public event Action<float, float> onDamage;

    private float life = 100;

    private void Start()
    {
        life = carConfigurationSO.MaxLife;
        onLifeUpdated?.Invoke(life, carConfigurationSO.MaxLife);
    }

    public void ResetLife()
    {
        life = carConfigurationSO.MaxLife;
        onLifeUpdated?.Invoke(life, carConfigurationSO.MaxLife);
    }

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
            onLifeUpdated?.Invoke(life, carConfigurationSO.MaxLife);
            onDie?.Invoke();
        }
        else
        {
            onDamage?.Invoke(life, carConfigurationSO.MaxLife);
            onLifeUpdated?.Invoke(life, carConfigurationSO.MaxLife);
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

        if (life > carConfigurationSO.MaxLife)
            life = carConfigurationSO.MaxLife;

        onLifeUpdated?.Invoke(life, carConfigurationSO.MaxLife);
    }
}