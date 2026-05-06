using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Wrench : MonoBehaviour, IPooleable
{
    [SerializeField] private float wrenchDamage = 20f;
    private Rigidbody rb;

    private void Awake() => rb = GetComponent<Rigidbody>();

    public bool IsActive => gameObject.activeSelf;

    public void Activate()
    {
        gameObject.SetActive(true);
        rb.linearVelocity = rb.angularVelocity = Vector3.zero;
    }

    public void Deactivate() => gameObject.SetActive(false);

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (int)Layers.Player)
        {
            other.GetComponentInParent<IDamageable>()?.TakeDamage(wrenchDamage);
            ReturnToPool();
        }
        else if (other.gameObject.layer == (int)Layers.Obstacles) ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null) PoolManager.Instance.Return(this);
        else Destroy(gameObject);
    }
}