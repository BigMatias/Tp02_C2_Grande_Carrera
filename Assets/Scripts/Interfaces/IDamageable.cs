using System;

public interface IDamageable
{
    event Action onDie;
    event Action<float, float> onDamage;
    void TakeDamage(float amount);
}
