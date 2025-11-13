using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/*
An implementation of the Mole abstract class. Defines different parameters to modify and
overrides the actions to do on different events (enables, disables, popped...).
*/

public class DiskMole : Mole
{
    [SerializeField]
    private Color disabledColor;

    [SerializeField]
    private Color enabledColor;

    [SerializeField]
    private Color fakeEnabledColor;

    [SerializeField]
    private Color hoverColor;

    [SerializeField]
    private Color fakeHoverColor;

    [SerializeField]
    private Color popColor;

    [SerializeField]
    private Texture textureEnabled;

    [SerializeField]
    private Texture textureDisabled;

    [SerializeField]
    private Texture distractorLeftTexture;

    [SerializeField]
    private Texture distractorRightTexture;

    [SerializeField]
    private Texture correctMoleTexture;

    [SerializeField]
    private Texture incorrectMoleTexture;

    [SerializeField]
    private AudioClip enableSound;

    [SerializeField]
    private AudioClip disableSound;

    [SerializeField]
    private AudioClip popSound;

    [SerializeField]
    private ParticleSystem popVisual;

    [SerializeField]
    private Image loadingImg;

    [SerializeField]

    private GameObject hoverInfoContainer;

    [SerializeField]
    private HoverInfo[] hoverInfos;

    [System.Serializable]
    public class HoverInfo
    {
        public string key;
        public GameObject value;
    }

    private Shader opaqueShader;
    private Shader glowShader;
    private Material meshMaterial;
    private AudioSource audioSource;
    private Animation animationPlayer;
    private Coroutine colorAnimation;
    private string playingClip = "";
    private bool hoverInfoShouldBeShown = false;
    private string currentSpawnId = null;


    public override void Init(TargetSpawner parentSpawner)
    {
        animationPlayer = gameObject.GetComponent<Animation>();
        meshMaterial = gameObject.GetComponentInChildren<Renderer>().material;
        opaqueShader = Shader.Find("Standard");
        glowShader = Shader.Find("Particles/Standard Unlit");
        audioSource = gameObject.GetComponent<AudioSource>();
        PlayAnimation("EnableDisable");
        meshMaterial.color = disabledColor;
        showHoverInfo(false);

        base.Init(parentSpawner);
    }

    private void updateHoverInfo()
    {
        hoverInfoShouldBeShown = false;
        foreach (HoverInfo hoverInfo in hoverInfos)
        {
            hoverInfo.value.SetActive(hoverInfo.key == validationArg);
            if (hoverInfo.key == validationArg) hoverInfoShouldBeShown = true;
        }
    }

    private void showHoverInfo(bool status) // TODO: make this more elegant with animation!
    {
        hoverInfoContainer.SetActive(status && hoverInfoShouldBeShown);
    }

    /*
    Override of the event functions of the base class.
    */

    protected override IEnumerator PlayEnabling()
    {
        showHoverInfo(false);
        updateHoverInfo();
        SetLoadingValue(0);
        PlaySound(enableSound);
        PlayAnimation("EnableDisable");

        if (moleOutcome == MoleOutcome.Valid)
        {
            meshMaterial.color = enabledColor;
            meshMaterial.mainTexture = textureEnabled;
        }
        else if (moleType == Mole.MoleType.DistractorLeft)
        {
            meshMaterial.color = fakeEnabledColor;
            meshMaterial.mainTexture = distractorLeftTexture;
        }
        else if (moleType == Mole.MoleType.DistractorRight)
        {
            meshMaterial.color = fakeEnabledColor;
            meshMaterial.mainTexture = distractorRightTexture;
        }
        currentSpawnId = System.Guid.NewGuid().ToString();
        TestStatsRecorder.RecordSpawn(currentSpawnId, validationArg, Time.time);
        yield return base.PlayEnabling();
    }

    protected override IEnumerator PlayDisabling()
    {
        showHoverInfo(false);
        SetLoadingValue(0);
        PlaySound(enableSound);
        PlayAnimation("EnableDisable"); // Don't show any feedback to users when an incorrect moles expires
        meshMaterial.color = disabledColor;
        meshMaterial.mainTexture = textureDisabled;

        yield return new WaitForSeconds(getAnimationDuration());
        yield return base.PlayDisabling();
    }

