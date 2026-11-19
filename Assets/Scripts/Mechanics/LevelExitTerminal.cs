// N.P.C. (Null Pointer Character) - Terminal to transition to the next level/sector
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitTerminal : MonoBehaviour
{
    [Header("Next Level Settings")]
    [SerializeField, Tooltip("Scene to load upon terminal activation.")]
    private string _nextSceneName = "Level02_Sector02";

    [Header("Visual Effects")]
    [SerializeField, Tooltip("Core transform to rotate and hover.")]
    private Transform _coreVisual;

    [SerializeField, Tooltip("Rotation speed in degrees per second.")]
    private float _rotationSpeed = 40f;

    [SerializeField, Tooltip("Vertical bobbing speed.")]
    private float _bobSpeed = 2f;

    [SerializeField, Tooltip("Vertical bobbing amplitude.")]
    private float _bobHeight = 0.12f;

    [Header("Prompts & Feedback")]
    [SerializeField]
    private string _interactionPrompt = "[E] Access Datacenter Core // Enter Next Level";

    [SerializeField]
    private string _completionBanner = "SECTOR 01: BROKEN VILLAGE ESCAPED\nENTERING SECTOR 02: DATACENTER CORE";

    private bool _playerInRange = false;
    private bool _hasActivated = false;
    private Vector3 _startCorePos;

    private void Start()
    {
        if (_coreVisual != null)
        {
            _startCorePos = _coreVisual.localPosition;
        }
    }

    private void Update()
    {
        if (_coreVisual != null)
        {
            _coreVisual.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
            float newY = _startCorePos.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            _coreVisual.localPosition = new Vector3(_startCorePos.x, newY, _startCorePos.z);
        }

        if (_playerInRange && !_hasActivated && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(CO_ActivateExit());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasActivated) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = true;
            ItemDuplication.BlockDuplication();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowPrompt(_interactionPrompt);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = false;
            ItemDuplication.AllowDuplication();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.HidePrompt();
            }
        }
    }

    private void OnDisable()
    {
        if (_playerInRange)
        {
            ItemDuplication.AllowDuplication();
        }
    }

    private IEnumerator CO_ActivateExit()
    {
        _hasActivated = true;
        _playerInRange = false;
        ItemDuplication.AllowDuplication();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.HidePrompt();
            HUDManager.Instance.ShowSectorComplete(_completionBanner, 6f);
            HUDManager.Instance.SetObjective("DATASTREAM SECURED: Transferring consciousness to Sector 02...");
        }

        Debug.Log("[LevelExitTerminal] Terminal activated! Loading scene: " + _nextSceneName);

        yield return new WaitForSeconds(1.2f);

        if (!string.IsNullOrEmpty(_nextSceneName) && Application.CanStreamedLevelBeLoaded(_nextSceneName))
        {
            SceneManager.LoadScene(_nextSceneName);
        }
        else
        {
            Debug.LogWarning($"[LevelExitTerminal] Scene \"{_nextSceneName}\" cannot be streamed. Falling back to MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
