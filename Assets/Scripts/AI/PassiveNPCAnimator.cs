using UnityEngine;

/// <summary>
/// Adds lively idle animations to passive medieval NPCs (e.g. Barkeep wiping a mug,
/// shifting weight, breathing) without requiring external animation clips.
/// </summary>
public class PassiveNPCAnimator : MonoBehaviour
{
    [Header("Bones")]
    [SerializeField] private Transform _torso;
    [SerializeField] private Transform _armL;
    [SerializeField] private Transform _armR;
    [SerializeField] private Transform _head;

    private Quaternion _torsoInit;
    private Quaternion _armLInit;
    private Quaternion _armRInit;
    private Quaternion _headInit;

    private void Awake()
    {
        if (_torso != null) _torsoInit = _torso.localRotation;
        if (_armL != null) _armLInit = _armL.localRotation;
        if (_armR != null) _armRInit = _armR.localRotation;
        if (_head != null) _headInit = _head.localRotation;
    }

    private void Update()
    {
        float t = Time.time;

        // Subtle breathing & torso sway
        if (_torso != null)
        {
            float sway = Mathf.Sin(t * 1.5f) * 2f;
            _torso.localRotation = _torsoInit * Quaternion.Euler(sway, 0f, 0f);
        }

        // Arm polishing / holding mug
        if (_armR != null)
        {
            float wipe = Mathf.Sin(t * 3.5f) * 12f;
            _armR.localRotation = _armRInit * Quaternion.Euler(wipe, wipe * 0.5f, 0f);
        }

        if (_armL != null)
        {
            float hold = Mathf.Sin(t * 1.5f) * 3f;
            _armL.localRotation = _armLInit * Quaternion.Euler(hold, 0f, 0f);
        }

        // Subtle head tilt / look
        if (_head != null)
        {
            float headTurn = Mathf.Sin(t * 0.7f) * 8f;
            _head.localRotation = _headInit * Quaternion.Euler(0f, headTurn, 0f);
        }
    }

    public void SetupBones(Transform torso, Transform armL, Transform armR, Transform head)
    {
        _torso = torso;
        _armL = armL;
        _armR = armR;
        _head = head;

        if (_torso != null) _torsoInit = _torso.localRotation;
        if (_armL != null) _armLInit = _armL.localRotation;
        if (_armR != null) _armRInit = _armR.localRotation;
        if (_head != null) _headInit = _head.localRotation;
    }
}
