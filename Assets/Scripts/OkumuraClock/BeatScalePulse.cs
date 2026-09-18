using System;
using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Beat Scale Pulse")]
[RequireComponent(typeof(RectTransform))]
public class BeatScalePulse : MonoBehaviour
{
    [Tooltip("Required. Countdown whose beats trigger the pulse. Assign it in the Inspector.")]
    [SerializeField]
    private CountdownClock _clock;

    [Tooltip("Scale multiplier reached at the top of the pulse.")]
    [Min(1f)]
    [SerializeField]
    private float _peakScale = 1.35f;

    [Tooltip("Total length of one pulse, in seconds.")]
    [Min(0.01f)]
    [SerializeField]
    private float _duration = 0.115f;

    [Tooltip("Normalized pulse shape. X is time from 0 to 1, Y is 0 at the base scale and 1 at the peak scale.")]
    [SerializeField]
    private AnimationCurve _shape = CreateDefaultShape();

    [Tooltip("Also pulse on the beat fired when the countdown starts. Off by default, so the first pulse lands one second in, with the first visible hand step.")]
    [SerializeField]
    private bool _pulseOnFirstBeat = false;

    private RectTransform _rectTransform;
    private Vector3 _baseScale;
    private float _time;
    private bool _pulsing;

    public CountdownClock Clock => _clock;

    public float PeakScale => _peakScale;

    public float Duration => _duration;

    public AnimationCurve Shape => _shape;

    public bool PulseOnFirstBeat => _pulseOnFirstBeat;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _baseScale = _rectTransform.localScale;

        if (_clock != null)
            return;

        throw new InvalidOperationException($"{nameof(BeatScalePulse)} on '{name}' requires a CountdownClock assigned in the Inspector.");
    }

    private void OnEnable()
    {
        _clock.Beat += OnBeat;
    }

    private void OnDisable()
    {
        _clock.Beat -= OnBeat;
        _pulsing = false;
        _rectTransform.localScale = _baseScale;
    }

    private void LateUpdate()
    {
        if (!_pulsing)
            return;

        _time += _clock.DeltaTime;

        if (_time >= _duration)
        {
            _pulsing = false;
            _rectTransform.localScale = _baseScale;
            return;
        }

        float scale = Mathf.LerpUnclamped(1f, _peakScale, _shape.Evaluate(_time / _duration));

        _rectTransform.localScale = new Vector3(_baseScale.x * scale, _baseScale.y * scale, _baseScale.z);
    }

    private void OnBeat(int secondsRemaining)
    {
        if (!_pulseOnFirstBeat && secondsRemaining == _clock.DurationSeconds)
            return;

        _time = 0f;
        _pulsing = true;
    }

    private static AnimationCurve CreateDefaultShape()
    {
        AnimationCurve shape = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.435f, 1f, 4.6f, -2.12f),
            new Keyframe(1f, 0f, -1.2f, 0f));

        shape.preWrapMode = WrapMode.ClampForever;
        shape.postWrapMode = WrapMode.ClampForever;

        return shape;
    }
}