    protected override void PlayMissed()
    {
        showHoverInfo(false);
        SetLoadingValue(0);
        meshMaterial.color = disabledColor;
        meshMaterial.mainTexture = textureDisabled;
        if (ShouldPerformanceFeedback())
        {
            PlayAnimation("PopWrongMole"); // Show negative feedback to users when a correct moles expires, to make it clear that they missed it
        }
        base.PlayMissed();

        if (!string.IsNullOrEmpty(currentSpawnId))
        {
            TestStatsRecorder.RecordMiss(currentSpawnId, validationArg, Time.time);
        }
        else
        {
            Debug.LogWarning("[DiskMole] PlayMissed: currentSpawnId null — miss not recorded.");
        }
        currentSpawnId = null;
    }

    protected override void PlayHoverEnter()
    {
        showHoverInfo(true);
        SetLoadingValue(0);

        if (moleOutcome == MoleOutcome.Valid)
        {
            meshMaterial.color = hoverColor;
        }
        else
        {
            meshMaterial.color = fakeHoverColor;
        }
    }

    protected override void PlayHoverLeave()
    {
        showHoverInfo(false);
        SetLoadingValue(0);

        if (moleOutcome == MoleOutcome.Valid)
        {
            meshMaterial.color = enabledColor;
        }
        else
        {
            meshMaterial.color = fakeEnabledColor;
        }
    }

    protected override IEnumerator PlayPopping()
    {
        showHoverInfo(false);
        SetLoadingValue(0);

        if (ShouldPerformanceFeedback())
        {
            if (moleOutcome == MoleOutcome.Valid)
            {
                PlayAnimation("PopCorrectMole");  // Show positive feedback to users that shoot a correct moles, to make it clear this is a success
                popVisual.startColor = enabledColor;
                popVisual.Play();
            }
            else
            {
                PlayAnimation("PopWrongMole");    // Show negative feedback to users that shoot an incorrect moles, to make it clear this is a fail
            }
        }
        meshMaterial.color = disabledColor;
        meshMaterial.mainTexture = textureDisabled;
        PlaySound(popSound);
        if (!string.IsNullOrEmpty(currentSpawnId))
        {
            TestStatsRecorder.RecordHit(currentSpawnId, validationArg, Time.time);
        }
        else
        {
            Debug.LogWarning("[DiskMole] PlayPopping: currentSpawnId null — hit not recorded.");
        }
        currentSpawnId = null;

        yield return new WaitForSeconds(getAnimationDuration());
        yield return base.PlayPopping();
    }

    public override void SetLoadingValue(float percent)
    {
        if (loadingImg != null) loadingImg.fillAmount = percent;
    }

    // Plays a sound.
    private void PlaySound(AudioClip audioClip)
    {
        if (!audioSource)
        {
            return;
        }
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    // Plays an animation clip.
    private void PlayAnimation(string animationName)
    {
        // Make sure the mole is in the right state.


        // Play the animation
        playingClip = animationName;
        animationPlayer.PlayQueued(animationName);
    }

    // Returns the duration of the currently playing animation clip.
    private float getAnimationDuration()
    {
        return animationPlayer.GetClip(playingClip).length;
    }

    // Sets up the TransitionColor coroutine to smoothly transition between two colors.
    private void PlayTransitionColor(float duration, Color startColor, Color endColor)
    {
        if (colorAnimation != null) StopCoroutine(colorAnimation);
        colorAnimation = StartCoroutine(TransitionColor(duration, startColor, endColor));
    }

    // Changes the color of the mesh.
    private void ChangeColor(Color color)
    {
        meshMaterial.color = color;
    }

    // Switches between the glowing and standard shader.
    private void SwitchShader(bool glow = false)
    {
        if (glow)
        {
            if (meshMaterial.shader.name == glowShader.name) return;
            meshMaterial.shader = glowShader;
        }
        else
        {
            if (meshMaterial.shader.name == opaqueShader.name) return;
            meshMaterial.shader = opaqueShader;
        }
    }

    // Ease function, Quart ratio.
    private float EaseQuartOut(float k)
    {
        return 1f - ((k -= 1f) * k * k * k);
    }

    private IEnumerator TransitionColor(float duration, Color startColor, Color endColor)
    {
        float durationLeft = duration;
        float totalDuration = duration;

        // Generation of a color gradient from the start color to the end color.
        Gradient colorGradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) };
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2] { new GradientAlphaKey(startColor.a, 0f), new GradientAlphaKey(endColor.a, 1f) };
        colorGradient.SetKeys(colorKey, alphaKey);

        // Playing of the animation. The DiskMole color is interpolated following the easing curve
        while (durationLeft > 0f)
        {
            float timeRatio = (totalDuration - durationLeft) / totalDuration;

            ChangeColor(colorGradient.Evaluate(EaseQuartOut(timeRatio)));
            durationLeft -= Time.deltaTime;

            yield return null;
        }

        // When the animation is finished, resets the color to its end value.
        ChangeColor(endColor);
    }
}

