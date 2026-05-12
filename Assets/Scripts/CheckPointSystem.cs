using UnityEngine;
using System.Collections.Generic;
using System;

public class CheckpointSystem : MonoBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    public event Action<int> OnCheckpointReached;

    public event Action OnInvalidFinishAttempt;

    public event Action OnAllCheckpointsPassed;

    public event Action<int> OnPlayerRespawned;


    [Header("Checkpoints de la pista")]
    [Tooltip("Arrastrar todos los Checkpoint en ORDEN. Si se deja vacío, se buscan por tag.")]
    [SerializeField] private Checkpoint[] checkpoints;

    [Tooltip("Tag usado para buscar checkpoints automáticamente si el array está vacío.")]
    [SerializeField] private string checkpointTag = "Checkpoint";

    [Header("Jugador")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Rigidbody del vehículo, para detener la inercia al respawn.")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Respawn")]
    [SerializeField] private KeyCode respawnKey = KeyCode.R;
    [Tooltip("Segundos de invulnerabilidad/freeze tras el respawn (para evitar triggers dobles).")]
    [SerializeField] private float respawnFreezeDuration = 0.5f;

    [Header("HUD Feedback")]
    [Tooltip("Referencia al HUD para mostrar mensajes. Puede ser nulo.")]
    [SerializeField] private CompetitionHUDMessages hudMessages;

    private HashSet<int> _passedIndices = new HashSet<int>();
    private int _lastCheckpointIndex = -1;      
    private int _nextExpectedIndex = 0;
    private bool _allPassed = false;
    private bool _respawnFrozen = false;
    private float _respawnFreezeTimer = 0f;
    public bool CanFinish => _allPassed;
    public int TotalCheckpoints => checkpoints != null ? checkpoints.Length : 0;
    public int PassedCount => _passedIndices.Count;
    public int LastCheckpointIndex => _lastCheckpointIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        InitializeCheckpoints();
        FindPlayerIfNeeded();
    }

    private void Update()
    {
        HandleRespawnFreeze();
        HandleRespawnInput();
    }

    private void InitializeCheckpoints()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            GameObject[] tagged = GameObject.FindGameObjectsWithTag(checkpointTag);
            checkpoints = new Checkpoint[tagged.Length];
            for (int i = 0; i < tagged.Length; i++)
                checkpoints[i] = tagged[i].GetComponent<Checkpoint>();

            System.Array.Sort(checkpoints, (a, b) => a.checkpointIndex.CompareTo(b.checkpointIndex));
        }

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].checkpointIndex != i)
                Debug.LogWarning($"[CheckpointSystem] El checkpoint en posición {i} tiene índice " +
                                 $"{checkpoints[i].checkpointIndex}. Los índices deben ser 0, 1, 2...");
        }

        UpdateCheckpointVisuals();

        Debug.Log($"[CheckpointSystem] {checkpoints.Length} checkpoints registrados.");
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (playerRigidbody == null)
                playerRigidbody = player.GetComponent<Rigidbody>()
                               ?? player.GetComponentInChildren<Rigidbody>();
        }
        else
        {
            Debug.LogWarning("[CheckpointSystem] No se encontró un GameObject con tag 'Player'.");
        }
    }

    public void ResetSystem()
    {
        _passedIndices.Clear();
        _lastCheckpointIndex = -1;
        _nextExpectedIndex = 0;
        _allPassed = false;
        _respawnFrozen = false;
        UpdateCheckpointVisuals();
        Debug.Log("[CheckpointSystem] Sistema reiniciado.");
    }

    public void RegisterCheckpointReached(int index)
    {
        if (_respawnFrozen) return;
        if (_passedIndices.Contains(index)) return; 

        if (index != _nextExpectedIndex)
        {
            Debug.Log($"[CheckpointSystem] Checkpoint {index} ignorado. Se esperaba el {_nextExpectedIndex}.");
            if (hudMessages != null)
                hudMessages.ShowMessage($"¡You went through checkpoint {index}! You must go through {_nextExpectedIndex} first.", 2f);
            return;
        }

        _passedIndices.Add(index);
        _lastCheckpointIndex = index;
        _nextExpectedIndex = index + 1;

        OnCheckpointReached?.Invoke(index);
        UpdateCheckpointVisuals();

        Debug.Log($"[CheckpointSystem]  Checkpoint {index} reached ({_passedIndices.Count}/{checkpoints.Length})");

        if (_passedIndices.Count >= checkpoints.Length)
        {
            _allPassed = true;
            OnAllCheckpointsPassed?.Invoke();
            Debug.Log("[CheckpointSystem]  Todos los checkpoints completados. Meta habilitada.");
            if (hudMessages != null)
                hudMessages.ShowMessage("¡All checkpoints reached! Go to the finish line.", 3f);
        }
        else
        {
            if (hudMessages != null)
                hudMessages.ShowMessage($"Checkpoint {index + 1}/{checkpoints.Length} ", 1.5f);
        }
    }

    public bool TryFinish()
    {
        if (_allPassed) return true;

        // Informar cuántos faltan
        int remaining = checkpoints.Length - _passedIndices.Count;
        Debug.Log($"[CheckpointSystem] Meta bloqueada. Faltan {remaining} checkpoint(s).");
        OnInvalidFinishAttempt?.Invoke();

        if (hudMessages != null)
            hudMessages.ShowMessage($"¡Finish line blocked! {remaining} remaining checkpoint(s).", 2.5f, true);

        return false;
    }

    private void HandleRespawnInput()
    {
        if (_respawnFrozen) return;
        if (!Input.GetKeyDown(respawnKey)) return;
        if (playerTransform == null) return;

        RespawnToLastCheckpoint();
    }

    public void RespawnToLastCheckpoint()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("[CheckpointSystem] playerTransform no asignado, no se puede hacer respawn.");
            return;
        }

        Vector3 respawnPos;
        Quaternion respawnRot;

        if (_lastCheckpointIndex >= 0 && _lastCheckpointIndex < checkpoints.Length)
        {
            Checkpoint cp = checkpoints[_lastCheckpointIndex];
            respawnPos = cp.RespawnPosition;
            respawnRot = cp.RespawnRotation;
            Debug.Log($"[CheckpointSystem] Respawn en checkpoint {_lastCheckpointIndex}");
        }
        else
        {
            respawnPos = GetStartPosition();
            respawnRot = GetStartRotation();
            Debug.Log("[CheckpointSystem] Respawn en posición de inicio (sin checkpoints pasados)");
        }

        playerTransform.position = respawnPos;
        playerTransform.rotation = respawnRot;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        StartRespawnFreeze();

        OnPlayerRespawned?.Invoke(_lastCheckpointIndex);

        if (hudMessages != null)
            hudMessages.ShowMessage("Respawned in last checkpoint.", 2f);
    }

    private void StartRespawnFreeze()
    {
        _respawnFrozen = true;
        _respawnFreezeTimer = respawnFreezeDuration;
    }

    private void HandleRespawnFreeze()
    {
        if (!_respawnFrozen) return;
        _respawnFreezeTimer -= Time.deltaTime;
        if (_respawnFreezeTimer <= 0f)
            _respawnFrozen = false;
    }

    private Vector3 GetStartPosition()
    {
        GameObject start = GameObject.Find("StartPosition");
        return start != null ? start.transform.position : Vector3.zero;
    }

    private Quaternion GetStartRotation()
    {
        GameObject start = GameObject.Find("StartPosition");
        return start != null ? start.transform.rotation : Quaternion.identity;
    }

    private void UpdateCheckpointVisuals()
    {
        if (checkpoints == null) return;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null) continue;
            if (_passedIndices.Contains(i))
                checkpoints[i].SetVisualState(CheckpointVisualState.Passed);
            else if (i == _nextExpectedIndex)
                checkpoints[i].SetVisualState(CheckpointVisualState.Active);
            else
                checkpoints[i].SetVisualState(CheckpointVisualState.Pending);
        }
    }

    private void OnDrawGizmos()
    {
        if (checkpoints == null || checkpoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < checkpoints.Length - 1; i++)
        {
            if (checkpoints[i] == null || checkpoints[i + 1] == null) continue;
            Gizmos.DrawLine(checkpoints[i].transform.position, checkpoints[i + 1].transform.position);
        }
    }
}