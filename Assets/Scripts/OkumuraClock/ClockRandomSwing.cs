using System;
using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Clock Random Swing")]
[RequireComponent(typeof(RectTransform))]
public class ClockRandomSwing : MonoBehaviour
{
    [Tooltip("Required. Countdown whose elapsed time schedules the swings. Assign it in the Inspector.")]
    [SerializeField]
    private CountdownClock _clock;

    [Tooltip("Number of swings started per second of countdown time. Each swing eases over the whole interval, with no hold.")]
    [Min(0.01f)]
    [SerializeField]
    private float _swingsPerSecond = 3f;

    [Tooltip("Probability from 0 to 1 that a swing turns clockwise. Otherwise it turns counter-clockwise.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float _clockwiseChance = 0.58f;

    [Tooltip("Smallest clockwise swing, in degrees.")]
    [Min(0f)]
    [SerializeField]
    private float _clockwiseMinDegrees = 45f;

    [Tooltip("Largest clockwise swing, in degrees.")]
    [Min(0f)]
    [SerializeField]
    private float _clockwiseMaxDegrees = 100f;

    [Tooltip("Smallest counter-clockwise swing, in degrees.")]
    [Min(0f)]
    [SerializeField]
    private float _counterClockwiseMinDegrees = 100f;

    [Tooltip("Largest counter-clockwise swing, in degrees.")]
    [Min(0f)]
    [SerializeField]
    private float _counterClockwiseMaxDegrees = 150f;

    [Tooltip("Normalized swing easing. X is progress from 0 to 1, Y is the fraction of the swing covered.")]
    [SerializeField]
    private AnimationCurve _ease = CreateDefaultEase();

    [Tooltip("Also swing during the first second, before the countdown has ticked. Off by default, so the first swing starts one second in, with the first visible hand step. While waiting, the hand holds its angle, and a swing already in flight finishes first.")]
    [SerializeField]
    private bool _swingBeforeFirstTick = false;

    private RectTransform _rectTransform;
    private Quaternion _baseRotation;
    private float _angle;
    private float _from;
    private float _to;
    private float _time;
    private int _lastIndex;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _baseRotation = _rectTransform.localRotation;

        if (_clock != null)
            return;

        throw new InvalidOperationException($"{nameof(ClockRandomSwing)} on '{name}' requires a CountdownClock assigned in the Inspector.");
    }

    private void OnEnable()
    {
        _angle = _baseRotation.eulerAngles.z;
        _from = _angle;
        _to = _angle;
        _time = 0f;
        _lastIndex = -1;
    }

    private void OnDisable()
    {
        _rectTransform.localRotation = _baseRotation;
    }

    private void LateUpdate()
    {
        float duration = 1f / _swingsPerSecond;

        if (_clock.IsRunning)
            StartSwingOnNewInterval();

        _time = Mathf.Min(_time + _clock.DeltaTime, duration);
        _angle = Mathf.LerpUnclamped(_from, _to, _ease.Evaluate(_time / duration));

        _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _angle);
    }

    private void StartSwingOnNewInterval()
    {
        if (!_swingBeforeFirstTick && _clock.SecondsRemaining == _clock.DurationSeconds)
        {
            _lastIndex = -1;
            return;
        }

        int index = Mathf.FloorToInt(_clock.Elapsed * _swingsPerSecond);

        if (index == _lastIndex)
            return;

        _lastIndex = index;
        StartSwing();
    }

    private void StartSwing()
    {
        _from = Mathf.Repeat(_angle, 360f);
        _to = _from + RollDelta();
        _time = 0f;
    }

    private float RollDelta()
    {
        if (UnityEngine.Random.value < _clockwiseChance)
            return -UnityEngine.Random.Range(_clockwiseMinDegrees, _clockwiseMaxDegrees);

        return UnityEngine.Random.Range(_counterClockwiseMinDegrees, _counterClockwiseMaxDegrees);
    }

    private static AnimationCurve CreateDefaultEase()
    {
        AnimationCurve ease = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f),
            new Keyframe(1f, 1f, 0f, 0f));

        ease.preWrapMode = WrapMode.ClampForever;
        ease.postWrapMode = WrapMode.ClampForever;

        return ease;
    }
}
