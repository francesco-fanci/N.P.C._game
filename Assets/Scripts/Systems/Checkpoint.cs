using UnityEngine;

/// <summary>
/// Individual checkpoint trigger zone. When the player enters the collider,
/// this checkpoint becomes the active respawn point via <see cref="CheckpointManager"/>.
/// Requires a Collider set to <c>isTrigger = true</c>.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    // ───────────────────────── Inspector Fields ─────────────────────────

    [Header("Spawn Settings")]
    [SerializeField, Tooltip("Vertical offset added to this transform's position when respawning the player (avoids ground clipping).")]
    private Vector3 _spawnOffset = new Vector3(0f, 1f, 0f);

    [Header("Detection")]
    [SerializeField, Tooltip("Tag used to identify the player.")]
    private string _playerTag = "Player";

    [Header("Visual Feedback")]
    [SerializeField, Tooltip("(Optional) Renderer whose material color changes on activation.")]
    private Renderer _flagRenderer;

    [SerializeField, Tooltip("Color applied to the flag renderer when this checkpoint is activated.")]
    private Color _activatedColor = Color.green;

    [SerializeField, Tooltip("(Optional) Particle system played on activation.")]
    private ParticleSystem _activationVFX;

    // ───────────────────────── Runtime State ────────────────────────────

    private bool _isActivated;

    // ───────────────────────── Public Properties ────────────────────────

    /// <summary>Whether this checkpoint has already been activated.</summary>
    public bool IsActivated => _isActivated;

    /// <summary>The effective respawn position (transform position + spawn offset).</summary>
    public Vector3 SpawnPosition => transform.position + _spawnOffset;

    // ───────────────────────── Unity Callbacks ──────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (_isActivated)
            return;

        if (!other.CompareTag(_playerTag))
            return;

        Activate();
    }

    // ───────────────────────── Internals ─────────────────────────────────

    /// <summary>Register this checkpoint and play feedback.</summary>
    private void Activate()
    {
        _isActivated = true;

        // Register with the manager.
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(SpawnPosition, transform.rotation);
        }
        else
        {
            Debug.LogWarning("[Checkpoint] CheckpointManager.Instance is null — checkpoint not registered.");
        }

        // Visual feedback: change material color.
        if (_flagRenderer != null)
        {
            // Use a property block so we don't create a material instance leak.
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            _flagRenderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", _activatedColor);   // URP Lit shader property
            block.SetColor("_Color", _activatedColor);        // Fallback / legacy property
            _flagRenderer.SetPropertyBlock(block);
        }

        // Visual feedback: particle burst.
        if (_activationVFX != null)
        {
            _activationVFX.Play();
        }

        Debug.Log($"[Checkpoint] \"{gameObject.name}\" activated.");
    }

#if UNITY_EDITOR
    // ───────────────────────── Editor Gizmos ────────────────────────────

    private void OnDrawGizmos()
    {
        // Draw the spawn point so designers can see where the player will appear.
        Gizmos.color = _isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(SpawnPosition, 0.35f);
        Gizmos.DrawLine(transform.position, SpawnPosition);
    }
#endif
}
