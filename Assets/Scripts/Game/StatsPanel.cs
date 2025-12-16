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

    [Header("Buttons")]
    public Button closeButton;
    public Button playAgainButton;

    private void Awake()
    {
     
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgain);
    }

    private void OnDestroy()
    {
   
        if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
        if (playAgainButton != null) playAgainButton.onClick.RemoveListener(OnPlayAgain);
    }

   
    public void Show()
    {
       
        CanvasGroup cg = GetComponentInParent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;                     
            cg.gameObject.SetActive(true);    
        }

       
        gameObject.SetActive(true);

        // Refresh text values
        Refresh();
    }

  
    public void Refresh()
    {
      
        int hits = TestStatsRecorder.totalHits;
        int misses = TestStatsRecorder.totalMisses;
        int total = hits + misses;
        float accuracy = total > 0 ? (float)hits / total * 100f : 0f;

        
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


        if (improvementText != null)
        {
            var recent = TestStatsRecorder.GetRecentSummaries(11);

            if (recent == null || recent.Length == 0)
            {
                improvementText.text = "No session history.";
            }
            else
            {
                
                var current = recent[recent.Length - 1];

              
                int prevCount = Mathf.Min(10, recent.Length - 1);

                if (prevCount <= 0)
                {
                    improvementText.text = "No previous sessions to compare.";
                }
                else
                {
                   
                    float sumAcc = 0f;
                    float sumRt = 0f;

                    int startIndex = recent.Length - 1 - prevCount;

                    for (int i = startIndex; i < startIndex + prevCount; i++)
                    {
                        sumAcc += recent[i].accuracy;       
                        sumRt += recent[i].avgReaction;     
                    }

                    float avgAcc = sumAcc / prevCount;
                    float avgRt = sumRt / prevCount;

                   
                    float curAcc = current.accuracy;
                    float curRt = current.avgReaction;

                    float accDiff_pp = (curAcc - avgAcc) * 100f;
                    float rtDiff = curRt - avgRt;

                    string green = "#2ECC71";
                    string red = "#E74C3C";

                    bool accImproved = accDiff_pp >= 0;
                    bool rtImproved = rtDiff <= 0; 

                    string accColor = accImproved ? green : red;
                    string rtColor = rtImproved ? green : red;

                    string accTriangle = accImproved ? "▲" : "▼";
                    string rtTriangle = rtImproved ? "▲" : "▼";

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
       
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }


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
