using System;
using UnityEngine;

public class Mine : MonoBehaviour, IDamageable
{
    [SerializeField] private float mineDamage = 10f;
    public event Action onDie;
    public event Action<float, float> onDamage;
    
    private void OnTriggerEnter(Collider other)
    {
        DoDamage(other.gameObject);
    }

    private void DoDamage(GameObject other)
    {
        IDamageable hs = other.GetComponent<IDamageable>();
        if (hs != null)
        {
            hs.TakeDamage(mineDamage);
        }
        Destroy(gameObject);
    }
    
    public void TakeDamage(float amount)
    {
        Destroy(gameObject);
    }
}
