// N.P.C. (Null Pointer Character) - Main Menu Manager v2.2
// Manages: Splash -> Menu transition, modal panels (Controls, About, Credits), scene fade, button juice.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // --- PANELS -------------------------------------------------------------
    [Header("Panels")]
    [SerializeField, Tooltip("Introductory splash panel.")]
    private GameObject _splashPanel;

    [SerializeField, Tooltip("Main menu buttons panel.")]
    private GameObject _menuPanel;

    // --- MODALS -------------------------------------------------------------
    [Header("Modals")]
    [SerializeField] private GameObject _controlsModal;
    [SerializeField] private GameObject _aboutModal;
    [SerializeField, Tooltip("Dedicated modal panel for Credits. If left unassigned, falls back to About modal.")]
    private GameObject _creditsModal;

    // --- FADE TRANSITIONS ---------------------------------------------------
    [Header("Fade Transitions")]
    [SerializeField, Tooltip("CanvasGroup on SplashPanel for fade.")]
    private CanvasGroup _splashCanvasGroup;

    [SerializeField, Tooltip("CanvasGroup on MenuPanel for fade.")]
    private CanvasGroup _menuCanvasGroup;

    [SerializeField, Range(0.1f, 1f)]
    private float _fadeDuration = 0.3f;

    // --- SPLASH ANIMATION ---------------------------------------------------
    [Header("Splash Screen")]
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText;
    [SerializeField] private float _pulseSpeed = 3.5f;
    [SerializeField, Range(2f, 10f), Tooltip("Seconds before automatically advancing to the main menu if no key is pressed.")]
    private float _splashAutoAdvanceDelay = 4.0f;

    // --- SCENE LOADING ------------------------------------------------------
    [Header("Scene Loading")]
    [SerializeField] private string _firstLevelSceneName = "Level01_BrokenVillage";

    // --- BUTTON JUICE -------------------------------------------------------
    [Header("Button Hover Scale")]
    [SerializeField, Range(1f, 1.15f)]
    private float _hoverScale = 1.04f;

    [SerializeField, Range(0.01f, 0.2f)]
    private float _hoverAnimDuration = 0.08f;

    // --- TERMINAL POLISH ----------------------------------------------------
    [Header("Terminal Visuals")]
    [SerializeField] private TextMeshProUGUI _menuSubtitleText;
    private string _baseSubtitleText;
    private Coroutine _cursorCoroutine;

    // --- FADE OVERLAY (for scene transition) --------------------------------
    [Header("Scene Transition Overlay")]
    [SerializeField, Tooltip("Full-screen black Image with CanvasGroup for fade-to-black. Create as last child of Canvas.")]
    private CanvasGroup _fadeOverlay;

    [SerializeField, Range(0.3f, 2f)]
    private float _sceneTransitionDuration = 0.6f;

    // --- PRIVATE STATE ------------------------------------------------------
    private bool _inSplashState = true;
    private bool _isTransitioning;
    private bool _isLoadingScene;
    private Color _pulseBaseColor;
    private float _startTime;

    // --- BUTTON SCALE COROUTINES -------------------------------------------
    private readonly Dictionary<Transform, Coroutine> _scaleCoroutines
        = new Dictionary<Transform, Coroutine>();

    // ========================================================================
    // LIFECYCLE
    // ========================================================================

    private void Start()
    {
        _startTime = Time.time;

        // Panel visibility
        SetPanelActive(_splashPanel, true);
        SetPanelActive(_menuPanel, false);
        SetPanelActive(_controlsModal, false);
        SetPanelActive(_aboutModal, false);
        if (_creditsModal != null && _creditsModal != _aboutModal)
        {
            SetPanelActive(_creditsModal, false);
        }

        // CanvasGroup init
        if (_splashCanvasGroup != null) _splashCanvasGroup.alpha = 1f;
        if (_menuCanvasGroup != null) _menuCanvasGroup.alpha = 0f;
        if (_fadeOverlay != null)
        {
            _fadeOverlay.alpha = 0f;
            _fadeOverlay.gameObject.SetActive(true);
        }

        // Splash pulse color
        if (_pressAnyKeyText != null)
        {
            _pulseBaseColor = _pressAnyKeyText.color;
        }

        // Cache base subtitle text
        if (_menuSubtitleText != null)
        {
            _baseSubtitleText = _menuSubtitleText.text;
        }

        // Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Register button hover/click events for ALL buttons in the Canvas
        RegisterButtonEvents();
    }

    private void Update()
    {
        if (_inSplashState)
        {
            PulsePressAnyKey();

            float elapsed = Time.time - _startTime;

            // Advance if user presses any key/click OR if timer expires without input
            if ((elapsed > 0.1f && (Input.anyKeyDown || Input.GetMouseButtonDown(0))) ||
                (elapsed >= _splashAutoAdvanceDelay))
            {
                TransitionToMenu();
            }
        }
        else
        {
            HandleMenuShortcuts();
        }
    }

    // ========================================================================
    // SPLASH -> MENU TRANSITION
    // ========================================================================

    public void TransitionToMenu()
    {
        if (_isTransitioning || !_inSplashState) return;
        _isTransitioning = true;
        _inSplashState = false;
        StartCoroutine(CO_FadeSplashToMenu());
    }

    private IEnumerator CO_FadeSplashToMenu()
    {
        // Fade out splash
        yield return CO_FadeCanvasGroup(_splashCanvasGroup, 1f, 0f, _fadeDuration);

        SetPanelActive(_splashPanel, false);
        SetPanelActive(_menuPanel, true);
        StartTerminalCursor();

        // Fade in menu
        yield return CO_FadeCanvasGroup(_menuCanvasGroup, 0f, 1f, _fadeDuration);

        _isTransitioning = false;
    }

    // ========================================================================
    // MODAL MANAGEMENT
    // ========================================================================

    private GameObject EffectiveCreditsModal => _creditsModal != null ? _creditsModal : _aboutModal;

    public void OpenControls() => OpenModal(_controlsModal);
    public void CloseControls() => CloseModal(_controlsModal);

    public void OpenAbout() => OpenModal(_aboutModal);
    public void CloseAbout() => CloseModal(_aboutModal);

    public void OpenCredits() => OpenModal(EffectiveCreditsModal);
    public void CloseCredits() => CloseModal(EffectiveCreditsModal);

    private void OpenModal(GameObject modal)
    {
        if (modal == null || _isTransitioning) return;
        modal.SetActive(true);

        // Reset vertical scroll position to top whenever a modal opens
        ScrollRect[] scrollRects = modal.GetComponentsInChildren<ScrollRect>(true);
        foreach (ScrollRect sr in scrollRects)
        {
            sr.verticalNormalizedPosition = 1f;
            sr.horizontalNormalizedPosition = 0f;
        }

        var cg = modal.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            StartCoroutine(CO_FadeCanvasGroup(cg, 0f, 1f, _fadeDuration * 0.5f));
        }
    }

    private void CloseModal(GameObject modal)
    {
        if (modal == null) return;

        var cg = modal.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            StartCoroutine(CO_FadeAndDeactivate(cg, modal, _fadeDuration * 0.5f));
        }
        else
        {
            modal.SetActive(false);
        }
    }

    private IEnumerator CO_FadeAndDeactivate(CanvasGroup cg, GameObject go, float duration)
    {
        yield return CO_FadeCanvasGroup(cg, 1f, 0f, duration);
        go.SetActive(false);
        cg.alpha = 1f; // Reset for next open
    }

    // ========================================================================
    // PLAY GAME (with fade-to-black)
    // ========================================================================

    public void PlayGame()
    {
        if (_isLoadingScene) return;
        _isLoadingScene = true;
        StartCoroutine(CO_FadeAndLoadScene());
    }

    private IEnumerator CO_FadeAndLoadScene()
    {
        // Fade overlay to black
        yield return CO_FadeCanvasGroup(_fadeOverlay, 0f, 1f, _sceneTransitionDuration);

        Debug.Log("[MainMenu] Launching: " + _firstLevelSceneName);
        SceneManager.LoadScene(_firstLevelSceneName);
    }

    // ========================================================================
    // QUIT
    // ========================================================================

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Exiting application...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ========================================================================
    // MENU SHORTCUTS & NAVIGATION
    // ========================================================================

    private void HandleMenuShortcuts()
    {
        if (_isLoadingScene || _isTransitioning) return;

        GameObject creditsTarget = EffectiveCreditsModal;
        bool isModalOpen = (_controlsModal != null && _controlsModal.activeSelf) ||
                           (_aboutModal != null && _aboutModal.activeSelf) ||
                           (creditsTarget != null && creditsTarget.activeSelf);

        if (isModalOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_controlsModal != null && _controlsModal.activeSelf) CloseControls();
                else if (creditsTarget != null && creditsTarget.activeSelf && creditsTarget != _aboutModal) CloseCredits();
                else if (_aboutModal != null && _aboutModal.activeSelf) CloseAbout();
            }
            return;
        }

        // Terminal hotkeys [01] - [04]
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            PlayGame();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            OpenControls();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            OpenAbout();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    private void StartTerminalCursor()
    {
        if (_cursorCoroutine != null) StopCoroutine(_cursorCoroutine);
        if (_menuSubtitleText != null)
        {
            _cursorCoroutine = StartCoroutine(CO_TerminalCursor());
        }
    }

    private IEnumerator CO_TerminalCursor()
    {
        bool cursorOn = false;
        while (true)
        {
            cursorOn = !cursorOn;
            if (_menuSubtitleText != null)
            {
                _menuSubtitleText.text = cursorOn
                    ? _baseSubtitleText + " <color=#00F0FF>_</color>"
                    : _baseSubtitleText + " <color=#00000000>_</color>";
            }
            yield return new WaitForSecondsRealtime(0.53f);
        }
    }

    // ========================================================================
    // SPLASH PULSE ANIMATION
    // ========================================================================

    private void PulsePressAnyKey()
    {
        if (_pressAnyKeyText == null) return;
        float alpha = Mathf.PingPong(Time.time * _pulseSpeed, 1f);
        Color c = _pulseBaseColor;
        c.a = Mathf.Lerp(0.25f, 1f, alpha);
        _pressAnyKeyText.color = c;
    }

    // ========================================================================
    // BUTTON HOVER/CLICK JUICE
    // ========================================================================

    private void RegisterButtonEvents()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            AddHoverEvent(btn.gameObject);
        }
    }

    private void AddHoverEvent(GameObject btnGO)
    {
        EventTrigger trigger = btnGO.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = btnGO.AddComponent<EventTrigger>();
        }
        trigger.triggers.Clear();

        Transform t = btnGO.transform;

        // PointerEnter -> scale up
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((_) =>
        {
            StartButtonScale(t, Vector3.one * _hoverScale, _hoverAnimDuration);
        });
        trigger.triggers.Add(entryEnter);

        // PointerExit -> scale back
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) =>
        {
            StartButtonScale(t, Vector3.one, _hoverAnimDuration);
        });
        trigger.triggers.Add(entryExit);

        // PointerClick -> punch scale
        var entryClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entryClick.callback.AddListener((_) =>
        {
            StartButtonPunch(t);
        });
        trigger.triggers.Add(entryClick);
    }

    private void StartButtonScale(Transform t, Vector3 target, float duration)
    {
        if (_scaleCoroutines.TryGetValue(t, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
        }
        _scaleCoroutines[t] = StartCoroutine(CO_ScaleButton(t, target, duration));
    }

    private void StartButtonPunch(Transform t)
    {
        if (_scaleCoroutines.TryGetValue(t, out Coroutine existing) && existing != null)
        {
            StopCoroutine(existing);
        }
        _scaleCoroutines[t] = StartCoroutine(CO_PunchScale(t));
    }

    private IEnumerator CO_ScaleButton(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        t.localScale = target;
    }

    private IEnumerator CO_PunchScale(Transform t)
    {
        Vector3 punchTarget = Vector3.one * 0.93f;
        yield return CO_ScaleButton(t, punchTarget, 0.05f);
        yield return CO_ScaleButton(t, Vector3.one * _hoverScale, 0.08f);
    }

    // ========================================================================
    // UTILITY
    // ========================================================================

    private IEnumerator CO_FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}