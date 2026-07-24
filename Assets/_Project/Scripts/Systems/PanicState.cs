using UnityEngine;

/// <summary>
/// Tracks the player's panic level (critical oxygen, taking damage) and
/// exposes movement/aim multipliers consumed by PlayerControllerIntegration
/// and HarpoonWeapon.
/// v0.1 design: panic is an adrenaline rush — it speeds the player up but
/// wrecks their aim. An adrenaline item can cancel it and grant temporary
/// immunity. Decays on its own over time once the trigger clears.
/// </summary>
[RequireComponent(typeof(OxygenSystem))]
public class PanicState : MonoBehaviour
{
    [Header("Panic Curve")]
    [SerializeField] private float panicRisePerSecond = 0.5f;
    [SerializeField] private float panicDecayPerSecond = 0.15f;

    [Header("Movement (panic speeds you up)")]
    [SerializeField] private float maxSpeedMultiplier = 1.4f;

    [Header("Aim (panic wrecks your accuracy)")]
    [SerializeField] private float minAimAccuracyMultiplier = 0.4f;

    [Header("Triggers")]
    [SerializeField] private float damagePanicAmount = 0.35f;

    private OxygenSystem oxygenSystem;
    private PlayerHealth playerHealth;
    private bool oxygenCritical;
    private float previousHealth = -1f;
    private float panicLevel; // 0..1
    private float adrenalineImmunityEndTime;

    public float PanicLevel => panicLevel;

    /// <summary>Consumed by PlayerControllerIntegration — panic boosts movement speed.</summary>
    public float MovementSpeedMultiplier => Mathf.Lerp(1f, maxSpeedMultiplier, panicLevel);

    /// <summary>Consumed by HarpoonWeapon — panic widens aim spread. 1 = perfect aim.</summary>
    public float AimAccuracyMultiplier => Mathf.Lerp(1f, minAimAccuracyMultiplier, panicLevel);

    private void Awake()
    {
        oxygenSystem = GetComponent<OxygenSystem>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        oxygenSystem.OnOxygenCritical += HandleOxygenCritical;
        oxygenSystem.OnOxygenChanged += HandleOxygenChanged;
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        oxygenSystem.OnOxygenCritical -= HandleOxygenCritical;
        oxygenSystem.OnOxygenChanged -= HandleOxygenChanged;
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleOxygenCritical() => oxygenCritical = true;

    private void HandleOxygenChanged(float current, float max)
    {
        if (current > 90f) oxygenCritical = false;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (previousHealth >= 0f && current < previousHealth)
            RaisePanic(damagePanicAmount);
        previousHealth = current;
    }

    private void Update()
    {
        if (Time.time < adrenalineImmunityEndTime)
        {
            panicLevel = Mathf.Clamp01(panicLevel - panicDecayPerSecond * Time.deltaTime);
            return;
        }

        float delta = (oxygenCritical ? panicRisePerSecond : -panicDecayPerSecond) * Time.deltaTime;
        panicLevel = Mathf.Clamp01(panicLevel + delta);
    }

    private void RaisePanic(float amount)
    {
        if (Time.time < adrenalineImmunityEndTime) return; // immune, damage still hurts but doesn't spike panic
        panicLevel = Mathf.Clamp01(panicLevel + amount);
    }

    /// <summary>Called by AdrenalineItem: cancels panic immediately and blocks new panic for `duration` seconds.</summary>
    public void ApplyAdrenaline(float duration)
    {
        panicLevel = 0f;
        adrenalineImmunityEndTime = Time.time + duration;
    }
}
