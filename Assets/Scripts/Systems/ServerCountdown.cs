// N.P.C. (Null Pointer Character) - Server wipe countdown timer (diegetic game clock)
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ServerCountdown : MonoBehaviour
{
    public static ServerCountdown Instance;

    [Header("Timer Settings")]
    [SerializeField, Tooltip("Total countdown duration in seconds (1200 = 20 minutes).")]
    private float _startTimeSeconds = 1200f;

    [Header("Game Over")]
    [SerializeField, Tooltip("Optional scene to load when timer expires.")]
    private string _gameOverSceneName = "";

    [Header("Events")]
    [SerializeField, Tooltip("Event fired when the countdown reaches zero.")]
    private UnityEvent _onServerWiped = new UnityEvent();

    private float _remainingTime;
    private bool _isPaused;
    private bool _hasExpired;

    public float RemainingTime
    {
        get { return _remainingTime; }
    }

    public string FormattedTime
    {
        get
        {
            float clamped = Mathf.Max(_remainingTime, 0f);
            int minutes = Mathf.FloorToInt(clamped / 60f);
            int seconds = Mathf.FloorToInt(clamped % 60f);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public string DisplayText
    {
        get { return "SERVER WIPE IN " + FormattedTime; }
    }

    public bool IsPaused
    {
        get { return _isPaused; }
    }

    public bool HasExpired
    {
        get { return _hasExpired; }
    }

    public float NormalizedTime
    {
        get { return Mathf.Clamp01(_remainingTime / _startTimeSeconds); }
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
        if (_hasExpired || _isPaused) return;

        _remainingTime -= Time.deltaTime;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            _hasExpired = true;
            OnTimerExpired();
        }
    }

    public void Pause()
    {
        _isPaused = true;
    }

    public void Resume()
    {
        _isPaused = false;
    }

    public void ResetTimer()
    {
        _remainingTime = _startTimeSeconds;
        _hasExpired = false;
    }

    public void AddTime(float seconds)
    {
        _remainingTime = Mathf.Clamp(_remainingTime + seconds, 0f, _startTimeSeconds);
    }

    private void OnTimerExpired()
    {
        _onServerWiped?.Invoke();

        if (!string.IsNullOrEmpty(_gameOverSceneName))
        {
            SceneManager.LoadScene(_gameOverSceneName);
        }
    }
}
