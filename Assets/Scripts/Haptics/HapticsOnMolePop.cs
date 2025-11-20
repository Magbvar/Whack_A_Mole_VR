using UnityEngine;
using Valve.VR;

public class HapticsOnMolePop : MonoBehaviour
{
    [Header("SteamVR Haptic Action")]
    public SteamVR_Action_Vibration hapticAction;

    [Header("Intensity Settings")]
    [Range(0f, 1f)] public float amplitude = 0.8f;
    public float frequency = 100f;
    public float duration = 0.12f;

    // --------- Public API (called by your mole scripts) ---------

    // Call when a mole pops but you don't know which hand
    public void OnMolePop()
    {
        Trigger(SteamVR_Input_Sources.LeftHand);
        Trigger(SteamVR_Input_Sources.RightHand);
    }

    // Call when your mole knows which hand popped it
    public void OnMolePopBy(SteamVR_Input_Sources hand)
    {
        Trigger(hand);
    }

    // Simple helper
    private void Trigger(SteamVR_Input_Sources hand)
    {
        if (hapticAction == null) return;

        hapticAction.Execute(
            0,                // start immediately
            duration,
            frequency,
            amplitude,
            hand
        );
    }
}
