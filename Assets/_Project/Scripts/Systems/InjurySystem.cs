using UnityEngine;

/// <summary>
/// Tracks injury severity from damage taken and whether the player is
/// currently bandaging. Exposes a movement multiplier consumed by
/// PlayerControllerIntegration (stacked there with a separate bandaging
/// movement penalty, since standing still to bandage should hurt more
/// than the injury itself).
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
public class InjurySystem : MonoBehaviour
{
    [Header("Injury Curve")]
    [SerializeField] private float minSpeedMultiplier = 0.6f; // multiplier at 0% health

    [Header("Bandaging")]
    [SerializeField] private float bandageDuration = 3f;
    [SerializeField] private float bandageHealAmount = 25f;

    private PlayerHealth playerHealth;
    private float severity; // 0 (healthy) .. 1 (near death)
    private float bandageEndTime;
    private float previousHealth = -1f;

    public bool IsBandaging { get; private set; }
    public float MovementMultiplier => Mathf.Lerp(1f, minSpeedMultiplier, severity);

    /// <summary>0 (healthy) .. 1 (near death). Used by InventorySystem to gate bandage use.</summary>
    public float Severity => severity;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        // Getting hit while bandaging interrupts it — no free heals mid-fight
        if (IsBandaging && previousHealth >= 0f && current < previousHealth)
            CancelBandaging();
        previousHealth = current;

        severity = max > 0f ? 1f - Mathf.Clamp01(current / max) : 0f;
    }

    private void Update()
    {
        if (!IsBandaging) return;

        if (Time.time >= bandageEndTime)
            FinishBandaging();
    }

    public void StartBandaging()
    {
        if (IsBandaging) return;
        IsBandaging = true;
        bandageEndTime = Time.time + bandageDuration;
    }

    private void FinishBandaging()
    {
        IsBandaging = false;
        playerHealth.Heal(bandageHealAmount);
    }

    public void CancelBandaging()
    {
        IsBandaging = false;
    }
}
