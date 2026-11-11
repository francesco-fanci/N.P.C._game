using UnityEngine;

/// <summary>
/// Trigger volume that updates the HUD Objective Tracker when the player enters.
/// Place along progression pathways to guide the player through the level.
/// </summary>
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

    /// <summary>
    /// Allows changing the objective text programmatically.
    /// </summary>
    public void SetText(string newText)
    {
        _objectiveText = newText;
    }
}
