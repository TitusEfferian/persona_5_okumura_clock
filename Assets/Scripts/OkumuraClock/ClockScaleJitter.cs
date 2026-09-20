using System;
using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Clock Scale Jitter")]
[RequireComponent(typeof(RectTransform))]
public class ClockScaleJitter : MonoBehaviour
{
    [Tooltip("Required. Countdown whose elapsed time schedules the jitter. Assign it in the Inspector.")]
    [SerializeField]
    private CountdownClock _clock;

    [Tooltip("Number of new random scales rolled per second of countdown time. Each value is held until the next change.")]
    [Min(0.01f)]
    [SerializeField]
    private float _changesPerSecond = 30f;

    [Tooltip("Smallest uniform scale multiplier applied to the base scale.")]
    [Min(0f)]
    [SerializeField]
    private float _minMultiplier = 0.7f;

    [Tooltip("Largest uniform scale multiplier applied to the base scale.")]
    [Min(0f)]
    [SerializeField]
    private float _maxMultiplier = 1.2f;

    [Tooltip("Also jitter the scale during the first second, before the countdown has ticked. Off by default, so the first scale change lands one second in, with the first visible hand step. While waiting, the base scale is restored.")]
    [SerializeField]
    private bool _jitterBeforeFirstTick = false;

    private RectTransform _rectTransform;
    private Vector3 _baseScale;
    private int _lastIndex;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _baseScale = _rectTransform.localScale;

        if (_clock != null)
            return;

        throw new InvalidOperationException($"{nameof(ClockScaleJitter)} on '{name}' requires a CountdownClock assigned in the Inspector.");
    }

    private void OnEnable()
    {
        _lastIndex = -1;
    }

    private void OnDisable()
    {
        _rectTransform.localScale = _baseScale;
    }

    private void LateUpdate()
    {
        if (!_clock.IsRunning || IsWaitingForFirstTick())
        {
            RestoreBaseScale();
            return;
        }

        int index = Mathf.FloorToInt(_clock.Elapsed * _changesPerSecond);

        if (index == _lastIndex)
            return;

        _lastIndex = index;

        float multiplier = UnityEngine.Random.Range(_minMultiplier, _maxMultiplier);

        _rectTransform.localScale = new Vector3(_baseScale.x * multiplier, _baseScale.y * multiplier, _baseScale.z);
    }

    private bool IsWaitingForFirstTick()
    {
        return !_jitterBeforeFirstTick && _clock.SecondsRemaining == _clock.DurationSeconds;
    }

    private void RestoreBaseScale()
    {
        if (_lastIndex == -1)
            return;

        _lastIndex = -1;
        _rectTransform.localScale = _baseScale;
    }
}
