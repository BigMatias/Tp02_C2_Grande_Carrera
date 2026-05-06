using UnityEngine;
using TMPro;
using System.Collections;

public class CompetitionHUDMessages : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup messageCanvasGroup;

    [Header("Colores")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.2f);

    [Header("Animación")]
    [SerializeField] private float fadeInTime = 0.2f;
    [SerializeField] private float fadeOutTime = 0.4f;

    private Coroutine _currentMessage;

    private void Awake()
    {
        if (messageCanvasGroup == null && messageText != null)
            messageCanvasGroup = messageText.GetComponent<CanvasGroup>()
                              ?? messageText.gameObject.AddComponent<CanvasGroup>();

        if (messageCanvasGroup != null) messageCanvasGroup.alpha = 0f;
    }

    /// <param name="text">Texto a mostrar.</param>
    /// <param name="duration">Segundos visibles antes de desvanecerse.</param>
    /// <param name="isWarning">Si es true, muestra en color de advertencia (rojo).</param>
    public void ShowMessage(string text, float duration, bool isWarning = false)
    {
        if (_currentMessage != null) StopCoroutine(_currentMessage);
        _currentMessage = StartCoroutine(DisplayMessage(text, duration, isWarning));
    }

    private IEnumerator DisplayMessage(string text, float duration, bool isWarning)
    {
        if (messageText == null) yield break;

        messageText.text = text;
        messageText.color = isWarning ? warningColor : normalColor;

        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            if (messageCanvasGroup) messageCanvasGroup.alpha = t / fadeInTime;
            yield return null;
        }
        if (messageCanvasGroup) messageCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(duration);

        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            if (messageCanvasGroup) messageCanvasGroup.alpha = 1f - (t / fadeOutTime);
            yield return null;
        }
        if (messageCanvasGroup) messageCanvasGroup.alpha = 0f;
    }
}