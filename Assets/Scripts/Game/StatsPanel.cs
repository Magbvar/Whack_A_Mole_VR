using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsPanel : MonoBehaviour
{
    [Header("Summary fields")]
    public TMP_Text hitsText;
    public TMP_Text missesText;
    public TMP_Text accuracyText;
    public TMP_Text reactionText;
    public TMP_Text improvementText;

    [Header("History & Graph")]
    // If you later implement a graph component, change this to the correct type (e.g. StatsGraph)
    // public StatsGraph statsGraph;
    // Or keep a container reference:
    // public GameObject graphContainer;

    [Header("Buttons")]
    public Button closeButton;
    public Button playAgainButton;

    private void Awake()
    {
        // wire default button handlers (can be overridden in Inspector)
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgain);
    }

    private void OnDestroy()
    {
        // clean up listeners to avoid leaks or duplicate calls on recompile
        if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
        if (playAgainButton != null) playAgainButton.onClick.RemoveListener(OnPlayAgain);
    }

    // Public: call this to show popup and update all fields
    public void Show()
    {
        // FIX: Restore visibility if a CanvasGroup faded the UI out
        CanvasGroup cg = GetComponentInParent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;                     // make canvas fully visible
            cg.gameObject.SetActive(true);     // re-enable it if calibration disabled it
        }

        // Make sure THIS panel is active
        gameObject.SetActive(true);

        // Refresh text values
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

        // --- Update basic text fields ---
        if (hitsText != null) hitsText.text = "Hits: " + hits;
        if (missesText != null) missesText.text = "Misses: " + misses;
        if (accuracyText != null) accuracyText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
        if (reactionText != null) reactionText.text = "Avg Reaction: " + avgReaction.ToString("F3") + "s";


        if (improvementText != null)
        {
            var recent = TestStatsRecorder.GetRecentSummaries(11);

            if (recent == null || recent.Length == 0)
            {
                improvementText.text = "No session history.";
            }
            else
            {
                // newest session = last entry
                var current = recent[recent.Length - 1];

                // number of previous sessions
                int prevCount = Mathf.Min(10, recent.Length - 1);

                if (prevCount <= 0)
                {
                    improvementText.text = "No previous sessions to compare.";
                }
                else
                {
                    // compute averages of previous sessions
                    float sumAcc = 0f;
                    float sumRt = 0f;

                    int startIndex = recent.Length - 1 - prevCount;

                    for (int i = startIndex; i < startIndex + prevCount; i++)
                    {
                        sumAcc += recent[i].accuracy;       // 0..1
                        sumRt += recent[i].avgReaction;     // seconds
                    }

                    float avgAcc = sumAcc / prevCount;
                    float avgRt = sumRt / prevCount;

                    // current session values
                    float curAcc = current.accuracy;
                    float curRt = current.avgReaction;

                    // compute differences
                    float accDiff_pp = (curAcc - avgAcc) * 100f;
                    float rtDiff = curRt - avgRt;

                    // colors
                    string green = "#2ECC71";
                    string red = "#E74C3C";

                    // determine improvement
                    bool accImproved = accDiff_pp >= 0;
                    bool rtImproved = rtDiff <= 0; // lower RT = better

                    // choose colors
                    string accColor = accImproved ? green : red;
                    string rtColor = rtImproved ? green : red;

                    // choose triangles
                    string accTriangle = accImproved ? "▲" : "▼";
                    string rtTriangle = rtImproved ? "▲" : "▼";

                    // final UI text
                    improvementText.text =
                        $"Accuracy:\n" +
                        $"Now: <color={accColor}>{(curAcc * 100f):F1}% {accTriangle}</color>\n" +
                        $"Avg: {(avgAcc * 100f):F1}%\n\n" +

                        $"Reaction Time:\n" +
                        $"Now: <color={rtColor}>{curRt:F3} s {rtTriangle}</color>\n" +
                        $"Avg: {avgRt:F3} s";



                }
            }
        }
        // ===================================================================


        // If you later add a graph:
        // if (statsGraph != null) statsGraph.DrawGraph();
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Hook to Play Again button: starts a new game by calling GameDirector
    private void OnPlayAgain()
    {
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
