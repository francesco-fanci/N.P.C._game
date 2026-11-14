// N.P.C. (Null Pointer Character) - Collectible crash log pickup
// Rotates and bobs in the world, press E to read the developer note

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CrashLogCollectible : MonoBehaviour
{
    [Header("Crash Log Content")]
    [SerializeField]
    private string _logTitle = "CRASH_LOG_2012_SECTOR01.TXT";

    [TextArea(5, 10)]
    [SerializeField]
    private string _logContent =
        "// DEVELOPER MEMO — NOV 2012\n" +
        "// ISSUE #4092: UNTEXTURED STONE SEAM IN NORTH ALLEY\n\n" +
        "\"The physics collider on the north perimeter wall failed to bake into the mesh.\n" +
        "Any entity moving against it at Z > 35 will bypass boundary checks and phase\n" +
        "directly into the unallocated server backstage.\n\n" +
        "LEAVE AS A KNOWN BUG FOR NOW. WE WILL FIX IN PATCH 1.04.\"\n" +
        "-- Lead Engine Programmer";

    [Header("Visual Animation")]
    [SerializeField] private float _rotationSpeed = 60f;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _bobHeight = 0.15f;

    private Vector3 _startPos;
    private bool _playerInRange = false;
    private bool _isOpen = false;

    private void Awake()
    {
        // Make sure the collider is set as trigger for detection
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Idle animation: spin and bob up/down
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
        float newY = _startPos.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
        transform.position = new Vector3(_startPos.x, newY, _startPos.z);

        // Open/close the log when E is pressed
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleLog();
        }
    }

    private void ToggleLog()
    {
        _isOpen = !_isOpen;

        if (HUDManager.Instance != null)
        {
            if (_isOpen)
            {
                HUDManager.Instance.ShowCrashLog(_logTitle, _logContent);
                HUDManager.Instance.ShowPrompt("[E] Close Crash Log");
                HUDManager.Instance.SetObjective("EXPLOIT ACQUIRED: Stone wall at Z > 35 has unbaked collision. Phase through it into the backstage.");
            }
            else
            {
                HUDManager.Instance.HideCrashLog();
                HUDManager.Instance.ShowPrompt($"[E] Inspect {_logTitle}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = true;
            ItemDuplication.BlockDuplication();

            if (HUDManager.Instance != null && !_isOpen)
            {
                HUDManager.Instance.ShowPrompt($"[E] Inspect {_logTitle}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = false;
            ItemDuplication.AllowDuplication();

            // Close the log if it was open when the player walks away
            if (_isOpen)
            {
                _isOpen = false;
                if (HUDManager.Instance != null) HUDManager.Instance.HideCrashLog();
            }

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.HidePrompt();
            }
        }
    }

    private void OnDisable()
    {
        _playerInRange = false;
        ItemDuplication.AllowDuplication();

        if (_isOpen && HUDManager.Instance != null)
        {
            _isOpen = false;
            HUDManager.Instance.HideCrashLog();
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.HidePrompt();
        }
    }
}
