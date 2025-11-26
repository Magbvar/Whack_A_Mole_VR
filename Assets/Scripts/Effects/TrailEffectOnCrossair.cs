using System.Collections.Generic;
using UnityEngine;

public class TrailEffectOnCrossair : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem trailEffect; // Assign in inspector
    public EventLogger eventLogger;     // Assign in inspector

    [Header("Hit Speed Settings")]
    public float maxHitTime = 5f;      // Slowest hit to consider
    public float minParticleSize = 0.1f;
    public float maxParticleSize = 0.5f;
    public float minEmissionRate = 6f;
    public float maxEmissionRate = 20f;
    public Gradient hitSpeedColorGradient;

    void OnEnable()
    {
        // Subscribe to EventLogger
        if (eventLogger != null)
        {
            eventLogger.OnEventLogged.AddListener(HandleEvent); // assumes you've added an event in EventLogger
        }
    }

    private void HandleEvent(Dictionary<string, object> datas)
    {

        string evt = datas["Event"].ToString();
        if (evt == "Mole Hit")
        {
            // Calculate hit speed
            float moleSpawnTime = datas.ContainsKey("MoleSpawnTime") ? (float)datas["MoleSpawnTime"] : Time.time;
            float hitSpeed = Time.time - moleSpawnTime;
            float normalizedSpeed = Mathf.Clamp01(hitSpeed / maxHitTime); // 0 = fast, 1 = slow

            // Update particle system based on hit speed
            var main = trailEffect.main;
            main.startSize = Mathf.Lerp(maxParticleSize, minParticleSize, normalizedSpeed);

            var emission = trailEffect.emission;
            emission.rateOverTime = Mathf.Lerp(maxEmissionRate, minEmissionRate, normalizedSpeed);
            if (hitSpeedColorGradient != null)
            {
                main.startColor = hitSpeedColorGradient.Evaluate(1f - normalizedSpeed);
            }
            // Play the effect
            trailEffect.Play();
        }
        if (evt == "Mole Missed" || evt == "Mole Expired")
        {
            trailEffect.Stop();
        }
    }
}
