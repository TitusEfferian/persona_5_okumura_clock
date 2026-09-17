using System;
using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Beat Step Rotator")]
[RequireComponent(typeof(RectTransform))]
public class BeatStepRotator : MonoBehaviour
{
    [Tooltip("Required. Countdown whose beats drive the steps. Assign it in the Inspector.")]
    [SerializeField]
    private CountdownClock _clock;

    [Tooltip("Local Z rotation, in degrees, while the full duration remains.")]
    [SerializeField]
    private float _startAngle = -180f;

    [Tooltip("Degrees added for each elapsed second. Negative turns clockwise on screen.")]
    [SerializeField]
    private float _degreesPerSecond = -6f;

    private RectTransform _rectTransform;

    public CountdownClock Clock => _clock;

    public float StartAngle => _startAngle;

    public float DegreesPerSecond => _degreesPerSecond;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;

        if (_clock != null)
            return;

        throw new InvalidOperationException($"{nameof(BeatStepRotator)} on '{name}' requires a CountdownClock assigned in the Inspector.");
    }

    private void OnEnable()
    {
        _clock.Beat += OnBeat;
        OnBeat(_clock.SecondsRemaining);
    }

    private void OnDisable()
    {
        _clock.Beat -= OnBeat;
    }

    private void OnBeat(int secondsRemaining)
    {
        float angle = _startAngle + (_clock.DurationSeconds - secondsRemaining) * _degreesPerSecond;

        _rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
