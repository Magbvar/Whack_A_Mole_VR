using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Valve.VR.InteractionSystem;

public class ComboCounter : MonoBehaviour
{
    [Header("References")]
    public GameDirector gameDirector;
    public EventLogger eventLogger;
    public Image fillImage;
    public TMP_Text ComboText;

    [Header("Settings")]
    public float moleLifetime = 3.6f;

    private float timeRemaining;
    private int comboIndex;

    void Start()
    {
        // Subscribe to GameDirector and EventLogger events
        if (gameDirector != null)
        {
            gameDirector.timeUpdate.AddListener(OnTimeUpdate);
            gameDirector.stateUpdate.AddListener(OnGameStateChange);
        }

        if (eventLogger != null)
        {
            eventLogger.OnEventLogged.AddListener(OnEventLogged);
        }

        // Initialize
        timeRemaining = 0f;
        comboIndex = 0;
        UpdateDisplay();
    }

    private void OnTimeUpdate(float currentTimeLeft)
    {
        // Optional: if you want to sync with game time
        UpdateDisplay();
    }

    private void OnGameStateChange(GameDirector.GameState newState)
    {
        if (newState == GameDirector.GameState.Playing)
        {
            // Reset display if needed
            UpdateDisplay();
        }
    }

    private void OnEventLogged(Dictionary<string, object> datas)
    {
        string evt = datas["Event"].ToString();

        switch (evt)
        {
            case "Mole Hit":
                comboIndex++;
                ComboText.text = comboIndex.ToString();
                ResetTimer();
                break;

            case "Mole Missed":
                comboIndex = 0;
                ComboText.text = comboIndex.ToString();
                ResetTimer();
                break;

            case "Mole Expired":
                comboIndex = 0;
                ComboText.text = comboIndex.ToString();
                break;
        }
    }

    private void ResetTimer()
    {
        timeRemaining = moleLifetime;
        UpdateDisplay();
    }

    private void Update()
    {
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (fillImage != null)
        {
            float progress = moleLifetime > 0f ? timeRemaining / moleLifetime : 0f;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = Mathf.Clamp01(progress);
        }
    }
}
