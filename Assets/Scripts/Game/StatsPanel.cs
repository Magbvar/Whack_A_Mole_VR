using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class StatsPanel : MonoBehaviour
{
    //most of this is chat.gpt generated 
    [Header("Summary fields")]
    public TMP_Text hitsText;
    public TMP_Text missesText;
    public TMP_Text accuracyText;
    public TMP_Text reactionText;
   

    [Header("History & Graph")]

    public StatsPanel statsGraph;          // reference to your StatsGraph component (optional)

    [Header("Buttons")]
    public Button closeButton;
    

    private void Awake()
    {
        // wire default button handlers (can be overridden in Inspector)
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
     
    }

    // Public: call this to show popup and update all fields
    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("[StatsPanel] Show() called. StackTrace:\n" + System.Environment.StackTrace);
        Refresh();
    }

    // Update fields from TestStatsRecorder
    public void Refresh()
    {
        // summary numbers
        int hits = TestStatsRecorder.totalHits;
        int misses = TestStatsRecorder.totalMisses;
        int total = hits + misses;
        float accuracy = total > 0 ? (float)hits / total * 100f : 0f;

        // compute avg reaction from recorded hits
        float avgReaction = 0f;
        int count = 0;
        foreach (var h in TestStatsRecorder.hits)
        {
            if (h.reactionTime > 0f)
            {
                avgReaction += h.reactionTime;
                count++;
            }
        }
        if (count > 0) avgReaction /= count;

        if (hitsText != null) hitsText.text = "Hits: " + hits;
        if (missesText != null) missesText.text = "Misses: " + misses;
        if (accuracyText != null) accuracyText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
        if (reactionText != null) reactionText.text = "Avg Reaction: " + avgReaction.ToString("F3") + "s";
        

        // draw graph (if present)
        if (statsGraph != null)
        {
            //statsGraph.DrawGraph();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Hook to Play Again button: starts a new game by calling GameDirector (simple approach)
    private void OnPlayAgain()
    {
        // find GameDirector and call StartGame
        var gd = FindObjectOfType<GameDirector>();
        if (gd != null)
        {
            Hide();
            gd.StartGame();
        }
        else
        {
            Debug.LogWarning("[StatsPanel] GameDirector not found.");
        }
    }
}
