using UnityEngine;

/// <summary>
/// Trigger volume placed at the exit of a sector (e.g. Backstage ledge of Level 1).
/// Displays the victory / sector escape banner on the HUD and updates the checkpoint.
/// </summary>
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
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _triggered = true;

            Debug.Log("<color=green><b>[N.P.C.] *** SECTOR 01 COMPLETED! ***</b></color>");

            // Show HUD victory banner
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowSectorComplete(_bannerText);
                HUDManager.Instance.SetObjective("SECTOR 01 COMPLETE: Unallocated backstage reached. Proceed to Sector 02.");
            }

            // Set new checkpoint on the backstage platform
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.SetCheckpoint(transform.position + Vector3.up * 1f, transform.rotation);
            }
        }
    }
}
