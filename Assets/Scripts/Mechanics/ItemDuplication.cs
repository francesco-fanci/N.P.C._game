// N.P.C. (Null Pointer Character) - Item duplication glitch mechanic
// Press E to spawn physics crates, capped with recycling of the oldest one

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ItemDuplication : MonoBehaviour
{
    // Simple counter to block duplication when interactables are nearby
    private static int _nearbyInteractables = 0;

    public static void BlockDuplication()
    {
        _nearbyInteractables++;
    }

    public static void AllowDuplication()
    {
        _nearbyInteractables = Mathf.Max(0, _nearbyInteractables - 1);
    }

    public static bool CanDuplicate => _nearbyInteractables <= 0;

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

    // Fired every time a crate is duplicated, parameter is active crate count
    public static event Action<int> OnCrateDuplicated;

    private readonly List<GameObject> _activeCrates = new List<GameObject>();
    private float _lastDuplicateTime = Mathf.NegativeInfinity;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Don't duplicate if something else is using the E key
            if (!CanDuplicate) return;

            TryDuplicate();
        }
    }

    private void OnDestroy()
    {
        _nearbyInteractables = 0;
    }

    // Attempts to spawn a crate, respecting cooldown and max cap
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

        // Clean up any externally destroyed crates
        _activeCrates.RemoveAll(c => c == null);

        // Destroy the oldest crate if we hit the limit
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

        // Make sure it has a Rigidbody so it falls and hits pressure plates
        if (newCrate.GetComponent<Rigidbody>() == null)
        {
            newCrate.AddComponent<Rigidbody>();
        }

        _activeCrates.Add(newCrate);
        _lastDuplicateTime = Time.time;

        PlayDuplicationEffect(newCrate);

        OnCrateDuplicated?.Invoke(_activeCrates.Count);
    }

    // Returns how many crates are currently alive
    public int GetActiveCrateCount()
    {
        _activeCrates.RemoveAll(c => c == null);
        return _activeCrates.Count;
    }

    // Hook for VFX/SFX when a crate spawns
    private void PlayDuplicationEffect(GameObject spawnedCrate)
    {
        // TODO: add particle effect or sound here
    }
}
