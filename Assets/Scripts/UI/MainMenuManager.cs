// N.P.C. (Null Pointer Character) - Main Menu Manager v2.0
// Gestisce: Splash -> Menu transition, modal panels, options, fade, audio, button juice.
using System.Collections;
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
    [SerializeField] private GameObject _optionsModal;

    // --- OPTIONS CONTROLS ---------------------------------------------------
    [Header("Options Controls")]
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private Toggle _fullscreenToggle;

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

    // --- SCENE LOADING ------------------------------------------------------
    [Header("Scene Loading")]
    [SerializeField] private string _firstLevelSceneName = "Level01_BrokenVillage";

    // --- AUDIO --------------------------------------------------------------
    [Header("Audio")]
    [SerializeField, Tooltip("AudioSource for UI SFX (hover, click).")]
    private AudioSource _sfxSource;

    [SerializeField] private AudioClip _hoverClip;
    [SerializeField] private AudioClip _clickClip;

    [SerializeField, Tooltip("AudioSource for background music.")]
    private AudioSource _musicSource;

    // --- BUTTON JUICE -------------------------------------------------------
    [Header("Button Hover Scale")]
    [SerializeField, Range(1f, 1.15f)]
    private float _hoverScale = 1.05f;

    [SerializeField, Range(0.01f, 0.2f)]
    private float _hoverAnimDuration = 0.08f;

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
        SetPanelActive(_optionsModal, false);

        // CanvasGroup init
        if (_splashCanvasGroup != null) _splashCanvasGroup.alpha = 1f;
        if (_menuCanvasGroup != null) _menuCanvasGroup.alpha = 0f;
        if (_fadeOverlay != null)
        {
            _fadeOverlay.alpha = 0f;
            _fadeOverlay.gameObject.SetActive(true);
        }

        // Options init
        if (_volumeSlider != null)
        {
            _volumeSlider.value = AudioListener.volume;
            _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.isOn = Screen.fullScreen;
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }

        // Splash pulse color
        if (_pressAnyKeyText != null)
        {
            _pulseBaseColor = _pressAnyKeyText.color;
        }

        // Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Register button hover/click events for ALL buttons in the Canvas
        RegisterButtonEvents();
    }

    private void OnDestroy()
    {
        // Cleanup listeners to avoid leaks
        if (_volumeSlider != null)
        {
            _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }

        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggled);
        }
    }

    private void Update()
    {
        if (_inSplashState)
        {
            PulsePressAnyKey();

            if (Time.time - _startTime > 0.3f && Input.anyKeyDown)
            {
                TransitionToMenu();
            }
        }
        else
        {
            HandleEscapeKey();
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

        // Fade in menu
        yield return CO_FadeCanvasGroup(_menuCanvasGroup, 0f, 1f, _fadeDuration);

        _isTransitioning = false;
    }

    // ========================================================================
    // MODAL MANAGEMENT
    // ========================================================================

    public void OpenControls() => OpenModal(_controlsModal);
    public void CloseControls() => CloseModal(_controlsModal);
    public void OpenAbout() => OpenModal(_aboutModal);
    public void CloseAbout() => CloseModal(_aboutModal);
    public void OpenOptions() => OpenModal(_optionsModal);
    public void CloseOptions() => CloseModal(_optionsModal);

    private void OpenModal(GameObject modal)
    {
        if (modal == null || _isTransitioning) return;
        PlaySFX(_clickClip);
        modal.SetActive(true);

        // Optional: fade in modal if it has a CanvasGroup
        var cg = modal.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            StartCoroutine(CO_FadeCanvasGroup(cg, 0f, 1f, _fadeDuration * 0.5f));
        }
    }

    private void CloseModal(GameObject modal)
    {
        if (modal == null) return;
        PlaySFX(_clickClip);

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
    // OPTIONS CALLBACKS
    // ========================================================================

    public void OnVolumeChanged(float value) => AudioListener.volume = value;
    public void OnFullscreenToggled(bool isOn) => Screen.fullScreen = isOn;

    // ========================================================================
    // PLAY GAME (with fade-to-black)
    // ========================================================================

    public void PlayGame()
    {
        if (_isLoadingScene) return;
        _isLoadingScene = true;
        PlaySFX(_clickClip);
        StartCoroutine(CO_FadeAndLoadScene());
    }

    private IEnumerator CO_FadeAndLoadScene()
    {
        // Fade music out (if present)
        if (_musicSource != null)
        {
            float startVol = _musicSource.volume;
            float elapsed = 0f;
            while (elapsed < _sceneTransitionDuration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / _sceneTransitionDuration);
                yield return null;
            }
        }

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
        PlaySFX(_clickClip);
        Debug.Log("[MainMenu] Exiting application...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ========================================================================
    // ESC KEY HANDLER
    // ========================================================================

    private void HandleEscapeKey()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (_controlsModal != null && _controlsModal.activeSelf)
        {
            CloseControls();
        }
        else if (_aboutModal != null && _aboutModal.activeSelf)
        {
            CloseAbout();
        }
        else if (_optionsModal != null && _optionsModal.activeSelf)
        {
            CloseOptions();
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
            // Add EventTrigger for hover scale
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

        // PointerEnter -> scale up + SFX
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((_) =>
        {
            StopCoroutine(nameof(CO_ScaleButton)); // prevent overlap
            StartCoroutine(CO_ScaleButton(btnGO.transform, Vector3.one * _hoverScale, _hoverAnimDuration));
            PlaySFX(_hoverClip);
        });
        trigger.triggers.Add(entryEnter);

        // PointerExit -> scale back
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((_) =>
        {
            StartCoroutine(CO_ScaleButton(btnGO.transform, Vector3.one, _hoverAnimDuration));
        });
        trigger.triggers.Add(entryExit);

        // PointerClick -> punch scale
        var entryClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entryClick.callback.AddListener((_) =>
        {
            StartCoroutine(CO_PunchScale(btnGO.transform));
        });
        trigger.triggers.Add(entryClick);
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

    private void PlaySFX(AudioClip clip)
    {
        if (_sfxSource != null && clip != null)
        {
            _sfxSource.PlayOneShot(clip);
        }
    }
}
