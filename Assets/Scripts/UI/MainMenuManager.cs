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
            }
        }
    }

    // Switches from the intro splash screen to the button dashboard
    public void TransitionToMenu()
    {
        _inSplashState = false;

        if (_splashPanel != null)
        {
            _splashPanel.SetActive(false);
        }

        if (_menuPanel != null)
        {
            _menuPanel.SetActive(true);
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
