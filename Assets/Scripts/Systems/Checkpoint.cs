// N.P.C. (Null Pointer Character) - Checkpoint trigger zone for player respawning
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField, Tooltip("Vertical offset added when respawning the player to avoid clipping.")]
    private Vector3 _spawnOffset = new Vector3(0f, 1f, 0f);

    [Header("Detection")]
    [SerializeField, Tooltip("Tag used to identify the player.")]
    private string _playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField, Tooltip("Optional renderer whose material color changes on activation.")]
    private Renderer _flagRenderer;

    [SerializeField, Tooltip("Color applied to the renderer when this checkpoint is activated.")]
    private Color _activatedColor = Color.green;

    [SerializeField, Tooltip("Optional particle system played on activation.")]
    private ParticleSystem _activationVFX;

    private bool _isActivated = false;

    public bool IsActivated
    {
        get { return _isActivated; }
    }

    public Vector3 SpawnPosition
    {
        get { return transform.position + _spawnOffset; }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isActivated) return;

        if (other.CompareTag(_playerTag) || other.GetComponent<PlayerController>() != null)
        {
            Activate();
        }
    }

    private void Activate()
    {
        _isActivated = true;

        // Save this checkpoint as the current active respawn point
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(SpawnPosition, transform.rotation);
        }

        // Change color to indicate it is active
        if (_flagRenderer != null)
        {
            _flagRenderer.material.color = _activatedColor;
        }

        if (_activationVFX != null)
        {
            _activationVFX.Play();
        }

        Debug.Log($"[Checkpoint] \"{gameObject.name}\" activated.");
    }

    private void OnDrawGizmos()
    {
        // Visual helper in Scene view
        Gizmos.color = _isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(SpawnPosition, 0.35f);
        Gizmos.DrawLine(transform.position, SpawnPosition);
    }
}
