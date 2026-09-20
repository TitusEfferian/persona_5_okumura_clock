using System;
using TMPro;
using UnityEngine;

[AddComponentMenu("UI/Okumura Clock/Countdown Text")]
[RequireComponent(typeof(TextMeshProUGUI))]
public class CountdownText : MonoBehaviour
{
    [Tooltip("Required. Countdown whose beats update the text. Assign it in the Inspector.")]
    [SerializeField]
    private CountdownClock _clock;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();

        if (_clock != null)
            return;

        throw new InvalidOperationException($"{nameof(CountdownText)} on '{name}' requires a CountdownClock assigned in the Inspector.");
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
        _text.SetText("{0:00}:{1:00}", secondsRemaining / 60, secondsRemaining % 60);
    }
}
