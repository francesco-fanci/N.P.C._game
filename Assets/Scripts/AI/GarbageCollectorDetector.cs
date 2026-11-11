using UnityEngine;

/// <summary>
/// Attached to the child detection trigger volume of a Garbage Collector.
/// When the player enters the red spotlight beam, triggers the FSM Purge routine.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GarbageCollectorDetector : MonoBehaviour
{
    [SerializeField]
    private GarbageCollectorFSM _fsm;

    private void Awake()
    {
        if (_fsm == null)
        {
            _fsm = GetComponentInParent<GarbageCollectorFSM>();
        }

        // Ensure collider is marked as trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fsm == null) return;

        // Check if player
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _fsm.OnPlayerDetected(other.gameObject);
        }
    }
}
