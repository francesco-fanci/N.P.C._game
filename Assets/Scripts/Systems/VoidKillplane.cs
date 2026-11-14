// N.P.C. (Null Pointer Character) - Void killplane trigger (falls below map -> respawn)
using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class VoidKillplane : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField, Tooltip("Fallback spawn position used when no checkpoint has been registered.")]
    private Vector3 _defaultSpawnPosition = Vector3.zero;

    public static event Action OnPlayerFellInVoid;

    private static Vector3? _lastCheckpoint;

    public static void SetCheckpoint(Vector3 position)
    {
        _lastCheckpoint = position;
    }

    public static void ClearCheckpoint()
    {
        _lastCheckpoint = null;
    }

    private void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (!box.isTrigger)
        {
            box.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null && !other.CompareTag("Player"))
        {
            // Ignore non-player objects falling into void
            return;
        }

        RespawnPlayer(cc, other.transform);
    }

    private void RespawnPlayer(CharacterController cc, Transform playerTransform)
    {
        OnPlayerFellInVoid?.Invoke();

        // Brief glitch effect on void death
        if (ScreenGlitchEffect.Instance != null)
        {
            ScreenGlitchEffect.Instance.TriggerGlitch(0.5f, 0.8f);
        }

        // Delegate to CheckpointManager if present in the scene
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer();
            return;
        }

        // Fallback if CheckpointManager is not present
        Vector3 respawnPos = _lastCheckpoint.HasValue ? _lastCheckpoint.Value : _defaultSpawnPosition;

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

        Debug.Log($"[VoidKillplane] Player respawned at {respawnPos}.");
    }
}
