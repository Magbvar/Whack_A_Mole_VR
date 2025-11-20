using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;


public class PopUpText : MonoBehaviour
{
    public EventLogger eventLogger;   // assign via Inspector
    public GameObject popupPrefab;    // prefab with TMP_Text
    public TMP_Text text1;
    public TMP_Text text2;
    public TMP_Text text3;
    public Canvas canvas;

    void Start()
    {
        eventLogger.OnEventLogged.AddListener(EventUpdate); // Subscribe to the actual UnityEvent
    }

    private void EventUpdate(Dictionary<string, object> eventData)
    {
        
        if (eventData["Event"].ToString() != "Mole Hit") return;

        float x = float.Parse(eventData["MolePositionWorldX"].ToString());
        float y = float.Parse(eventData["MolePositionWorldY"].ToString());
        float z = float.Parse(eventData["MolePositionWorldZ"].ToString());

        Vector3 spawnPos = new Vector3(x, y, z);
        ShowPopUp(spawnPos);
    }


    private void ShowPopUp(Vector3 worldPos)
    {
        // Convert world position to canvas position
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Camera.main.WorldToScreenPoint(worldPos),
            canvas.worldCamera,
            out canvasPos
        );

        // Instantiate the popup as a child of the canvas
        GameObject instance = Instantiate(popupPrefab, canvas.transform);
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.localPosition = canvasPos;

        // Set text
        TMP_Text popupText = instance.GetComponent<TMP_Text>();
        string FeedbackText;
        FeedbackText = UnityEngine.Random.Range(0, 3) switch
        {
            0 => "Nice!",
            1 => "Perfect",
            2 => "Good"
        };

        popupText.text = FeedbackText; // or pick randomly from options

        // Optional: destroy after 1 second
        Destroy(instance, 1f);
    }


}
