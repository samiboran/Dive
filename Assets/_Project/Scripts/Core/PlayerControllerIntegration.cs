using System;
using UnityEngine;

/// <summary>
/// ENTEGRASYON — mevcut Rigidbody tabanlı PlayerController (buoyancy + AddForce)
/// ile OxygenSystem/PanicState/InjurySystem arasındaki köprü.
///
/// v2 düzeltmeleri:
/// - CharacterController referansı KALDIRILDI — hız Rigidbody.velocity üzerinden okunur
/// - SetCombatState artık HarpoonWeapon.OnFired event'ine bağlı (5 sn timer)
/// - Panic/Injury speed multiplier blending gerçek kodda (SpeedMultiplier property)
///
/// PlayerController v0.1, hareket hızını hesaplarken bu component'in
/// SpeedMultiplier değerini çarpmalı:
///     float finalSpeed = baseSpeed * integration.SpeedMultiplier;
/// </summary>
[RequireComponent(typeof(OxygenSystem))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerIntegration : MonoBehaviour
{
    [Header("Combat Detection")]
    [SerializeField] private float combatDuration = 5f; // Ateş sonrası combat'ta kalma süresi
    [SerializeField] private HarpoonWeapon weapon; // boş bırakılırsa child'larda aranır

    [Header("Bandage Movement Penalty")]
    [SerializeField] private float bandagingMoveMultiplier = 0.3f;

    [Header("Carry Capacity (Weight)")]
    [Tooltip("Ağırlık → hız çarpanı eğrisi. X: taşınan ağırlık (kg), Y: hız çarpanı.")]
    [SerializeField] private AnimationCurve weightSpeedCurve = AnimationCurve.Linear(0f, 1f, 30f, 0.6f);
    [SerializeField] private float overweightThreshold = 30f; // üstü = aşırı yük uyarısı

    public event Action OnOverweight; // UI bağlanır

    [Header("Movement Thresholds")]
    [SerializeField] private float moveThreshold = 0.2f; // m/s — altı idle sayılır

    private Rigidbody rb;
    private OxygenSystem oxygenSystem;
    private PanicState panicState;
    private InjurySystem injurySystem;
    private InventorySystem inventorySystem;
    private UnderwaterExtraction.Core.PlayerController playerController;

    private float combatTimer = 0f;
    private bool overweightWarned = false;

    /// <summary>
    /// PlayerController hareket hızını bununla çarpar.
    /// Panic boost + injury penalty + bandage penalty tek değerde blend edilmiş.
    /// </summary>
    public float SpeedMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        oxygenSystem = GetComponent<OxygenSystem>();
        panicState = GetComponent<PanicState>();
        injurySystem = GetComponent<InjurySystem>();
        inventorySystem = GetComponent<InventorySystem>();
        playerController = GetComponent<UnderwaterExtraction.Core.PlayerController>();

        if (weapon == null)
            weapon = GetComponentInChildren<HarpoonWeapon>();
    }

    private void OnEnable()
    {
        if (weapon != null)
            weapon.OnFired += HandleFired;
    }

    private void OnDisable()
    {
        if (weapon != null)
            weapon.OnFired -= HandleFired;
    }

    private void HandleFired()
    {
        combatTimer = combatDuration;
        oxygenSystem.SetCombatState(true);
    }

    private void Update()
    {
        // 1) DERİNLİK — PlayerController'ın waterSurfaceHeight referansıyla aynı hesap
        // kullanılmalı, yoksa OxygenSystem ile buoyancy farklı yüzey varsayımlarıyla çalışır.
        float depth = playerController != null ? playerController.DepthBelowSurface : Mathf.Max(0f, -transform.position.y);
        oxygenSystem.SetDepth(depth);

        // 2) HAREKET DURUMU — Rigidbody hızından oku
        float speed = rb.velocity.magnitude;
        bool isSprinting = speed > moveThreshold && Input.GetKey(KeyCode.LeftShift);

        string movementState =
            speed <= moveThreshold ? "idle" :
            isSprinting ? "sprint" : "swim";

        oxygenSystem.SetMovementState(movementState);

        // 3) COMBAT TIMER — süre dolunca combat biter
        if (combatTimer > 0f)
        {
            combatTimer -= Time.deltaTime;
            if (combatTimer <= 0f)
                oxygenSystem.SetCombatState(false);
        }

        // 4) SPEED MULTIPLIER BLENDING — panic × injury × bandage
        float multiplier = 1f;

        if (panicState != null)
            multiplier *= panicState.MovementSpeedMultiplier;

        if (injurySystem != null)
        {
            multiplier *= injurySystem.MovementMultiplier;
            if (injurySystem.IsBandaging)
                multiplier *= bandagingMoveMultiplier;
        }

        // 5) TAŞIMA AĞIRLIĞI — envanter ağırlığı hızı düşürür
        if (inventorySystem != null)
        {
            float totalWeight = inventorySystem.GetTotalWeight();
            multiplier *= weightSpeedCurve.Evaluate(totalWeight);

            if (totalWeight > overweightThreshold && !overweightWarned)
            {
                overweightWarned = true;
                OnOverweight?.Invoke();
            }
            else if (totalWeight <= overweightThreshold)
            {
                overweightWarned = false;
            }
        }

        SpeedMultiplier = multiplier;
    }
}
