// N.P.C. (Null Pointer Character) - Detection trigger for Garbage Collector enemies
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GarbageCollectorDetector : MonoBehaviour
{
    [SerializeField]
    private GarbageCollectorFSM _fsm;

    private void Awake()
    {
        // Try to find the FSM on the parent if not assigned in the inspector
        if (_fsm == null)
        {
            _fsm = GetComponentInParent<GarbageCollectorFSM>();
        }

        // Make sure collider is a trigger so OnTriggerEnter works
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fsm == null) return;

        // Only react to the player entering the detection zone
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _fsm.OnPlayerDetected(other.gameObject);
        }
    }
}
