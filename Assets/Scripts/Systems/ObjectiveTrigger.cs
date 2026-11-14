// N.P.C. (Null Pointer Character) - Trigger volume that updates the HUD objective tracker
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective")]
    [TextArea(2, 4)]
    [SerializeField, Tooltip("Text shown on the HUD Objective Tracker.")]
    private string _objectiveText = "OBJECTIVE: Advance forward.";

    [SerializeField, Tooltip("If true, activates only once.")]
    private bool _oneShot = true;

    private bool _hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered && _oneShot) return;

        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.GetComponent<PlayerController>() != null)
        {
            _hasTriggered = true;

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.SetObjective(_objectiveText);
            }
        }
    }

    // Set objective text from script
    public void SetText(string newText)
    {
        _objectiveText = newText;
    }
}
