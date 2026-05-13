using System.Collections;
using TMPro;
using UnityEngine;

public class UIDamageCanvas : MonoBehaviour
{
    [SerializeField] private Canvas canvasDamage;
    [SerializeField] private TMP_Text textDamage;

    private CarController _carController;
    private Coroutine _textActiveCoroutine;

    private void Awake()
    {
        CarSpawner.OnCarSpawned += HandleCarSpawned;
        canvasDamage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        CarSpawner.OnCarSpawned -= HandleCarSpawned;

        if (_carController != null)
            _carController.onPlayerCrashed -= CarController_onPlayerCrashed;

        if (_textActiveCoroutine != null)
            StopCoroutine(_textActiveCoroutine);
    }

    private void HandleCarSpawned(GasSystem gas, HealthSystemV2 health)
    {
        _carController = gas.GetComponent<CarController>();
        _carController.onPlayerCrashed += CarController_onPlayerCrashed;
    }

    private void CarController_onPlayerCrashed(float damage, Transform healthPoint)
    {
        if (_textActiveCoroutine != null)
            StopCoroutine(_textActiveCoroutine);

        canvasDamage.gameObject.SetActive(true);
        textDamage.transform.position = healthPoint.position;
        textDamage.text = damage.ToString();
        textDamage.color = Color.red;

        _textActiveCoroutine = StartCoroutine(TextActive());
    }

    private IEnumerator TextActive()
    {
        yield return new WaitForSeconds(1);
        canvasDamage.gameObject.SetActive(false);
    }
}