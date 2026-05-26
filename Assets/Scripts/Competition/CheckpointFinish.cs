using UnityEngine;

public class CheckpointFinish : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CheckpointSystem checkpointSystem;

    [Header("Feedback Visual / Sonoro")]
    [SerializeField] private GameObject finishLineVFX;
    [SerializeField] private AudioSource finishSound;

    [Header("Visual de estado")]
    [Tooltip("Renderer cuyo color cambia seg�n si la meta est� habilitada o no.")]
    [SerializeField] private MeshRenderer finishLineRenderer;
    [SerializeField] private Color lockedColor = new Color(0.9f, 0.2f, 0.1f, 0.7f);
    [SerializeField] private Color unlockedColor = new Color(0.1f, 0.9f, 0.3f, 0.7f);

    private bool _triggered = false;

    private void Start()
    {
        CheckpointSystem.Instance.OnAllCheckpointsPassed += HandleAllCheckpointsPassed;
        CheckpointSystem.Instance.OnInvalidFinishAttempt += HandleInvalidAttempt;

        SetFinishLineColor(lockedColor);
    }
    // Bug: Alta - Si CheckpointSystem.Instance fue destruido antes (cambio de escena, cierre de juego), el desuscribirse acá lanza NRE. Validar Instance != null antes de "-=".
    // Warning: Baja - Suscripción se hace en Start y desuscripción en OnDisable. Es asimétrico.
    private void OnDisable()
    {
        CheckpointSystem.Instance.OnAllCheckpointsPassed -= HandleAllCheckpointsPassed;
        CheckpointSystem.Instance.OnInvalidFinishAttempt -= HandleInvalidAttempt;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (other.gameObject.layer != (int)Layers.Player) return;

        bool canFinish = CheckpointSystem.Instance != null
            ? CheckpointSystem.Instance.TryFinish()
            : true; 

        if (!canFinish) return;

        _triggered = true;

        if (finishLineVFX) finishLineVFX.SetActive(true);
        if (finishSound) finishSound.Play();

        if (CompetitionManager.Instance != null)
        {
            CompetitionManager.Instance.OnPlayerFinishedLap();
            Debug.Log("[CheckpointFinish] Meta cruzada v�lidamente.");
        }
        else
        {
            Debug.LogWarning("[CheckpointFinish] CompetitionManager no encontrado.");
        }
    }

    private void HandleAllCheckpointsPassed()
    {
        SetFinishLineColor(unlockedColor);
        Debug.Log("[CheckpointFinish] Meta desbloqueada.");
    }

    private void HandleInvalidAttempt()
    {
        StartCoroutine(FlashLockedColor());
    }

    public void ResetTrigger()
    {
        _triggered = false;
        SetFinishLineColor(lockedColor);
        if (finishLineVFX) finishLineVFX.SetActive(false);
    }

    private void SetFinishLineColor(Color color)
    {
        if (finishLineRenderer != null)
            finishLineRenderer.material.color = color;
    }

    private System.Collections.IEnumerator FlashLockedColor()
    {
        Color flashColor = Color.white;
        SetFinishLineColor(flashColor);
        yield return new WaitForSeconds(0.15f);
        SetFinishLineColor(lockedColor);
        yield return new WaitForSeconds(0.15f);
        SetFinishLineColor(flashColor);
        yield return new WaitForSeconds(0.15f);
        SetFinishLineColor(lockedColor);
    }
}