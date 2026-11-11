using System.Collections;
using UnityEngine;

/// <summary>
/// Simulates "wall clipping through broken polygon seams" — a glitch-as-design mechanic.
/// When the player enters the trigger zone and presses E, the wall's solid collider
/// is temporarily disabled so the player can walk through.
/// Attach to the wall segment. The wall needs:
///   1. A <see cref="MeshRenderer"/> (the visible surface).
///   2. A solid <see cref="Collider"/> that normally blocks movement.
///   3. A trigger <see cref="Collider"/> (on this object or a child) that detects the player.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class ClippingTrigger : MonoBehaviour
{
    #region Inspector Fields

    [Header("Wall References")]
    [SerializeField, Tooltip("The solid collider that blocks the player. Disabled during clipping.")]
    private Collider _wallCollider;

    [Header("Clipping Settings")]
    [SerializeField, Tooltip("If true, requires pressing E inside the trigger. If false, activates automatically on touch.")]
    private bool _requireKeyPress = true;

    [SerializeField, Tooltip("How long (seconds) the wall stays passable after activation.")]
    private float _clipDuration = 2f;

    [Header("Visual Feedback (Optional)")]
    [SerializeField, Tooltip("Material swapped onto the wall while it is clipped. Leave empty to skip.")]
    private Material _glitchMaterial;

    #endregion

    #region Private State

    private MeshRenderer _meshRenderer;
    private Material _originalMaterial;
    private bool _playerInside;
    private bool _isClipped;
    private Coroutine _clipRoutine;
    private Transform _playerTransform;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshRenderer != null)
        {
            _originalMaterial = _meshRenderer.sharedMaterial;
        }

        if (_wallCollider == null)
        {
            Debug.LogWarning("[ClippingTrigger] Wall collider is not assigned. " +
                             "Attempting to find a non-trigger collider on this object.", this);

            foreach (Collider col in GetComponents<Collider>())
            {
                if (!col.isTrigger)
                {
                    _wallCollider = col;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        // Activate clipping when the player is in the trigger zone and presses E
        if (_playerInside && !_isClipped && Input.GetKeyDown(KeyCode.E))
        {
            if (_clipRoutine != null)
            {
                StopCoroutine(_clipRoutine);
            }

            _clipRoutine = StartCoroutine(ClipRoutine());
        }
    }

    #endregion

    #region Trigger Callbacks

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerInside = true;
            _playerTransform = other.transform;

            if (_requireKeyPress)
            {
                ItemDuplication.RegisterInteractionBlocker(this);
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.SetPrompt("[E] Phase Wall (Glitch)", this);
                }
            }
            else if (!_isClipped)
            {
                if (_clipRoutine != null) StopCoroutine(_clipRoutine);
                _clipRoutine = StartCoroutine(ClipRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerInside = false;

            if (_requireKeyPress)
            {
                ItemDuplication.UnregisterInteractionBlocker(this);
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.ClearPrompt(this);
                }
            }
        }
    }

    private void OnDisable()
    {
        _playerInside = false;

        if (_requireKeyPress)
        {
            ItemDuplication.UnregisterInteractionBlocker(this);
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ClearPrompt(this);
            }
        }

        // Restore collider and material state safely
        if (_wallCollider != null)
        {
            _wallCollider.enabled = true;
        }

        if (_glitchMaterial != null && _meshRenderer != null && _originalMaterial != null)
        {
            _meshRenderer.sharedMaterial = _originalMaterial;
        }

        _isClipped = false;
        _clipRoutine = null;
    }

    #endregion

    #region Clipping Logic

    /// <summary>
    /// Disables the wall collider for <see cref="_clipDuration"/> seconds,
    /// then re-enables it once the player is no longer overlapping.
    /// Includes timeout and distance failsafes against deadlocks.
    /// </summary>
    private IEnumerator ClipRoutine()
    {
        _isClipped = true;

        // Disable the solid wall
        if (_wallCollider != null)
        {
            _wallCollider.enabled = false;
        }

        // Swap to glitch material using sharedMaterial to prevent leaks
        if (_glitchMaterial != null && _meshRenderer != null)
        {
            _meshRenderer.sharedMaterial = _glitchMaterial;
        }

        // Trigger dynamic screen tear / chromatic glitch burst
        if (ScreenGlitchEffect.Instance != null)
        {
            ScreenGlitchEffect.Instance.TriggerGlitch(0.6f, 1f);
        }

        // Wait for the clip duration
        yield return new WaitForSeconds(_clipDuration);

        // Wait until the player has left before re-enabling, with timeout and distance failsafe
        float maxWait = 6f;
        float elapsed = 0f;
        while (_playerInside && elapsed < maxWait)
        {
            if (_playerTransform != null && Vector3.Distance(transform.position, _playerTransform.position) > 6f)
            {
                _playerInside = false;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        _playerInside = false;

        // Re-enable the solid wall
        if (_wallCollider != null)
        {
            _wallCollider.enabled = true;
        }

        // Restore original material
        if (_glitchMaterial != null && _meshRenderer != null && _originalMaterial != null)
        {
            _meshRenderer.sharedMaterial = _originalMaterial;
        }

        _isClipped = false;
        _clipRoutine = null;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Checks whether the collider belongs to the player.
    /// Uses <see cref="CharacterController"/> detection and tag fallback.
    /// </summary>
    private static bool IsPlayer(Collider other)
    {
        return other.GetComponent<CharacterController>() != null
            || other.CompareTag("Player");
    }

    #endregion
}
