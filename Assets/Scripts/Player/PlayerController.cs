using UnityEngine;

/// <summary>
/// 3rd-person character controller for "N.P.C. (Null Pointer Character)".
/// Uses <see cref="CharacterController"/> for all movement — no Rigidbody.
/// Supports walk / sprint, custom gravity, single jump, and
/// <em>Coyote Time</em> ("False Jump") — a grace window that lets the player
/// jump shortly after leaving a ledge.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Movement
    // ─────────────────────────────────────────────────────────────────

    [Header("Movement")]
    [SerializeField, Tooltip("Walking speed in m/s.")]
    private float _walkSpeed = 5f;

    [SerializeField, Tooltip("Sprint speed in m/s (Left Shift).")]
    private float _sprintSpeed = 8f;

    [SerializeField, Tooltip("How quickly the character accelerates to target speed.")]
    private float _movementSmoothing = 0.1f;

    [SerializeField, Tooltip("Rate at which the character rotates toward the movement direction (degrees/s).")]
    private float _rotationSpeed = 720f;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Gravity & Jump
    // ─────────────────────────────────────────────────────────────────

    [Header("Gravity & Jump")]
    [SerializeField, Tooltip("Gravity acceleration (negative = downward). -20 gives a snappy feel.")]
    private float _gravity = -20f;

    [SerializeField, Tooltip("Jump height in metres.")]
    private float _jumpHeight = 1.5f;

    [SerializeField, Tooltip("Velocity applied on the frame the character lands to keep it stuck to slopes.")]
    private float _stickToGroundForce = -2f;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Coyote Time ("False Jump")
    // ─────────────────────────────────────────────────────────────────

    [Header("Coyote Time (\"False Jump\")")]
    [SerializeField, Tooltip("Seconds after leaving a ledge during which the player can still jump.")]
    private float _coyoteTime = 0.2f;

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Ground Check
    // ─────────────────────────────────────────────────────────────────

    [Header("Ground Check")]
    [SerializeField, Tooltip("Extra radius for the ground-check SphereCast.")]
    private float _groundCheckRadius = 0.25f;

    [SerializeField, Tooltip("Distance below the character checked for ground.")]
    private float _groundCheckDistance = 0.15f;

    [SerializeField, Tooltip("Layers considered as ground.")]
    private LayerMask _groundLayers = ~0; // everything by default

    // ─────────────────────────────────────────────────────────────────
    //  Inspector — Camera Reference
    // ─────────────────────────────────────────────────────────────────

    [Header("Camera Reference")]
    [SerializeField, Tooltip("Transform of the camera used to compute movement direction. " +
                             "Assign the PlayerCamera GameObject here.")]
    private Transform _cameraTransform;

    // ─────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// True when the character is considered grounded (controller + SphereCast).
    /// </summary>
    public bool IsGrounded => _isGrounded;

    /// <summary>
    /// Fired on the frame the character lands after being airborne.
    /// </summary>
    public event System.Action OnLanded;

    // ─────────────────────────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────────────────────────

    private CharacterController _controller;
    private Vector3 _velocity;
    private Vector3 _smoothVelocity;
    private float _currentSpeed;

    private bool _isGrounded;
    private bool _wasGroundedLastFrame;
    private float _lastGroundedTime;
    private bool _jumpRequested;

    // ─────────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (_cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                _cameraTransform = mainCam.transform;
            }
            else
            {
                Debug.LogWarning($"[PlayerController] No camera assigned and no MainCamera found.", this);
            }
        }
    }

    private void Update()
    {
        UpdateGroundCheck();
        HandleJumpInput();
        HandleMovement();
        ApplyGravityAndJump();
        ApplyMotion();
    }

    // ─────────────────────────────────────────────────────────────────
    //  Ground Check
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Combines <see cref="CharacterController.isGrounded"/> with a
    /// SphereCast for extra reliability on slopes and edges.
    /// </summary>
    private void UpdateGroundCheck()
    {
        _wasGroundedLastFrame = _isGrounded;

        // Primary: CharacterController built-in check
        bool controllerGrounded = _controller.isGrounded;

        // Secondary: SphereCast from the bottom sphere of the CharacterController downward
        bool sphereGrounded = false;
        Vector3 sphereOrigin = transform.position + _controller.center + Vector3.down * (_controller.height * 0.5f - _controller.radius);
        float castDistance = _controller.radius + _groundCheckDistance;
        if (Physics.SphereCast(sphereOrigin, _groundCheckRadius, Vector3.down,
                               out _, castDistance, _groundLayers,
                               QueryTriggerInteraction.Ignore))
        {
            sphereGrounded = true;
        }

        _isGrounded = controllerGrounded || sphereGrounded;

        // Track last grounded timestamp for coyote time
        if (_isGrounded)
        {
            _lastGroundedTime = Time.time;
        }

        // Landing event
        if (_isGrounded && !_wasGroundedLastFrame)
        {
            OnLanded?.Invoke();
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Input
    // ─────────────────────────────────────────────────────────────────

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _jumpRequested = true;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Movement (camera-relative)
    // ─────────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        if (input.magnitude >= 0.1f && _cameraTransform != null)
        {
            // Build camera-relative direction (ignore camera pitch)
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 desiredDirection = (camForward * input.z + camRight * input.x).normalized;

            // Rotate character toward movement direction
            float targetAngle = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle,
                                                        _rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            // Determine target speed (walk vs sprint)
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isSprinting ? _sprintSpeed : _walkSpeed;

            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _smoothVelocity.x,
                                              _movementSmoothing);

            _velocity.x = desiredDirection.x * _currentSpeed;
            _velocity.z = desiredDirection.z * _currentSpeed;
        }
        else
        {
            // Decelerate to zero
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, 0f, ref _smoothVelocity.x,
                                              _movementSmoothing);
            _velocity.x = Mathf.SmoothDamp(_velocity.x, 0f, ref _smoothVelocity.y, _movementSmoothing);
            _velocity.z = Mathf.SmoothDamp(_velocity.z, 0f, ref _smoothVelocity.z, _movementSmoothing);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  Gravity & Jump
    // ─────────────────────────────────────────────────────────────────

    private void ApplyGravityAndJump()
    {
        if (_isGrounded && _velocity.y < 0f)
        {
            // Small downward velocity keeps the controller snapped to slopes
            _velocity.y = _stickToGroundForce;
        }

        // Coyote time: allow jump if grounded OR within the grace window
        bool canJump = _isGrounded || (Time.time - _lastGroundedTime <= _coyoteTime);

        if (_jumpRequested && canJump)
        {
            // v = sqrt(2 * |gravity| * jumpHeight)
            _velocity.y = Mathf.Sqrt(2f * Mathf.Abs(_gravity) * _jumpHeight);
            _isGrounded = false;
            _lastGroundedTime = -1f; // consume coyote time
        }

        _jumpRequested = false;

        // Apply gravity every frame
        _velocity.y += _gravity * Time.deltaTime;
    }

    // ─────────────────────────────────────────────────────────────────
    //  Final Motion
    // ─────────────────────────────────────────────────────────────────

    private void ApplyMotion()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }

    // ─────────────────────────────────────────────────────────────────
    //  Public Utilities
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets character velocity and movement momentum to zero.
    /// Call when teleporting or respawning at a checkpoint.
    /// </summary>
    public void ResetVelocity()
    {
        _velocity = Vector3.zero;
        _smoothVelocity = Vector3.zero;
        _currentSpeed = 0f;
        _jumpRequested = false;
    }
}