public static class TestStatsRecorder
{

    //This is Chats work, skal sætte mig ind i det!
    public static int totalHits = 0;
    public static int totalMisses = 0;

    [Serializable]
    public class SessionSummary
    {
        public string sessionId;
        public string dateUtc;      // ISO string
        public int hits;
        public int misses;
        public float accuracy;      // 0..1
        public float avgReaction;   // seconds
    }

    // ---------- History storage + JSON wrapper (paste after SessionSummary) ----------
    private static List<SessionSummary> sessionHistory = new List<SessionSummary>();
    private const string PREFS_HISTORY_KEY = "MoleSessionHistory_v1";

    [Serializable]
    private class SessionSummaryWrapper { public SessionSummary[] sessions; }

    public static void LoadHistory()
    {
        sessionHistory.Clear();
        if (!PlayerPrefs.HasKey(PREFS_HISTORY_KEY)) return;

        string json = PlayerPrefs.GetString(PREFS_HISTORY_KEY, "");
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var wrapper = JsonUtility.FromJson<SessionSummaryWrapper>(json);
            if (wrapper != null && wrapper.sessions != null)
                sessionHistory = new List<SessionSummary>(wrapper.sessions);
            Debug.Log($"[TestStatsRecorder] Loaded {sessionHistory.Count} saved sessions.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TestStatsRecorder] LoadHistory error: " + ex.Message);
        }
    }
    public static string GetHistoryString(int maxEntries = 10)
    {
        if (sessionHistory == null || sessionHistory.Count == 0)
            return "No previous sessions.";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int start = Math.Max(0, sessionHistory.Count - maxEntries);
        for (int i = sessionHistory.Count - 1; i >= start; i--)
        {
            var s = sessionHistory[i];
            sb.AppendLine($"{s.dateUtc} — Hits: {s.hits}, Misses: {s.misses}, Acc: {(s.accuracy * 100f):F1}%, AvgR: {s.avgReaction:F3}s");
        }
        return sb.ToString();
    }

    private static void SaveHistory()
    {
        try
        {
            var wrapper = new SessionSummaryWrapper();
            wrapper.sessions = sessionHistory.ToArray();
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PREFS_HISTORY_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[TestStatsRecorder] Saved session history (" + sessionHistory.Count + " entries).");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TestStatsRecorder] SaveHistory error: " + ex.Message);
        }
    }
    // -------------------------------------------------------------------------------
    public static void AppendSessionToHistory()
    {
        try
        {
            // build a summary from the current in-memory data
            SessionSummary s = new SessionSummary();
            s.sessionId = string.IsNullOrEmpty(sessionId) ? ("sess_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")) : sessionId;
            s.dateUtc = DateTime.UtcNow.ToString("s");
            s.hits = totalHits;
            s.misses = totalMisses;
            int total = s.hits + s.misses;
            s.accuracy = total > 0 ? (float)s.hits / total : 0f;

            // compute average reaction time if available
            float avgReaction = 0f;
            int count = 0;
            foreach (var h in hits)
            {
                if (h.reactionTime > 0f) { avgReaction += h.reactionTime; count++; }
            }
            if (count > 0) avgReaction /= count;
            s.avgReaction = avgReaction;

            // append to in-memory history and persist
            sessionHistory.Add(s);
            SaveHistory();

            Debug.Log($"[TestStatsRecorder] Appended session {s.sessionId} to history. Total saved sessions: {sessionHistory.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TestStatsRecorder] AppendSessionToHistory error: " + ex.Message);
        }
    }


    // store spawn times so we can compute reaction times
    private static Dictionary<string, float> spawnTimes = new Dictionary<string, float>();

    public class HitRecord
    {
        public string id;
        public string type;
        public float spawnTime;     // 0 if unknown
        public float hitTime;
        public float reactionTime;  // hitTime - spawnTime (if spawnTime known)
    }
    public static List<HitRecord> hits = new List<HitRecord>();

    // optional session id to split files per-session
    private static string sessionId = null;

    public static void StartSession()
    {
        sessionId = "sess_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        Reset(); // clears in-memory counters for a fresh session
        Debug.Log($"[TestStatsRecorder] Session started: {sessionId}");
    }

    public static void Reset()
    {
        totalHits = 0;
        totalMisses = 0;
        hits.Clear();
        spawnTimes.Clear();
        Debug.Log("[TestStatsRecorder] Reset stats for new session.");
    }

    public static void RecordSpawn(string spawnId, string moleType, float time)
    {
        // store spawn time for reaction calculation later
        try { spawnTimes[spawnId] = time; } catch { }

        Debug.Log($"[TestStatsRecorder] Spawn: id={spawnId} type={moleType} time={time:F3};");

        try
        {
            string folder = Path.Combine(Application.persistentDataPath, "test_mole_stats");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string filename = sessionId != null ? $"spawns_{sessionId}.txt" : "spawns.txt";
            string path = Path.Combine(folder, filename);
            string line = $"{DateTime.UtcNow:s},{spawnId},{moleType},{time:F3}\n";
            File.AppendAllText(path, line);
            Debug.Log($"[TestStatsRecorder] appended spawn to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TestStatsRecorder] failed to write spawn file: {ex.Message}");
        }
    }

    public static void RecordHit(string spawnId, string moleType, float time)
    {
        totalHits++;

        float spawnTime = 0f;
        float reaction = 0f;
        if (!string.IsNullOrEmpty(spawnId) && spawnTimes.TryGetValue(spawnId, out float st))
        {
            spawnTime = st;
            reaction = time - st;
            // optionally remove the spawnTime so memory won't grow
            spawnTimes.Remove(spawnId);
        }

        var rec = new HitRecord { id = spawnId, type = moleType, spawnTime = spawnTime, hitTime = time, reactionTime = reaction };
        hits.Add(rec);

        

        try
        {
            string folder = Path.Combine(Application.persistentDataPath, "test_mole_stats");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string filename = sessionId != null ? $"hits_{sessionId}.txt" : "hits.txt";
            string path = Path.Combine(folder, filename);
            // include reaction time in file
            string line = $"{DateTime.UtcNow:s},{spawnId},{moleType},{time:F3},{reaction:F3}\n";
            File.AppendAllText(path, line);
            Debug.Log($"[TestStatsRecorder] appended hit to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TestStatsRecorder] failed to write hit file: {ex.Message}");
        }
    }

    public static void RecordMiss(string spawnId, string moleType, float time)
    {
        totalMisses++;

        // if spawnTime exists, remove it — spawn is finished
        if (!string.IsNullOrEmpty(spawnId) && spawnTimes.ContainsKey(spawnId))
            spawnTimes.Remove(spawnId);

       
        try
        {
            string folder = Path.Combine(Application.persistentDataPath, "test_mole_stats");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string filename = sessionId != null ? $"misses_{sessionId}.txt" : "misses.txt";
            string path = Path.Combine(folder, filename);
            string line = $"{DateTime.UtcNow:s},{spawnId},{moleType},{time:F3}\n";
            File.AppendAllText(path, line);
            Debug.Log($"[TestStatsRecorder] appended miss to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TestStatsRecorder] failed to write miss file: {ex.Message}");
        }
    }

    // print a richer summary
    public static void PrintSummary()
    {
        int total = totalHits + totalMisses;
        float accuracy = total > 0 ? (float)totalHits / total : 0f;

        // compute average reaction if any
        float avgReaction = 0f;
        var reactions = new List<float>();
        foreach (var h in hits) if (h.reactionTime > 0f) reactions.Add(h.reactionTime);
        if (reactions.Count > 0)
        {
            float sum = 0f;
            foreach (var r in reactions) sum += r;
            avgReaction = sum / reactions.Count;
        }

        Debug.Log($"[TestStatsRecorder] Summary - Hits: {totalHits}, Misses: {totalMisses}, Accuracy: {accuracy:P1}, AvgReaction: {avgReaction:F3}s");
    }
}

