using UnityEngine;

/// <summary>
/// Singleton that tracks the currently active checkpoint and handles player respawning.
/// Uses a simple static instance that does NOT persist across scene loads.
/// Attach to a single GameObject in the scene (e.g., "CheckpointManager").
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    // ───────────────────────── Singleton ─────────────────────────────────

    /// <summary>Scene-local singleton instance.</summary>
    public static CheckpointManager Instance { get; private set; }

    // ───────────────────────── Inspector Fields ─────────────────────────

    [Header("References")]
    [SerializeField, Tooltip("The player's Transform. Auto-found via tag if left empty.")]
    private Transform _playerTransform;

    [Header("Settings")]
    [SerializeField, Tooltip("Tag used to locate the player if no reference is assigned.")]
    private string _playerTag = "Player";

    // ───────────────────────── Runtime State ────────────────────────────

    private Vector3 _checkpointPosition;
    private Quaternion _checkpointRotation;
    private CharacterController _playerController;

    // ───────────────────────── Public Properties ────────────────────────

    /// <summary>World-space position of the current active checkpoint.</summary>
    public Vector3 CheckpointPosition => _checkpointPosition;

    /// <summary>Rotation of the current active checkpoint.</summary>
    public Quaternion CheckpointRotation => _checkpointRotation;

    // ───────────────────────── Public API ────────────────────────────────

    /// <summary>
    /// Register a new active checkpoint. Called by <see cref="Checkpoint"/> trigger zones.
    /// </summary>
    /// <param name="position">World-space respawn position.</param>
    /// <param name="rotation">Respawn facing rotation.</param>
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        _checkpointPosition = position;
        _checkpointRotation = rotation;

        // Synchronize with VoidKillplane fallback API
        VoidKillplane.SetCheckpoint(position);

        Debug.Log($"[CheckpointManager] Checkpoint set at {position}");
    }

    /// <summary>
    /// Teleport the player to the current checkpoint.
    /// Properly disables the <see cref="CharacterController"/> before moving the transform
    /// and resets downward/horizontal momentum.
    /// </summary>
    public void RespawnPlayer()
    {
        if (_playerTransform == null)
        {
            InitializePlayerReference();
        }

        if (_playerTransform == null)
        {
            Debug.LogError("[CheckpointManager] Cannot respawn — player reference is missing.");
            return;
        }

        // CharacterController must be disabled before moving the transform directly,
        // otherwise the controller's internal state overrides the position change.
        if (_playerController != null)
            _playerController.enabled = false;

        _playerTransform.position = _checkpointPosition;
        _playerTransform.rotation = _checkpointRotation;

        if (_playerController != null)
            _playerController.enabled = true;

        // Reset velocity so player doesn't slam into the ground with stored falling speed
        PlayerController pc = _playerTransform.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ResetVelocity();
        }

        Debug.Log($"[CheckpointManager] Player respawned at {_checkpointPosition}");
    }

    // ───────────────────────── Unity Callbacks ──────────────────────────

    private void Awake()
    {
        // Simple scene-local singleton — no DontDestroyOnLoad.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CheckpointManager] Duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePlayerReference();
    }

    private void Start()
    {
        if (_playerTransform == null)
        {
            InitializePlayerReference();
        }
    }

    private void InitializePlayerReference()
    {
        // Auto-find the player if no reference was assigned in the Inspector.
        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(_playerTag);
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
        }

        // Cache the CharacterController.
        if (_playerTransform != null && _playerController == null)
        {
            _playerController = _playerTransform.GetComponent<CharacterController>();
        }

        // Save the player's initial position as the first checkpoint if not already set.
        if (_playerTransform != null && _checkpointPosition == Vector3.zero && _checkpointRotation == Quaternion.identity)
        {
            _checkpointPosition = _playerTransform.position;
            _checkpointRotation = _playerTransform.rotation;
            VoidKillplane.SetCheckpoint(_checkpointPosition);
            Debug.Log("[CheckpointManager] Initial checkpoint saved at player start position.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
