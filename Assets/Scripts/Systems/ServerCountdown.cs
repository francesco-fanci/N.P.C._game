using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the server-wipe countdown timer for "N.P.C. (Null Pointer Character)".
/// Counts down from a configurable start time and fires an event when the server is wiped.
/// Attach to a single persistent GameObject in the scene (e.g., "GameManager").
/// </summary>
public class ServerCountdown : MonoBehaviour
{
    // ───────────────────────── Inspector Fields ─────────────────────────

    [Header("Timer Settings")]
    [SerializeField, Tooltip("Total countdown duration in seconds (default: 1200 = 20 minutes).")]
    private float _startTimeSeconds = 1200f;

    [Header("Game Over")]
    [SerializeField, Tooltip("If set, this scene is loaded when the timer reaches zero.")]
    private string _gameOverSceneName = "";

    [Header("Events")]
    [SerializeField, Tooltip("Fired once when the countdown reaches zero.")]
    private UnityEvent _onServerWiped = new UnityEvent();

    // ───────────────────────── Runtime State ────────────────────────────

    private float _remainingTime;
    private bool _isPaused;
    private bool _hasExpired;

    // ───────────────────────── Public Properties ────────────────────────

    /// <summary>Seconds left on the countdown clock.</summary>
    public float RemainingTime => _remainingTime;

    /// <summary>Countdown formatted as "MM:SS".</summary>
    public string FormattedTime
    {
        get
        {
            float clamped = Mathf.Max(_remainingTime, 0f);
            int minutes = Mathf.FloorToInt(clamped / 60f);
            int seconds = Mathf.FloorToInt(clamped % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }

    /// <summary>Full HUD string: "SERVER WIPE IN MM:SS".</summary>
    public string DisplayText => $"SERVER WIPE IN {FormattedTime}";

    /// <summary>Whether the countdown is currently paused.</summary>
    public bool IsPaused => _isPaused;

    /// <summary>Whether the timer has already reached zero.</summary>
    public bool HasExpired => _hasExpired;

    /// <summary>Normalized progress (1 = full time, 0 = expired).</summary>
    public float NormalizedTime => Mathf.Clamp01(_remainingTime / _startTimeSeconds);

    // ───────────────────────── Public API ────────────────────────────────

    /// <summary>Pause the countdown (e.g., when the game menu is open).</summary>
    public void Pause()  => _isPaused = true;

    /// <summary>Resume the countdown after a pause.</summary>
    public void Resume() => _isPaused = false;

    /// <summary>Reset the timer back to the configured start time.</summary>
    public void ResetTimer()
    {
        _remainingTime = _startTimeSeconds;
        _hasExpired = false;
    }

    /// <summary>Add (or subtract) seconds from the remaining time.</summary>
    /// <param name="seconds">Positive to add time, negative to subtract.</param>
    public void AddTime(float seconds)
    {
        _remainingTime = Mathf.Clamp(_remainingTime + seconds, 0f, _startTimeSeconds);
    }

    public static ServerCountdown Instance { get; private set; }

    // ───────────────────────── Unity Callbacks ──────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _remainingTime = _startTimeSeconds;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (_hasExpired || _isPaused)
            return;

        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            _hasExpired = true;
            OnTimerExpired();
        }
    }

    // ───────────────────────── Internals ─────────────────────────────────

    /// <summary>Called exactly once when the countdown hits zero.</summary>
    private void OnTimerExpired()
    {
        _onServerWiped?.Invoke();

        if (!string.IsNullOrEmpty(_gameOverSceneName))
        {
            SceneManager.LoadScene(_gameOverSceneName);
        }
    }
}
