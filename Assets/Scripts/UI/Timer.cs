using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Timer : MonoBehaviour
{
    public enum TimerDisplayType
    {
        CountdownNumbers
    }

    [Header("References")]
    public GameDirector gameDirector;
    public TimerDisplayType displayType = TimerDisplayType.CountdownNumbers;
    public TMP_Text textDisplay;

    private float totalTime;
    private float timeRemaining;
    private float minutes;
    private float seconds;

    private void Start()
    {
        if (gameDirector != null)
        {
            // Subscribe to GameDirector events
            gameDirector.timeUpdate.AddListener(OnTimeUpdate);
            gameDirector.stateUpdate.AddListener(OnGameStateChange);
        }
    }

    // Called every frame by GameDirector's coroutine
    private void OnTimeUpdate(float currentTimeLeft)
    {
        timeRemaining = currentTimeLeft;
        UpdateDisplay();
    }

    // Called whenever game starts, pauses, or stops
    private void OnGameStateChange(GameDirector.GameState newState)
    {
        switch (newState)
        {
            case GameDirector.GameState.Playing:
                // Update our total duration whenever game starts
                totalTime = GetGameDuration();
                break;
            case GameDirector.GameState.Stopped:
                timeRemaining = 0;
                UpdateDisplay();
                break;
        }
    }

    // Helper: pulls the duration directly from GameDirector
    private float GetGameDuration()
    {
        // use reflection-safe access in case it's private
        var field = typeof(GameDirector).GetField("gameDefaultDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (float)field.GetValue(gameDirector);
        return 0f;
    }

    private void UpdateDisplay()
    {

        float progress = Mathf.Clamp01(timeRemaining / totalTime);
        if (textDisplay != null)
        {
            minutes = Mathf.FloorToInt(timeRemaining / 60f);
            seconds = Mathf.FloorToInt(timeRemaining % 60f);
            textDisplay.text = $"{minutes:00}:{seconds:00}"; ;
        }
    }
}
