// N.P.C. (Null Pointer Character) - In-game HUD Manager (timer, RAM meter, prompts, modals)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Server Countdown (Top Right)")]
    [SerializeField, Tooltip("Text displaying 'SERVER WIPE IN MM:SS'.")]
    private TextMeshProUGUI _countdownText;

    [SerializeField, Tooltip("Color of the countdown text.")]
    private Color _countdownNormalColor = new Color(1f, 0.25f, 0.25f, 1f);

    [SerializeField, Tooltip("Flash color when under 1 minute.")]
    private Color _countdownUrgentColor = Color.red;

    [Header("Memory Leak (Top Left)")]
    [SerializeField, Tooltip("Slider or fill bar showing RAM usage.")]
    private Slider _ramSlider;

    [SerializeField, Tooltip("Text showing RAM usage percentage.")]
    private TextMeshProUGUI _ramUsageText;

    [SerializeField, Tooltip("Text showing active crates (e.g. 'Active Crates: 3/5').")]
    private TextMeshProUGUI _crateCounterText;

    [SerializeField, Tooltip("Warning banner text (e.g. 'MEM_LEAK: OOM WARN').")]
    private TextMeshProUGUI _warningBannerText;

    [Header("RAM Meter Tuning")]
    [SerializeField, Tooltip("Base RAM percentage with 0 crates.")]
    private float _baseRamPercent = 18f;

    [SerializeField, Tooltip("Additional RAM percentage per spawned crate.")]
    private float _ramPerCrate = 16f;

    [SerializeField, Tooltip("Max crates allowed (matches ItemDuplication cap).")]
    private int _maxCrates = 5;

    [Header("Reticle & Prompts")]
    [SerializeField, Tooltip("Center crosshair or reticle dot.")]
    private GameObject _centerReticle;

    [SerializeField, Tooltip("Contextual action prompt (e.g. '[E] Interact / Duplicate Crate').")]
    private TextMeshProUGUI _actionPromptText;

    [Header("Corrupted Dialogue Popup")]
    [SerializeField, Tooltip("Panel for corrupted NPC dialogue.")]
    private GameObject _dialoguePanel;

    [SerializeField, Tooltip("Text inside the corrupted dialogue panel.")]
    private TextMeshProUGUI _dialogueText;

    [Header("Crash Log Modal (Lore)")]
    [SerializeField, Tooltip("Popup modal window for developer crash logs.")]
    private GameObject _crashLogPanel;

    [SerializeField, Tooltip("Title text of crash log.")]
    private TextMeshProUGUI _crashLogTitleText;

    [SerializeField, Tooltip("Body text of crash log.")]
    private TextMeshProUGUI _crashLogBodyText;

    [Header("Sector Victory / Transition Banner")]
    [SerializeField, Tooltip("Banner shown when escaping a sector.")]
    private GameObject _sectorCompleteBanner;

    [SerializeField, Tooltip("Text in the sector complete banner.")]
    private TextMeshProUGUI _sectorCompleteText;

    [Header("Objective Tracker (Top Center)")]
    [SerializeField, Tooltip("Container panel for the active objective.")]
    private GameObject _objectivePanel;

    [SerializeField, Tooltip("Text displaying current objective / protocol.")]
    private TextMeshProUGUI _objectiveText;

    [Header("Pause Menu Modal (Esc)")]
    [SerializeField, Tooltip("Modal window for the pause / debug menu.")]
    private GameObject _pauseMenuPanel;

    [SerializeField, Tooltip("Optional sub-panel displaying keyboard controls & exploit guide.")]
    private GameObject _controlsSubPanel;

    [Header("Prologue Intro Modal")]
    [SerializeField, Tooltip("Modal window shown on level start to introduce world lore.")]
    private GameObject _prologueModal;

    [SerializeField, Tooltip("Show the prologue intro modal on first start.")]
    private bool _showPrologueOnStart = true;

    private static bool _hasShownPrologueThisSession = false;

    private bool _isPaused = false;
    private const string DefaultPrompt = "[E] Duplicate Crate / Exploit";

    public bool IsPaused
    {
        get { return _isPaused; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        ItemDuplication.OnCrateDuplicated += HandleCrateCountChanged;
    }

    private void OnDisable()
    {
        ItemDuplication.OnCrateDuplicated -= HandleCrateCountChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        UpdateCrateAndRamDisplay(0);

        if (_actionPromptText != null)
        {
            _actionPromptText.text = DefaultPrompt;
        }

        if (_objectiveText != null && string.IsNullOrEmpty(_objectiveText.text))
        {
            SetObjective("PROTOCOL 01: Find an exploit to bypass village perimeter before server wipe.");
        }

        if (_showPrologueOnStart && _prologueModal != null && !_hasShownPrologueThisSession)
        {
            _hasShownPrologueThisSession = true;
            StartCoroutine(CO_ShowPrologueDelayed());
        }
    }

    private void Update()
    {
        if (_prologueModal != null && _prologueModal.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                ClosePrologue();
            }
            return;
        }

        UpdateCountdownDisplay();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // Update the server wipe timer in the top-right
    private void UpdateCountdownDisplay()
    {
        if (_countdownText == null) return;

        if (ServerCountdown.Instance != null)
        {
            _countdownText.text = ServerCountdown.Instance.DisplayText;

            // Flash red if remaining time is under 1 minute
            if (ServerCountdown.Instance.RemainingTime <= 60f)
            {
                float t = Mathf.PingPong(Time.time * 3f, 1f);
                _countdownText.color = Color.Lerp(_countdownNormalColor, _countdownUrgentColor, t);
            }
            else
            {
                _countdownText.color = _countdownNormalColor;
            }
        }
        else
        {
            _countdownText.text = "SERVER WIPE IN 20:00";
        }
    }

    private void HandleCrateCountChanged(int currentCount)
    {
        UpdateCrateAndRamDisplay(currentCount);
    }

    // Updates the RAM bar and active crates counter based on current active crates
    public void UpdateCrateAndRamDisplay(int crateCount)
    {
        float ramPercent = Mathf.Clamp(_baseRamPercent + (crateCount * _ramPerCrate), 0f, 100f);

        if (_ramSlider != null)
        {
            _ramSlider.value = ramPercent / 100f;
        }

        if (_ramUsageText != null)
        {
            _ramUsageText.text = "RAM USAGE: " + Mathf.RoundToInt(ramPercent) + "%";
        }

        if (_crateCounterText != null)
        {
            _crateCounterText.text = "Active Crates: " + crateCount + "/" + _maxCrates;
        }

        if (_warningBannerText != null)
        {
            if (crateCount >= _maxCrates)
            {
                _warningBannerText.text = "MEM_LEAK: OOM CRITICAL (MAX CAP)";
                _warningBannerText.color = Color.red;
                _warningBannerText.gameObject.SetActive(true);
            }
            else if (crateCount >= 3)
            {
                _warningBannerText.text = "MEM_LEAK: OOM WARN";
                _warningBannerText.color = new Color(1f, 0.6f, 0.2f);
                _warningBannerText.gameObject.SetActive(true);
            }
            else
            {
                _warningBannerText.text = "STATUS: MEMORY_LEAK_TRACKED";
                _warningBannerText.color = new Color(0.2f, 0.9f, 0.9f);
                _warningBannerText.gameObject.SetActive(true);
            }
        }
    }

    // --- Action Prompts ---

    public void ShowPrompt(string promptText)
    {
        if (_actionPromptText != null)
        {
            _actionPromptText.text = promptText;
            _actionPromptText.gameObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (_actionPromptText != null)
        {
            _actionPromptText.text = DefaultPrompt;
            _actionPromptText.gameObject.SetActive(true);
        }
    }

    // Backward-compatible helpers for callers passing a source
    public void SetPrompt(string promptText, object source = null)
    {
        ShowPrompt(promptText);
    }

    public void ClearPrompt(object source = null)
    {
        HidePrompt();
    }

    // --- Corrupted NPC Dialogue Popup ---

    public void ShowCorruptedDialogue(string message = "Error 404: DialogueString Missing\n[Uncaught NullPointerException]", float duration = 4f)
    {
        if (_dialoguePanel != null && _dialogueText != null)
        {
            _dialogueText.text = message;
            _dialoguePanel.SetActive(true);

            CancelInvoke(nameof(HideDialogue));
            Invoke(nameof(HideDialogue), duration);
        }
    }

    public void HideDialogue()
    {
        if (_dialoguePanel != null)
        {
            _dialoguePanel.SetActive(false);
        }
    }

    // --- Crash Log Modal ---

    public void ShowCrashLog(string title, string content)
    {
        if (_crashLogPanel != null)
        {
            if (_crashLogTitleText != null) _crashLogTitleText.text = title;
            if (_crashLogBodyText != null) _crashLogBodyText.text = content;
            _crashLogPanel.SetActive(true);
        }
    }

    public void HideCrashLog()
    {
        if (_crashLogPanel != null)
        {
            _crashLogPanel.SetActive(false);
        }
    }

    // --- Sector Complete Banner ---

    public void ShowSectorComplete(string message = "SECTOR 01 ESCAPED\nENTERING WIREFRAME BACKSTAGE", float duration = 5f)
    {
        if (_sectorCompleteBanner != null && _sectorCompleteText != null)
        {
            _sectorCompleteText.text = message;
            _sectorCompleteBanner.SetActive(true);

            CancelInvoke(nameof(HideSectorComplete));
            if (duration > 0f)
            {
                Invoke(nameof(HideSectorComplete), duration);
            }
        }
    }

    public void HideSectorComplete()
    {
        if (_sectorCompleteBanner != null)
        {
            _sectorCompleteBanner.SetActive(false);
        }
    }

    // --- Objective Tracker ---

    public void SetObjective(string objective)
    {
        if (_objectiveText != null)
        {
            _objectiveText.text = objective;
        }
        if (_objectivePanel != null)
        {
            _objectivePanel.SetActive(!string.IsNullOrEmpty(objective));
        }
    }

    // --- Pause Menu ---

    public void TogglePause()
    {
        SetPaused(!_isPaused);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void SetPaused(bool pause)
    {
        _isPaused = pause;
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.SetActive(_isPaused);
        }

        if (!_isPaused && _controlsSubPanel != null)
        {
            _controlsSubPanel.SetActive(false);
        }

        Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isPaused;
    }

    public void RespawnFromCheckpoint()
    {
        ResumeGame();
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer();
        }
    }

    public void RestartSector()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        _hasShownPrologueThisSession = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ToggleControlsModal()
    {
        if (_controlsSubPanel != null)
        {
            _controlsSubPanel.SetActive(!_controlsSubPanel.activeSelf);
        }
    }

    // --- Prologue Modal ---

    private System.Collections.IEnumerator CO_ShowPrologueDelayed()
    {
        yield return new WaitForEndOfFrame();
        ShowPrologue();
    }

    public void ShowPrologue()
    {
        if (_prologueModal != null)
        {
            _prologueModal.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ClosePrologue()
    {
        if (_prologueModal != null)
        {
            _prologueModal.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
