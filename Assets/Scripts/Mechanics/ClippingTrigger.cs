// N.P.C. (Null Pointer Character) - Wall clipping glitch mechanic
// Lets the player phase through a wall by pressing E in the trigger zone

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ClippingTrigger : MonoBehaviour
{
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

    private MeshRenderer _meshRenderer;
    private Material _originalMaterial;
    private bool _playerInside;
    private bool _isClipped;
    private Coroutine _clipRoutine;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshRenderer != null)
        {
            _originalMaterial = _meshRenderer.sharedMaterial;
        }

        // Try to auto-find the solid collider if not assigned
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

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        _playerInside = true;

        if (_requireKeyPress)
        {
            ItemDuplication.BlockDuplication();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowPrompt("[E] Phase Wall (Glitch)");
            }
        }
        else if (!_isClipped)
        {
            // Auto-clip mode: start immediately
            if (_clipRoutine != null) StopCoroutine(_clipRoutine);
            _clipRoutine = StartCoroutine(ClipRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        _playerInside = false;

        if (_requireKeyPress)
        {
            ItemDuplication.AllowDuplication();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.HidePrompt();
            }
        }
    }

    private void OnDisable()
    {
        _playerInside = false;

        if (_requireKeyPress)
        {
            ItemDuplication.AllowDuplication();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.HidePrompt();
            }
        }

        // Restore collider and material so nothing stays broken
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

    // Disables the wall collider for a set duration, then re-enables it
    private IEnumerator ClipRoutine()
    {
        _isClipped = true;

        // Disable the solid wall so player can walk through
        if (_wallCollider != null)
        {
            _wallCollider.enabled = false;
        }

        // Swap to glitch material for visual feedback
        if (_glitchMaterial != null && _meshRenderer != null)
        {
            _meshRenderer.sharedMaterial = _glitchMaterial;
        }

        // Trigger screen glitch effect
        if (ScreenGlitchEffect.Instance != null)
        {
            ScreenGlitchEffect.Instance.TriggerGlitch(0.6f, 1f);
        }

        // Wait for the clip duration
        yield return new WaitForSeconds(_clipDuration);

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

    // Check if the collider belongs to the player
    private static bool IsPlayer(Collider other)
    {
        return other.GetComponent<CharacterController>() != null
            || other.CompareTag("Player");
    }
}
