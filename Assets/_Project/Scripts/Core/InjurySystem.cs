using System;
using System.Collections;
using UnityEngine;

public enum InjuryType
{
    None,
    SharkBite,
    JellyfishSting,
    Barotrauma
}

public class InjurySystem : MonoBehaviour
{
    [Header("Shark Bite")]
    [SerializeField] private float sharkBleedDps = 3f;
    [SerializeField] private float sharkMovePenalty = 0.7f;

    [Header("Bandage")]
    [SerializeField] private float bandageApplyTime = 4f;

    // State
    private InjuryType currentInjury = InjuryType.None;
    private bool isBandaging = false;
    private Coroutine bleedCoroutine;
    private Coroutine bandageCoroutine;

    // Events
    public event Action<InjuryType> OnInjured;
    public event Action OnBandageApplied;
    public event Action OnBandageInterrupted;

    // Exposed for PlayerController
    public float MovementMultiplier { get; private set; } = 1f;
    public bool IsBandaging => isBandaging;

    public void ApplyInjury(InjuryType type)
    {
        if (currentInjury != InjuryType.None) return;

        currentInjury = type;
        OnInjured?.Invoke(type);

        switch (type)
        {
            case InjuryType.SharkBite:
                MovementMultiplier = sharkMovePenalty;
                bleedCoroutine = StartCoroutine(BleedCoroutine(sharkBleedDps));
                break;
        }

        Debug.Log($"[InjurySystem] Injury applied: {type}");
    }

    public void StartBandage()
    {
        if (currentInjury == InjuryType.None || isBandaging) return;

        isBandaging = true;
        bandageCoroutine = StartCoroutine(BandageCoroutine());
    }

    private IEnumerator BandageCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < bandageApplyTime)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        HealInjury();
        isBandaging = false;
        OnBandageApplied?.Invoke();
    }

    public void InterruptBandage()
    {
        if (!isBandaging) return;

        if (bandageCoroutine != null)
            StopCoroutine(bandageCoroutine);

        isBandaging = false;
        OnBandageInterrupted?.Invoke();
        Debug.Log("[InjurySystem] Bandage interrupted!");
    }

    private void HealInjury()
    {
        if (bleedCoroutine != null)
            StopCoroutine(bleedCoroutine);

        currentInjury = InjuryType.None;
        MovementMultiplier = 1f;
    }

    private IEnumerator BleedCoroutine(float dps)
    {
        var health = GetComponent<PlayerHealth>();
        while (currentInjury != InjuryType.None)
        {
            yield return new WaitForSeconds(1f);
            health?.TakeDamage(dps);
        }
    }

    public InjuryType GetCurrentInjury() => currentInjury;
}
