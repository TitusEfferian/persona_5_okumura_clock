using System;
using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Countdown Clock")]
public class CountdownClock : MonoBehaviour
{
    [Tooltip("Number of seconds the countdown runs, from the first beat to the last.")]
    [Min(1)]
    [SerializeField]
    private int _durationSeconds = 30;

    [Tooltip("Start the countdown as soon as the scene starts.")]
    [SerializeField]
    private bool _playOnStart = true;

    [Tooltip("Advance with unscaled time so the countdown ignores Time.timeScale.")]
    [SerializeField]
    private bool _useUnscaledTime = false;

    private float _elapsed;
    private int _lastRemaining;

    public event Action<int> Beat;
    public event Action Finished;

    public int DurationSeconds => _durationSeconds;

    public float Elapsed => _elapsed;

    public bool IsRunning { get; private set; }

    public int SecondsRemaining => Mathf.Max(0, _durationSeconds - Mathf.FloorToInt(_elapsed));

    public bool UsesUnscaledTime => _useUnscaledTime;

    public float DeltaTime => _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private void Start()
    {
        if (_playOnStart)
            Restart();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        _elapsed += DeltaTime;

        int remaining = SecondsRemaining;

        if (remaining == _lastRemaining)
            return;

        _lastRemaining = remaining;
        Beat?.Invoke(remaining);

        if (remaining > 0)
            return;

        IsRunning = false;
        Finished?.Invoke();
    }

    public void Restart()
    {
        _elapsed = 0f;
        _lastRemaining = _durationSeconds;
        IsRunning = true;
        Beat?.Invoke(_durationSeconds);
    }

    public void Stop()
    {
        IsRunning = false;
    }
}
