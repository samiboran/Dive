using UnityEngine;

/// <summary>
/// Tracks the player's panic level (critical oxygen, taking damage) and
/// exposes a movement speed multiplier consumed by PlayerControllerIntegration.
/// v0.1: panic slightly slows the player down (shaky, fumbling) and decays
/// over time once the trigger clears.
/// </summary>
[RequireComponent(typeof(OxygenSystem))]
public class PanicState : MonoBehaviour
{
    [Header("Panic Curve")]
    [SerializeField] private float panicRisePerSecond = 0.5f;
    [SerializeField] private float panicDecayPerSecond = 0.15f;
    [SerializeField] private float minSpeedMultiplier = 0.7f;

    [Header("Triggers")]
    [SerializeField] private float damagePanicAmount = 0.35f;

    private OxygenSystem oxygenSystem;
    private PlayerHealth playerHealth;
    private bool oxygenCritical;
    private float previousHealth = -1f;
    private float panicLevel; // 0..1

    public float PanicLevel => panicLevel;
    public float MovementSpeedMultiplier => Mathf.Lerp(1f, minSpeedMultiplier, panicLevel);

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
            panicLevel = Mathf.Clamp01(panicLevel + damagePanicAmount);
        previousHealth = current;
    }

    private void Update()
    {
        float delta = (oxygenCritical ? panicRisePerSecond : -panicDecayPerSecond) * Time.deltaTime;
        panicLevel = Mathf.Clamp01(panicLevel + delta);
    }
}
