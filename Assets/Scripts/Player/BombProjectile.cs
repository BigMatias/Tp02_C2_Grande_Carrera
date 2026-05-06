using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombProjectile : MonoBehaviour, IPooleable
{
    [Header("Explosión")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 100f;
    [SerializeField] private LayerMask hitLayers;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public bool IsActive => gameObject.activeSelf;

    public void Activate()
    {
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Launch(Vector3 initialVelocity)
    {
        rb.linearVelocity = initialVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != (int)Layers.Player)
            Explode();
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);

        foreach (Collider c in colliders)
        {
            IDamageable target = c.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(explosionDamage);
            }
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Return(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}