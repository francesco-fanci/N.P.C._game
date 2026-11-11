using UnityEngine;

/// <summary>
/// Procedural animator for the stylized humanoid Merchant character in "N.P.C.".
/// Drives limb swings, body bounce, lean into turns, jump tucks, and landing squash/stretch
/// based on CharacterController velocity and grounding state.
/// </summary>
public class PlayerModelAnimator : MonoBehaviour
{
    #region Inspector Fields — Rig References

    [Header("Rig Transforms (Children of Model Root)")]
    [SerializeField, Tooltip("Hips / Root of the visual mesh.")]
    private Transform _hips;

    [SerializeField, Tooltip("Upper torso / chest.")]
    private Transform _torso;

    [SerializeField, Tooltip("Left leg thigh.")]
    private Transform _legL;

    [SerializeField, Tooltip("Right leg thigh.")]
    private Transform _legR;

    [SerializeField, Tooltip("Left arm shoulder.")]
    private Transform _armL;

    [SerializeField, Tooltip("Right arm shoulder.")]
    private Transform _armR;

    [SerializeField, Tooltip("Head.")]
    private Transform _head;

    [Header("Animation Tuning")]
    [SerializeField, Tooltip("Leg & arm swing frequency multiplier.")]
    private float _strideFrequency = 11f;

    [SerializeField, Tooltip("Leg swing angle in degrees during walk.")]
    private float _maxLegAngle = 38f;

    [SerializeField, Tooltip("Arm swing angle in degrees during walk.")]
    private float _maxArmAngle = 32f;

    [SerializeField, Tooltip("Vertical hip bobbing amount during walk.")]
    private float _hipBobAmount = 0.08f;

    [SerializeField, Tooltip("Forward body lean angle during sprint.")]
    private float _sprintLeanAngle = 14f;

    #endregion

    #region Private State

    private CharacterController _controller;
    private PlayerController _playerController;
    private float _walkCycle = 0f;
    private float _squashFactor = 1f;
    private Vector3 _hipsInitialLocalPos;
    private Quaternion _torsoInitialLocalRot;
    private Quaternion _headInitialLocalRot;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _controller = GetComponentInParent<CharacterController>();
        _playerController = GetComponentInParent<PlayerController>();

        if (_hips != null)
        {
            _hipsInitialLocalPos = _hips.localPosition;
        }

        if (_torso != null)
        {
            _torsoInitialLocalRot = _torso.localRotation;
        }

