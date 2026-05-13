using UnityEngine;
using UnityEngine.UI;

public class UILife : MonoBehaviour
{
    [SerializeField] private Image barLife;

    private HealthSystemV2 _target;

    private void Awake()
    {
        CarSpawner.OnCarSpawned += HandleCarSpawned;
    }

    private void OnDestroy()
    {
        CarSpawner.OnCarSpawned -= HandleCarSpawned;

        if (_target != null)
        {
            _target.onLifeUpdated -= HealthSystem_onLifeUpdated;
            _target.onDie -= HealthSystem_onDie;
        }
    }

    private void HandleCarSpawned(GasSystem gas, HealthSystemV2 health)
    {
        _target = health;
        _target.onLifeUpdated += HealthSystem_onLifeUpdated;
        _target.onDie += HealthSystem_onDie;
        barLife.fillAmount = 1f;
    }

    private void HealthSystem_onLifeUpdated(float current, float max)
    {
        barLife.fillAmount = current / max;
    }

    private void HealthSystem_onDie()
    {
        barLife.fillAmount = 0;
    }
}