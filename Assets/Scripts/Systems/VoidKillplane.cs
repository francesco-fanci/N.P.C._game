using System;
using UnityEngine;

/// <summary>
/// Kill plane that sits beneath the playable area.
/// Any player that falls past this trigger is respawned at the latest checkpoint
/// (or their initial spawn position if no checkpoint has been set).
/// Place a large BoxCollider (Is Trigger = true) at Y = –15 (or wherever your void begins).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class VoidKillplane : MonoBehaviour
{
    #region Inspector Fields

    [Header("Respawn")]
    [SerializeField, Tooltip("Fallback spawn position used when no checkpoint has been registered.")]
    private Vector3 _defaultSpawnPosition = Vector3.zero;

    #endregion

    #region Events

    /// <summary>
    /// Fired when the player falls into the void, before respawn.
    /// Subscribe to trigger screen flash, SFX, analytics, etc.
    /// </summary>
    public static event Action OnPlayerFellInVoid;

    #endregion

    #region Static Checkpoint API

    private static Vector3? _lastCheckpoint;

    /// <summary>
    /// Registers a new checkpoint position. Call this from your
    /// checkpoint triggers / <c>CheckpointManager</c>.
    /// </summary>
    /// <param name="position">World-space respawn position.</param>
    public static void SetCheckpoint(Vector3 position)
    {
        _lastCheckpoint = position;
    }

    /// <summary>
    /// Clears the stored checkpoint so the default spawn is used.
    /// Useful on scene reload.
    /// </summary>
    public static void ClearCheckpoint()
    {
        _lastCheckpoint = null;
    }

    #endregion

    #region Private State

    /// <summary>
    /// Cached at Awake so we always have a fallback even if the user
    /// forgets to set <see cref="_defaultSpawnPosition"/> in the Inspector.
    /// </summary>
    private Vector3 _resolvedDefaultSpawn;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Ensure the collider is a trigger
        BoxCollider box = GetComponent<BoxCollider>();
        if (!box.isTrigger)
        {
            box.isTrigger = true;
            Debug.Log("[VoidKillplane] BoxCollider was not set to trigger — fixed automatically.", this);
        }

        _resolvedDefaultSpawn = _defaultSpawnPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null && !other.CompareTag("Player"))
        {
            // Not the player — might be a crate or other object; ignore.
            return;
        }

        RespawnPlayer(cc, other.transform);
    }

    #endregion

    #region Respawn Logic

    /// <summary>
    /// Moves the player back to the latest checkpoint or default spawn.
    /// Delegates to <see cref="CheckpointManager"/> if present to keep respawns synchronized.
    /// </summary>
    private void RespawnPlayer(CharacterController cc, Transform playerTransform)
    {
        // Notify listeners (screen flash, SFX, analytics…)
        OnPlayerFellInVoid?.Invoke();

        // Trigger glitch burst feedback on void death
        if (ScreenGlitchEffect.Instance != null)
        {
            ScreenGlitchEffect.Instance.TriggerGlitch(0.6f, 1f);
        }

        // Delegate to CheckpointManager singleton if available (preferred)
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer();
            return;
        }

        // Fallback if CheckpointManager is not present in the scene
        Vector3 respawnPos = _lastCheckpoint ?? _resolvedDefaultSpawn;

        if (cc != null)
        {
            cc.enabled = false;
        }

        playerTransform.position = respawnPos;

        if (cc != null)
        {
            cc.enabled = true;
        }

        PlayerController pc = playerTransform.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ResetVelocity();
        }

        Debug.Log($"[VoidKillplane] Player respawned at {respawnPos}.", this);
    }

    #endregion
}
