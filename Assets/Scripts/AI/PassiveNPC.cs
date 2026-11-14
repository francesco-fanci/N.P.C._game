// N.P.C. (Null Pointer Character) - Passive NPC that shows corrupted dialogue when spoken to
using UnityEngine;

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

            // Block item duplication while near an NPC
            ItemDuplication.BlockDuplication();

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowPrompt($"[E] Talk to {_npcName}");
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
        _playerInRange = false;

        ItemDuplication.AllowDuplication();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.HidePrompt();
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
