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

    [Header("Font Size Settings")]
    public float baseFontSize = 74f;
    public float mediumComboFontSize = 86f;   // e.g. at combo >= mediumThreshold
    public float bigComboFontSize = 100f;      // e.g. at combo >= bigThreshold
    public int mediumThreshold = 5;
    public int bigThreshold = 10;

    [Header("Pop Animation Settings")]
    public float popScaleMultiplier = 1.25f;
    public float popDuration = 0.12f; // time to scale up (and same to scale back)
    private Coroutine popRoutine;
    private Vector3 originalScale;

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

        timeRemaining = 0f;
        comboIndex = 0;

        if (ComboText != null)
        {
            originalScale = ComboText.transform.localScale;
            ComboText.text = comboIndex.ToString();
            UpdateFontSize();
        }

        UpdateDisplay();
    }

    private void OnTimeUpdate(float currentTimeLeft)
    {
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
                UpdateFontSize();
                float previousMultiplier = popScaleMultiplier;
                if (comboIndex >= bigThreshold) popScaleMultiplier = 1.35f;
                else if (comboIndex >= mediumThreshold) popScaleMultiplier = 1.3f;
                else popScaleMultiplier = 1.25f;
                PlayPop();
                ResetTimer();
                break;

            case "Mole Missed":
                comboIndex = 0;
                ComboText.text = comboIndex.ToString();
                UpdateFontSize();
                ResetTimer();
                break;

            case "Mole Expired":
                comboIndex = 0;
                ComboText.text = comboIndex.ToString();
                UpdateFontSize();
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
    private void UpdateFontSize()
    {
        if (ComboText == null) return;

        if (comboIndex >= bigThreshold)
        {
            ComboText.fontSize = bigComboFontSize;
        }
        else if (comboIndex >= mediumThreshold)
        {
            ComboText.fontSize = mediumComboFontSize;
        }
        else
        {
            ComboText.fontSize = baseFontSize;
        }
    }

    private void PlayPop()
    {
        if (popRoutine != null)
        {
            StopCoroutine(popRoutine);
            popRoutine = null;
        }

        popRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        if (ComboText == null)
        {
            yield break;
        }

        Vector3 startScale = ComboText.transform.localScale;
        Vector3 peakScale = originalScale * popScaleMultiplier;

        float t = 0f;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            float lerp = t / popDuration;
            ComboText.transform.localScale = Vector3.Lerp(startScale, peakScale, lerp);
            yield return null;
        }

        ComboText.transform.localScale = peakScale;

        t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float lerp = t / popDuration;
            ComboText.transform.localScale = Vector3.Lerp(peakScale, originalScale, lerp);
            yield return null;
        }

        ComboText.transform.localScale = originalScale;

        popRoutine = null;
    }
}
