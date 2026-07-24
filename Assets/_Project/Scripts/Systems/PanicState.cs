using System;
using System.Collections;
using UnityEngine;

public class PanicState : MonoBehaviour
{
    [Header("Thresholds")]
    [SerializeField] private float warningThreshold = 90f;
    [SerializeField] private float panicThreshold = 60f;

    [Header("Panic Effects")]
    [SerializeField] private float speedBoostMultiplier = 1.3f;
    [SerializeField] private float aimSwayMultiplier = 2.5f;
    [SerializeField] private float adsStabilityPenalty = 0.6f;

    [Header("Adrenaline")]
    [SerializeField] private float adrenalineDuration = 18f;

    // State
    private bool isPanicking = false;
    private bool isAdrenalineActive = false;
    private OxygenSystem oxygenSystem;

    // Exposed properties for PlayerController
    public float MovementSpeedMultiplier { get; private set; } = 1f;
    public float AimSwayMultiplier { get; private set; } = 1f;
    public float AdsStabilityMultiplier { get; private set; } = 1f;
    public bool IsPanicking => isPanicking;

    // Events
    public event Action OnPanicStarted;
    public event Action OnPanicEnded;
    public event Action OnAdrenalineUsed;

    private void Start()
    {
        oxygenSystem = GetComponent<OxygenSystem>();
        if (oxygenSystem != null)
        {
            oxygenSystem.OnOxygenChanged += HandleOxygenChanged;
        }
    }

    private void OnDestroy()
    {
        if (oxygenSystem != null)
            oxygenSystem.OnOxygenChanged -= HandleOxygenChanged;
    }

    private void HandleOxygenChanged(float current, float max)
    {
        if (isAdrenalineActive) return;

        if (current <= panicThreshold && !isPanicking)
        {
            EnterPanic();
        }
        else if (current > panicThreshold && isPanicking)
        {
            ExitPanic();
        }
    }

    private void EnterPanic()
    {
        isPanicking = true;
        MovementSpeedMultiplier = speedBoostMultiplier;
        AimSwayMultiplier = aimSwayMultiplier;
        AdsStabilityMultiplier = adsStabilityPenalty;
        OnPanicStarted?.Invoke();
        Debug.Log("[PanicState] PANIC STARTED — Fight or flight active.");
    }

    private void ExitPanic()
    {
        isPanicking = false;
        MovementSpeedMultiplier = 1f;
        AimSwayMultiplier = 1f;
        AdsStabilityMultiplier = 1f;
        OnPanicEnded?.Invoke();
    }

    public void UseAdrenalineShot()
    {
        if (isAdrenalineActive) return;
        StartCoroutine(AdrenalineCoroutine());
    }

    private IEnumerator AdrenalineCoroutine()
    {
        isAdrenalineActive = true;
        bool wasPanicking = isPanicking;

        if (wasPanicking)
        {
            ExitPanic();
        }

        OnAdrenalineUsed?.Invoke();
        Debug.Log($"[PanicState] Adrenaline active for {adrenalineDuration}s");

        yield return new WaitForSeconds(adrenalineDuration);

        isAdrenalineActive = false;
        Debug.Log("[PanicState] Adrenaline worn off.");

        if (oxygenSystem != null && oxygenSystem.GetCurrentOxygen() <= panicThreshold)
        {
            EnterPanic();
        }
    }
}
