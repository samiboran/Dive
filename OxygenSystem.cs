using System;
using UnityEngine;

public class OxygenSystem : MonoBehaviour
{
    [Header("Base Configuration")]
    [SerializeField] private SO_TankData currentTank;
    [SerializeField] private float baseCapacitySeconds = 1800f;

    [Header("Consumption Multipliers")]
    [SerializeField] private float idleMultiplier = 0.5f;
    [SerializeField] private float swimMultiplier = 1.0f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float combatMultiplier = 1.4f;

    [Header("Depth Curve")]
    [SerializeField] private AnimationCurve depthConsumptionCurve; // 0-1 arası derinlik → çarpan

    // State
    private float currentOxygen;
    private float maxOxygen;
    private bool isInAirPocket = false;
    private bool isInCombat = false;
    private float currentDepth = 0f;
    private string currentMovementState = "swim";

    // External zone effects (stackable)
    private float zoneEffectMultiplier = 1f;

    // Events
    public event Action<float, float> OnOxygenChanged; // current, max
    public event Action OnOxygenCritical; // ≤ 90 sn
    public event Action OnOxygenDepleted;

    private bool criticalWarned = false;

    private void Start()
    {
        EquipTank(currentTank);
    }

    private void Update()
    {
        if (isInAirPocket) return;

        float depthFactor = depthConsumptionCurve.Evaluate(Mathf.Clamp01(currentDepth / 100f));
        float movementMultiplier = GetMovementMultiplier();

        float totalMultiplier = movementMultiplier * (isInCombat ? combatMultiplier : 1f) * depthFactor * zoneEffectMultiplier;

        float consumption = Time.deltaTime * totalMultiplier;
        currentOxygen -= consumption;

        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);

        if (currentOxygen <= 90f && !criticalWarned)
        {
            criticalWarned = true;
            OnOxygenCritical?.Invoke();
        }

        if (currentOxygen <= 0f)
        {
            OnOxygenDepleted?.Invoke();
            enabled = false; // RefillOxygen/RefillFromSpareTank tekrar açar
        }
    }

    public void EquipTank(SO_TankData tank)
    {
        currentTank = tank ?? currentTank;
        maxOxygen = currentTank != null ? currentTank.CapacitySeconds : baseCapacitySeconds;
        currentOxygen = maxOxygen;
        criticalWarned = false;
        enabled = true;
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    /// <summary>Tankı tamamen doldurur (RefillStation kullanımı).</summary>
    public void RefillOxygen()
    {
        currentOxygen = maxOxygen;
        criticalWarned = false;
        enabled = true; // FIX: depleted sonrası kapanan Update'i geri aç
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    /// <summary>
    /// Envanterdeki yedek tüpü kullanır — tankı DEĞİŞTİRMEZ,
    /// mevcut tanka amount kadar oksijen ekler (max'i aşmaz).
    /// EquipTank() tam tank değişimi; bu ise kısmi dolum.
    /// </summary>
    public void RefillFromSpareTank(float amount)
    {
        currentOxygen = Mathf.Clamp(currentOxygen + amount, 0f, maxOxygen);
        if (currentOxygen > 90f) criticalWarned = false;
        enabled = true;
        OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
    }

    public void SetMovementState(string state)
    {
        // "idle", "swim", "sprint"
        currentMovementState = state;
    }

    private float GetMovementMultiplier()
    {
        return currentMovementState switch
        {
            "idle" => idleMultiplier,
            "sprint" => sprintMultiplier,
            _ => swimMultiplier
        };
    }

    public void SetCombatState(bool inCombat) => isInCombat = inCombat;
    public void SetDepth(float depth) => currentDepth = depth;

    public void ApplyZoneEffect(float multiplier, string source)
    {
        zoneEffectMultiplier *= multiplier;
        Debug.Log($"[OxygenSystem] Zone effect applied from {source}: x{multiplier}");
    }

    public void RemoveZoneEffect(float multiplier, string source)
    {
        zoneEffectMultiplier /= Mathf.Max(multiplier, 0.001f);
    }

    public void EnterAirPocket() => isInAirPocket = true;
    public void ExitAirPocket() => isInAirPocket = false;

    public float GetCurrentOxygen() => currentOxygen;
    public float GetMaxOxygen() => maxOxygen;
}
