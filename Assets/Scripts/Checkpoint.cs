using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Configuración")]
    public int checkpointIndex = 0;

    [Header("Feedback Visual")]
    [SerializeField] private MeshRenderer indicatorRenderer;
    [SerializeField] private Color pendingColor = new Color(1f, 0.6f, 0f, 0.6f);   
    [SerializeField] private Color passedColor = new Color(0.1f, 0.9f, 0.3f, 0.6f);
    [SerializeField] private Color activeColor = new Color(0.2f, 0.6f, 1f, 0.8f);  

    [Header("Spawn Point")]
    public Transform respawnPoint;

    private void Start()
    {
        SetVisualState(CheckpointVisualState.Pending);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != (int)Layers.Player) return;
        CheckpointSystem.Instance?.RegisterCheckpointReached(checkpointIndex);
    }

    public void SetVisualState(CheckpointVisualState state)
    {
        Debug.Log(state);
        if (indicatorRenderer == null) return;
        switch (state)
        {
            case CheckpointVisualState.Pending: indicatorRenderer.material.color = pendingColor; break;
            case CheckpointVisualState.Active: indicatorRenderer.material.color = activeColor; break;
            case CheckpointVisualState.Passed: indicatorRenderer.material.color = passedColor; break;
        }
    }

    public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position;
    public Quaternion RespawnRotation => respawnPoint != null ? respawnPoint.rotation : transform.rotation;
}

public enum CheckpointVisualState { Pending, Active, Passed }