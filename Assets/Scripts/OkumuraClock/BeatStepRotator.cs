using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Beat Step Rotator")]
[RequireComponent(typeof(RectTransform))]
public class BeatStepRotator : MonoBehaviour
{
    [Tooltip("Countdown whose beats drive the steps. Left empty, the first CountdownClock in the scene is used.")]
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

        if (_clock == null)
            _clock = FindFirstObjectByType<CountdownClock>();

        if (_clock != null)
            return;

        Debug.LogError("BeatStepRotator needs a CountdownClock in the scene.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        if (_clock == null)
            return;

        _clock.Beat += OnBeat;
        OnBeat(_clock.SecondsRemaining);
    }

    private void OnDisable()
    {
        if (_clock == null)
            return;

        _clock.Beat -= OnBeat;
    }

    private void OnBeat(int secondsRemaining)
    {
        float angle = _startAngle + (_clock.DurationSeconds - secondsRemaining) * _degreesPerSecond;

        _rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
