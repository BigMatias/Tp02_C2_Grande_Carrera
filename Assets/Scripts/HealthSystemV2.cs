using System;
using UnityEngine;

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