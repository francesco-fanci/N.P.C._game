// N.P.C. (Null Pointer Character) - Manager for active checkpoint and player respawn
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("References")]
    [SerializeField, Tooltip("The player Transform. Auto-found via tag if left empty.")]
    private Transform _playerTransform;

    [Header("Settings")]
    [SerializeField, Tooltip("Tag used to locate the player if no reference is assigned.")]
    private string _playerTag = "Player";

    private Vector3 _checkpointPosition;
    private Quaternion _checkpointRotation;
    private CharacterController _playerController;

    public Vector3 CheckpointPosition
    {
        get { return _checkpointPosition; }
    }

    public Quaternion CheckpointRotation
    {
        get { return _checkpointRotation; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        FindPlayer();
    }

    private void Start()
    {
        if (_playerTransform == null)
        {
            FindPlayer();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Set a new active checkpoint position
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        _checkpointPosition = position;
        _checkpointRotation = rotation;

        VoidKillplane.SetCheckpoint(position);
        Debug.Log($"[CheckpointManager] Checkpoint updated to {position}");
    }

    // Respawns the player at the current active checkpoint
    public void RespawnPlayer()
    {
        if (_playerTransform == null)
        {
            FindPlayer();
        }

        if (_playerTransform == null)
        {
            Debug.LogError("[CheckpointManager] Cannot respawn: player not found!");
            return;
        }

        // CharacterController must be disabled before directly modifying transform position
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        _playerTransform.position = _checkpointPosition;
        _playerTransform.rotation = _checkpointRotation;

        if (_playerController != null)
        {
            _playerController.enabled = true;
        }

        // Reset any falling momentum
        PlayerController pc = _playerTransform.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ResetVelocity();
        }

        Debug.Log($"[CheckpointManager] Player respawned at {_checkpointPosition}");
    }

    private void FindPlayer()
    {
        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(_playerTag);
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
        }

        if (_playerTransform != null && _playerController == null)
        {
            _playerController = _playerTransform.GetComponent<CharacterController>();
        }

        // Default first checkpoint to player's starting position
        if (_playerTransform != null && _checkpointPosition == Vector3.zero)
        {
            _checkpointPosition = _playerTransform.position;
            _checkpointRotation = _playerTransform.rotation;
            VoidKillplane.SetCheckpoint(_checkpointPosition);
        }
    }
}
