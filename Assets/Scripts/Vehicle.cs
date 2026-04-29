using System;
using UnityEngine;

public class Vehicle : MonoBehaviour, IDamageable
{
    [SerializeField] private CarConfigurationSO carConfigurationSO;

    public event Action onDie;
    public event Action<float, float> onDamage;
    private float life;

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
            onDie?.Invoke();
        }
        else
        {
            onDamage?.Invoke(life, carConfigurationSO.MaxLife);
        }
        Debug.Log(damage);
    }

}