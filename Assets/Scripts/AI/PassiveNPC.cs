using UnityEngine;

/// <summary>
/// A static NPC that loops an idle animation/state.
/// When the player approaches and presses E, displays a corrupted dialogue box:
/// "Error 404: DialogueString Missing [Uncaught NullPointerException]".
/// As specified in GDD section 5.2 / NPC Logic Flow.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PassiveNPC : MonoBehaviour
{
    [Header("NPC Setup")]
    [SerializeField, Tooltip("NPC identifier (e.g. 'Barkeep_01.npc').")]
    private string _npcName = "Barkeep_01.npc";

    [TextArea(2, 4)]
    [SerializeField, Tooltip("Corrupted error string displayed when spoken to.")]
    private string _corruptedDialogue = "Error 404: DialogueString Missing\n[Uncaught NullPointerException: Pointer 0x00FF8B4C de-allocated]";

    [SerializeField, Tooltip("Duration in seconds the dialogue popup stays visible.")]
    private float _displayDuration = 4.5f;

    private bool _playerInRange = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Speak();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = true;
            ItemDuplication.RegisterInteractionBlocker(this);
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.SetPrompt($"[E] Talk to {_npcName}", this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = false;
            ItemDuplication.UnregisterInteractionBlocker(this);
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ClearPrompt(this);
            }
        }
    }

    private void OnDisable()
    {
        _playerInRange = false;
        ItemDuplication.UnregisterInteractionBlocker(this);
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ClearPrompt(this);
        }
    }

    public void Speak()
    {
        Debug.Log($"<color=magenta>[NPC {_npcName}] Dialogue requested -> CORRUPTED_STRING_RETURNED</color>");
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowCorruptedDialogue(_corruptedDialogue, _displayDuration);
        }
    }
}
