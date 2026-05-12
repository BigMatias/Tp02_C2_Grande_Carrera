using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private float mineDamage = 10f;
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
}
