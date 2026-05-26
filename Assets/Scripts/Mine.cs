using System;
using UnityEngine;

// Warning: Alta - Mine NO se devuelve a un Pool. Es Destroy(gameObject)
// Error: Media - Implementa IDamageable pero los eventos onDie y onDamage NUNCA se invocan en TakeDamage.
public class Mine : MonoBehaviour, IDamageable
{
    [SerializeField] private float mineDamage = 10f;
    public event Action onDie;
    public event Action<float, float> onDamage;
    
    // Bug: Media - OnTriggerEnter detona la mina contra CUALQUIER cosa que toque (incluyendo civiles que pasen, balas, otros NPCs). El TP habla de "minas explosivas que NOS dañen" — la mina sólo debería activarse al contacto con el player.
    // Warning: Baja - Sin radio de explosión (no usa OverlapSphere como BombProjectile); el daño es punto-a-punto. Para una "mina explosiva" se esperaba área de efecto.
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