        if (_head != null)
        {
            _headInitialLocalRot = _head.localRotation;
        }
    }

    private void OnEnable()
    {
        if (_playerController != null)
        {
            _playerController.OnLanded += HandleLanded;
        }
    }

    private void OnDisable()
    {
        if (_playerController != null)
        {
            _playerController.OnLanded -= HandleLanded;
        }
    }

    private void Update()
    {
        if (_hips == null) return;

        // Smooth squash recovery
        _squashFactor = Mathf.MoveTowards(_squashFactor, 1f, Time.deltaTime * 6f);

        // Determine ground & speed state
        bool isGrounded = _controller != null ? _controller.isGrounded : true;
        Vector3 horizontalVelocity = _controller != null ? new Vector3(_controller.velocity.x, 0f, _controller.velocity.z) : Vector3.zero;
        float speed = horizontalVelocity.magnitude;
        bool isMoving = isGrounded && speed > 0.2f;

        if (isGrounded)
        {
            if (isMoving)
            {
                // Advance walk/sprint cycle
                float speedFactor = Mathf.Clamp(speed / 5f, 0.5f, 2f);
                _walkCycle += Time.deltaTime * _strideFrequency * speedFactor;

                float legSwing = Mathf.Sin(_walkCycle) * _maxLegAngle;
                float armSwing = Mathf.Cos(_walkCycle) * _maxArmAngle;

                // Legs swing in opposition
                if (_legL != null) _legL.localRotation = Quaternion.Euler(legSwing, 0f, 0f);
                if (_legR != null) _legR.localRotation = Quaternion.Euler(-legSwing, 0f, 0f);

                // Arms swing opposite to corresponding leg
                if (_armL != null) _armL.localRotation = Quaternion.Euler(-armSwing, 0f, 0f);
                if (_armR != null) _armR.localRotation = Quaternion.Euler(armSwing, 0f, 0f);

                // Hip bobbing with two dips per walk cycle
                float bobY = Mathf.Abs(Mathf.Sin(_walkCycle)) * _hipBobAmount;
                _hips.localPosition = _hipsInitialLocalPos + Vector3.up * bobY;

                // Forward lean proportional to speed
                float lean = Mathf.Clamp(speed / 8f, 0f, 1f) * _sprintLeanAngle;
                if (_torso != null) _torso.localRotation = Quaternion.Euler(lean, 0f, 0f);
            }
            else
            {
                // Idle breathing cycle
                float breath = Mathf.Sin(Time.time * 2.2f) * 0.02f;
                _hips.localPosition = Vector3.Lerp(_hips.localPosition, _hipsInitialLocalPos + Vector3.up * breath, Time.deltaTime * 8f);

                if (_legL != null) _legL.localRotation = Quaternion.Slerp(_legL.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (_legR != null) _legR.localRotation = Quaternion.Slerp(_legR.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (_armL != null) _armL.localRotation = Quaternion.Slerp(_armL.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (_armR != null) _armR.localRotation = Quaternion.Slerp(_armR.localRotation, Quaternion.identity, Time.deltaTime * 10f);
                if (_torso != null) _torso.localRotation = Quaternion.Slerp(_torso.localRotation, _torsoInitialLocalRot, Time.deltaTime * 8f);

                // Subtle idle head look
                if (_head != null)
                {
                    float headYaw = Mathf.Sin(Time.time * 0.8f) * 6f;
                    _head.localRotation = Quaternion.Euler(0f, headYaw, 0f);
                }
            }
        }
        else
        {
            // Airborne / Jumping pose: legs slightly tucked, arms raised for balance
            float verticalVelocity = _controller != null ? _controller.velocity.y : 0f;
            float jumpTilt = verticalVelocity > 0f ? -15f : 20f;

            if (_legL != null) _legL.localRotation = Quaternion.Slerp(_legL.localRotation, Quaternion.Euler(-25f, 0f, -8f), Time.deltaTime * 8f);
            if (_legR != null) _legR.localRotation = Quaternion.Slerp(_legR.localRotation, Quaternion.Euler(-18f, 0f, 8f), Time.deltaTime * 8f);
            if (_armL != null) _armL.localRotation = Quaternion.Slerp(_armL.localRotation, Quaternion.Euler(45f, 0f, 25f), Time.deltaTime * 8f);
            if (_armR != null) _armR.localRotation = Quaternion.Slerp(_armR.localRotation, Quaternion.Euler(45f, 0f, -25f), Time.deltaTime * 8f);
            if (_torso != null) _torso.localRotation = Quaternion.Slerp(_torso.localRotation, Quaternion.Euler(jumpTilt, 0f, 0f), Time.deltaTime * 8f);
        }

        // Apply landing squash & stretch to hips
        Vector3 targetScale = new Vector3(1f / Mathf.Sqrt(_squashFactor), _squashFactor, 1f / Mathf.Sqrt(_squashFactor));
        _hips.localScale = Vector3.Lerp(_hips.localScale, targetScale, Time.deltaTime * 15f);
    }

    private void HandleLanded()
    {
        // Impact squash
        _squashFactor = 0.76f;
    }

    #endregion

    #region Public Configuration Helper

    public void SetupBones(Transform hips, Transform torso, Transform legL, Transform legR, Transform armL, Transform armR, Transform head)
    {
        _hips = hips;
        _torso = torso;
        _legL = legL;
        _legR = legR;
        _armL = armL;
        _armR = armR;
        _head = head;

        if (_hips != null) _hipsInitialLocalPos = _hips.localPosition;
        if (_torso != null) _torsoInitialLocalRot = _torso.localRotation;
        if (_head != null) _headInitialLocalRot = _head.localRotation;
    }

    #endregion
}
