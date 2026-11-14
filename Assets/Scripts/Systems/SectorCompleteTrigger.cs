// N.P.C. (Null Pointer Character) - End of level trigger, displays sector escaped banner
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SectorCompleteTrigger : MonoBehaviour
{
    [Header("Sector Info")]
    [SerializeField]
    private string _bannerText = "SECTOR 01: THE BROKEN VILLAGE ESCAPED\nDATA EXPORTED TO WIREFRAME BACKSTAGE";

    [SerializeField, Tooltip("Whether this sector completion has already triggered.")]
    private bool _triggered = false;

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
        if (_triggered) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _triggered = true;

            Debug.Log("[N.P.C.] Sector 01 completed!");

            // Display victory banner and update objective in HUD
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowSectorComplete(_bannerText);
                HUDManager.Instance.SetObjective("SECTOR 01 COMPLETE: Unallocated backstage reached. Proceed to Sector 02.");
            }

            // Save checkpoint on the exit platform
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetCheckpoint(transform.position + Vector3.up * 1f, transform.rotation);
            }
        }
    }
}
