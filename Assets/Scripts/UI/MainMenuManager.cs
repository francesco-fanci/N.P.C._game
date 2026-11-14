// N.P.C. (Null Pointer Character) - Main Menu and Boot Splash Manager
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField, Tooltip("Introductory splash panel shown on startup ('Press Any Key to Start').")]
    private GameObject _splashPanel;

    [SerializeField, Tooltip("Main menu buttons panel (Play, Controls, Story, Quit).")]
    private GameObject _menuPanel;

    [Header("Modals")]
    [SerializeField, Tooltip("Modal window displaying keyboard controls and glitch exploits.")]
    private GameObject _controlsModal;

    [SerializeField, Tooltip("Modal window displaying story lore and project info.")]
    private GameObject _aboutModal;

    [SerializeField, Tooltip("Modal window displaying system options (volume, fullscreen).")]
    private GameObject _optionsModal;

    [Header("Options Controls")]
    [SerializeField, Tooltip("Slider for master volume.")]
    private Slider _volumeSlider;

    [SerializeField, Tooltip("Toggle for fullscreen mode.")]
    private Toggle _fullscreenToggle;

    [Header("Fade Transitions")]
    [SerializeField, Tooltip("CanvasGroup on SplashPanel for fade animation.")]
    private CanvasGroup _splashCanvasGroup;

    [SerializeField, Tooltip("CanvasGroup on MenuPanel for fade animation.")]
    private CanvasGroup _menuCanvasGroup;

    [SerializeField, Tooltip("Duration of fade transitions in seconds.")]
    private float _fadeDuration = 0.3f;

    [Header("Splash Screen Animation")]
    [SerializeField, Tooltip("Blinking text prompt on the splash screen.")]
    private TextMeshProUGUI _pressAnyKeyText;

    [SerializeField, Tooltip("Speed of the breathing/blinking pulse effect.")]
    private float _pulseSpeed = 3.5f;

    [Header("Scene Loading")]
    [SerializeField, Tooltip("Name of the first playable level scene.")]
    private string _firstLevelSceneName = "Level01_BrokenVillage";

    private bool _inSplashState = true;
    private Color _pulseColorOriginal;
    private float _startTime;

    private void Start()
    {
        _startTime = Time.time;

        // Start in splash screen state
        if (_splashPanel != null) _splashPanel.SetActive(true);
        if (_menuPanel != null) _menuPanel.SetActive(false);
        if (_controlsModal != null) _controlsModal.SetActive(false);
        if (_aboutModal != null) _aboutModal.SetActive(false);
        if (_optionsModal != null) _optionsModal.SetActive(false);

        // Initialize options UI to match current system settings
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

        // Fade setup: ensure MenuPanel starts invisible if using CanvasGroups
        if (_menuCanvasGroup != null)
        {
            _menuCanvasGroup.alpha = 0f;
        }

        if (_pressAnyKeyText != null)
        {
            _pulseColorOriginal = _pressAnyKeyText.color;
        }

        // Unlock mouse cursor for menu navigation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Phase 1: Wait for any key or mouse click to transition to the main menu
        if (_inSplashState)
        {
            // Pulsing effect on the prompt text
            if (_pressAnyKeyText != null)
            {
                float alpha = Mathf.PingPong(Time.time * _pulseSpeed, 1f);
                Color c = _pulseColorOriginal;
                c.a = Mathf.Lerp(0.25f, 1f, alpha);
                _pressAnyKeyText.color = c;
            }

            // Only accept input after a brief moment to avoid consuming the Editor Play click
            if (Time.time - _startTime > 0.2f && Input.anyKeyDown)
            {
                TransitionToMenu();
            }
        }
        else
        {
            // Close any open modal when ESC is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
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
        }
    }

    // Switches from the intro splash screen to the button dashboard with fade
    public void TransitionToMenu()
    {
        _inSplashState = false;
        StartCoroutine(FadeTransition());
    }

    private IEnumerator FadeTransition()
    {
        // Fade out splash
        if (_splashCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _splashCanvasGroup.alpha = 1f - (elapsed / _fadeDuration);
                yield return null;
            }
            _splashCanvasGroup.alpha = 0f;
        }

        if (_splashPanel != null) _splashPanel.SetActive(false);
        if (_menuPanel != null) _menuPanel.SetActive(true);

        // Fade in menu
        if (_menuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _menuCanvasGroup.alpha = elapsed / _fadeDuration;
                yield return null;
            }
            _menuCanvasGroup.alpha = 1f;
        }
    }

    // Loads the vertical slice level
    public void PlayGame()
    {
        Debug.Log("[MainMenu] Launching runtime: " + _firstLevelSceneName);
        SceneManager.LoadScene(_firstLevelSceneName);
    }

    // --- Controls Modal ---

    public void OpenControls()
    {
        if (_controlsModal != null)
        {
            _controlsModal.SetActive(true);
        }
    }

    public void CloseControls()
    {
        if (_controlsModal != null)
        {
            _controlsModal.SetActive(false);
        }
    }

    // --- About / Story Modal ---

    public void OpenAbout()
    {
        if (_aboutModal != null)
        {
            _aboutModal.SetActive(true);
        }
    }

    public void CloseAbout()
    {
        if (_aboutModal != null)
        {
            _aboutModal.SetActive(false);
        }
    }

    // --- Options Modal ---

    public void OpenOptions()
    {
        if (_optionsModal != null)
        {
            _optionsModal.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (_optionsModal != null)
        {
            _optionsModal.SetActive(false);
        }
    }

    // --- Options Callbacks ---

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    public void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Quits the application
    public void QuitGame()
    {
        Debug.Log("[MainMenu] Exiting application...");
        Application.Quit();

#if UNITY_EDITOR
        // Stop playing in the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
