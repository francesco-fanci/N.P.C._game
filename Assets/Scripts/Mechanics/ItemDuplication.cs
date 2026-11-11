using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows the player to "exploit" item duplication — a glitch-turned-mechanic.
/// Press E to spawn a physics crate at the designated spawn point.
/// Active crates are capped; the oldest is recycled when the limit is reached.
/// Attach this component to the Player GameObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ItemDuplication : MonoBehaviour
{
    #region Interaction Blocker API

    private static readonly HashSet<object> _interactionBlockers = new HashSet<object>();

    /// <summary>
    /// Registers an object (e.g. NPC or lore terminal) that is currently using the [E] key.
    /// Prevents accidental crate duplication during dialogue or inspection.
    /// </summary>
    public static void RegisterInteractionBlocker(object source)
    {
        if (source != null)
        {
            _interactionBlockers.Add(source);
        }
    }

    /// <summary>
    /// Unregisters an interaction blocker when the player leaves range or closes the dialogue.
    /// </summary>
    public static void UnregisterInteractionBlocker(object source)
    {
        if (source != null)
        {
            _interactionBlockers.Remove(source);
        }
    }

    /// <summary>
    /// True when any NPC, terminal, or wall trigger is currently in range of the player.
    /// </summary>
    public static bool IsInteractionBlocked => _interactionBlockers.Count > 0;

    #endregion

    #region Inspector Fields

    [Header("Duplication Setup")]
    [SerializeField, Tooltip("Prefab of the crate to duplicate. Must have a Rigidbody.")]
    private GameObject _cratePrefab;

    [SerializeField, Tooltip("Transform slightly in front of the player where crates spawn.")]
    private Transform _spawnPoint;

    [Header("Limits")]
    [SerializeField, Tooltip("Maximum number of active crates allowed at once.")]
    private int _maxCrates = 5;

    [SerializeField, Tooltip("Cooldown in seconds between duplication attempts.")]
    private float _cooldown = 0.5f;

    #endregion

    #region Events

    /// <summary>
    /// Fired every time a crate is successfully duplicated.
    /// Parameter is the current number of active crates.
    /// </summary>
    public static event Action<int> OnCrateDuplicated;

    #endregion

    #region Private State

    private readonly List<GameObject> _activeCrates = new List<GameObject>();
    private float _lastDuplicateTime = Mathf.NegativeInfinity;

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Do not duplicate if an interaction (NPC, terminal, wall) is using the E key
            if (IsInteractionBlocked) return;

            TryDuplicate();
        }
    }

    private void OnDestroy()
    {
        _interactionBlockers.Clear();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Attempts to duplicate a crate. Respects cooldown and max-crate cap.
    /// Can be called from other systems (e.g. UI buttons) as well as from Update.
    /// </summary>
    public void TryDuplicate()
    {
        if (_cratePrefab == null || _spawnPoint == null)
        {
            Debug.LogWarning("[ItemDuplication] Crate prefab or spawn point is not assigned.", this);
            return;
        }

        // Cooldown guard
        if (Time.time - _lastDuplicateTime < _cooldown)
        {
            return;
        }

        // Purge any crates that were destroyed externally (e.g. fell into the void)
        _activeCrates.RemoveAll(c => c == null);

        // Enforce cap — destroy the oldest crate if we are at the limit
        if (_activeCrates.Count >= _maxCrates)
        {
            GameObject oldest = _activeCrates[0];
            _activeCrates.RemoveAt(0);

            if (oldest != null)
            {
                Destroy(oldest);
            }
        }

        // Spawn the new crate
        GameObject newCrate = Instantiate(_cratePrefab, _spawnPoint.position, _spawnPoint.rotation);

        // Guarantee a Rigidbody so the crate falls and interacts with pressure plates
        if (newCrate.GetComponent<Rigidbody>() == null)
        {
            newCrate.AddComponent<Rigidbody>();
        }

        _activeCrates.Add(newCrate);
        _lastDuplicateTime = Time.time;

        // Visual / audio hook
        PlayDuplicationEffect(newCrate);

        // Notify listeners
        OnCrateDuplicated?.Invoke(_activeCrates.Count);
    }

    /// <summary>
    /// Returns the current number of active (non-destroyed) crates.
    /// </summary>
    public int GetActiveCrateCount()
    {
        _activeCrates.RemoveAll(c => c == null);
        return _activeCrates.Count;
    }

    #endregion

    #region Effects Hook

    /// <summary>
    /// Called immediately after a crate is spawned.
    /// Override in a subclass or subscribe to <see cref="OnCrateDuplicated"/>
    /// to play particles, sounds, or screen effects.
    /// </summary>
    /// <param name="spawnedCrate">The newly instantiated crate.</param>
    protected virtual void PlayDuplicationEffect(GameObject spawnedCrate)
    {
        // Stub — hook your VFX / SFX here.
        // Example: Instantiate a particle prefab at spawnedCrate.transform.position
        // Example: AudioSource.PlayClipAtPoint(clip, spawnedCrate.transform.position);
    }

    #endregion
}
