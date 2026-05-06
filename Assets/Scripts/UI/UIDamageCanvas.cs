using System.Collections;
using TMPro;
using UnityEngine;

public class UIDamageCanvas : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private Canvas canvasDamage;
    [SerializeField] private TMP_Text textDamage;

    private Coroutine textActiveCoroutine;

    private void Awake()
    {
        carController.onPlayerCrashed += CarController_onPlayerCrashed;
    }

    private void OnDestroy()
    {
        if (textActiveCoroutine != null)
            StopCoroutine(textActiveCoroutine); 
    }

    private void CarController_onPlayerCrashed(float damage, Transform healthPoint)
    {
        canvasDamage.gameObject.SetActive(true);
        textDamage.transform.position = healthPoint.position;
        textDamage.text = damage.ToString();
        textDamage.color = Color.red;

        textActiveCoroutine = StartCoroutine(TextActive());
    }

    private IEnumerator TextActive()
    {
        yield return new WaitForSeconds(1);
        canvasDamage.gameObject.SetActive(false);
    }
}
