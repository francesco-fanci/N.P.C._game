using UnityEngine;

/// <summary>
/// 3rd-person orbital camera for "N.P.C. (Null Pointer Character)".
/// Lives on its own GameObject (NOT parented to the player) and follows
/// the player each frame while orbiting via mouse input.
/// Performs a basic collision raycast so the camera pulls forward when
/// geometry is between it and the player — but intentionally does NOT
/// fight wall clipping too aggressively so the player can peek through
/// backface-culled hollow geometry (a deliberate design choice).
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Target
    // ─────────────────────────────────────────────────────────────────

    [Header("Target")]
    [SerializeField, Tooltip("The player Transform this camera orbits around.")]
    private Transform _target;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Orbit
    // ─────────────────────────────────────────────────────────────────

    [Header("Orbit")]
    [SerializeField, Tooltip("Mouse look sensitivity.")]
    private float _mouseSensitivity = 2.0f;

    [SerializeField, Tooltip("Minimum vertical angle (looking up). Negative = below horizon.")]
    private float _minVerticalAngle = -35f;

    [SerializeField, Tooltip("Maximum vertical angle (looking down towards ground).")]
    private float _maxVerticalAngle = 70f;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Framing
    // ─────────────────────────────────────────────────────────────────

    [Header("Framing")]
    [SerializeField, Tooltip("Default distance from the player pivot.")]
    private float _distance = 3.5f;

    [SerializeField, Tooltip("Height offset above the player pivot (shoulder height).")]
    private float _heightOffset = 1.5f;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Collision
    // ─────────────────────────────────────────────────────────────────

    [Header("Collision")]
    [SerializeField, Tooltip("Layers that can block the camera line-of-sight.")]
    private LayerMask _collisionLayers = ~0;

    [SerializeField, Tooltip("Small padding so the camera doesn't sit right on the wall.")]
    private float _collisionPadding = 0.2f;

    [SerializeField, Tooltip("Radius of the SphereCast used for camera collision.")]
    private float _collisionRadius = 0.15f;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Smoothing
    // ─────────────────────────────────────────────────────────────────

    [Header("Smoothing")]
    [SerializeField, Tooltip("How quickly the camera recovers its full distance after a collision pull-in.")]
    private float _distanceRecoverySpeed = 8f;

    // ─────────────────────────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────────────────────────

    private float _yaw;
    private float _pitch;
    private float _currentDistance;

    // ─────────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Auto-enable URP Post-Processing on camera
        var camData = GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (camData != null)
        {
            camData.renderPostProcessing = true;
        }

        // Auto-find player if not assigned
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
            else
            {
                Debug.LogWarning("[PlayerCamera] No target assigned and no GameObject tagged 'Player' found.", this);
            }
        }

        _currentDistance = _distance;
    }

    private void Start()
    {
        // Lock and hide cursor by default
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialise yaw from the camera's current facing so it doesn't snap on Start
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;

        // Normalise pitch into -180..180 range
        if (_pitch > 180f) _pitch -= 360f;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        HandleRotationInput();
        UpdateCameraPosition();
    }

    // ─────────────────────────────────────────────────────────────────
    //  Rotation
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads mouse axes and updates yaw / pitch with clamping.
    /// </summary>
    private void HandleRotationInput()
    {
        // Only process mouse input when the cursor is locked
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _yaw += mouseX;
        _pitch -= mouseY;

        _pitch = Mathf.Clamp(_pitch, _minVerticalAngle, _maxVerticalAngle);
    }

    // ─────────────────────────────────────────────────────────────────
    //  Positioning & Collision
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates the desired camera position from the orbit parameters,
    /// performs a collision check, and places the camera.
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Pivot point: player position + height offset
        Vector3 pivot = _target.position + Vector3.up * _heightOffset;

        // Desired offset direction from the orbit angles
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredOffset = rotation * Vector3.back * _distance;
        Vector3 desiredPosition = pivot + desiredOffset;

        // ── Collision ────────────────────────────────────────────────
        float desiredDist = _distance;
        Vector3 directionFromPivot = (desiredPosition - pivot).normalized;
        float castDistance = _distance;

        if (Physics.SphereCast(pivot, _collisionRadius, directionFromPivot,
                               out RaycastHit hit, castDistance, _collisionLayers,
                               QueryTriggerInteraction.Ignore))
        {
            // Pull the camera in front of the hit point
            desiredDist = Mathf.Max(hit.distance - _collisionPadding, 0.1f);
        }

        // Smoothly recover toward full distance when no longer blocked
        _currentDistance = Mathf.MoveTowards(_currentDistance, desiredDist,
                                              _distanceRecoverySpeed * Time.deltaTime);

        // If the collision is closer than current, snap immediately (don't clip through)
        if (desiredDist < _currentDistance)
        {
            _currentDistance = desiredDist;
        }

        // Final camera placement
        Vector3 finalPosition = pivot + directionFromPivot * _currentDistance;
        transform.position = finalPosition;
        transform.rotation = rotation;
    }
}
