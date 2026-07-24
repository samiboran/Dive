using System;
using UnityEngine;

/// <summary>
/// Oyuncunun ekipmanına yönelik "gear fear" durumları — BotDiverAI'nin
/// Yellow/Red tier melee saldırılarının hedefi (maske çıkarma / regülatör çekme).
///
/// Maske çıkması: OxygenSystem'in zone effect stack'i üzerinden O2 tüketimini
/// artırır (ApplyZoneEffect/RemoveZoneEffect) VE bir MaskPickup sahneye düşer.
/// Zamanla kendi kendine düzelmez — oyuncu maskeyi bulup E ile ritim QTE'sini
/// (RhythmRecoveryQTE) tamamlamalı. Kaçırılan her vuruş ekstra O2 maliyeti
/// ekler ama dizi her zaman biter (RhythmRecoveryQTE'nin kendi tasarımı).
///
/// Regülatör çekilmesi: anlık O2 kaybı (RefillFromSpareTank negatif amount),
/// ayrı bir kurtarma akışı yok.
/// </summary>
[RequireComponent(typeof(OxygenSystem))]
[RequireComponent(typeof(RhythmRecoveryQTE))]
public class PlayerEquipmentState : MonoBehaviour
{
    [Header("Mask Knocked Off")]
    [SerializeField] private float maskOffOxygenDrainMultiplier = 1.5f;
    [SerializeField] private GameObject maskPickupPrefab;
    [SerializeField] private float maskDropScatterRadius = 1.5f;
    [Tooltip("RhythmRecoveryQTE'de kaçırılan her vuruş için ek O2 kaybı (saniye).")]
    [SerializeField] private float missedBeatOxygenCost = 15f;

    [Header("Regulator Pulled")]
    [SerializeField] private float regulatorPullOxygenLoss = 60f; // saniye cinsinden ani kayıp

    private OxygenSystem oxygenSystem;
    private RhythmRecoveryQTE rhythmQTE;
    private MaskPickup activeMaskPickup;
    private bool maskOff = false;

    public bool IsMaskOff => maskOff;

    public event Action OnMaskKnockedOff;
    public event Action OnMaskRecovered;
    public event Action OnRegulatorPulled;

    private void Awake()
    {
        oxygenSystem = GetComponent<OxygenSystem>();
        rhythmQTE = GetComponent<RhythmRecoveryQTE>();
    }

    public void KnockOffMask()
    {
        if (maskOff) return;

        maskOff = true;
        oxygenSystem.ApplyZoneEffect(maskOffOxygenDrainMultiplier, "MaskOff");
        OnMaskKnockedOff?.Invoke();
        Debug.Log("[PlayerEquipmentState] Maske çıkarıldı — suya düştü, bulup E ile geri tak.");

        if (maskPickupPrefab != null)
        {
            Vector3 dropPos = transform.position + UnityEngine.Random.insideUnitSphere * maskDropScatterRadius;
            GameObject obj = Instantiate(maskPickupPrefab, dropPos, Quaternion.identity);
            activeMaskPickup = obj.GetComponent<MaskPickup>();
            activeMaskPickup?.Init(this);
        }
        else
        {
            Debug.LogWarning("[PlayerEquipmentState] maskPickupPrefab atanmadı — maske hiç bulunamayacak.");
        }
    }

    /// <summary>MaskPickup, oyuncu E'ye basınca bunu çağırır.</summary>
    public void BeginMaskRecovery()
    {
        if (!maskOff || rhythmQTE == null || rhythmQTE.IsActive) return;

        rhythmQTE.OnSequenceCompleted += HandleMaskRecoveryCompleted;
        rhythmQTE.StartSequence();
    }

    private void HandleMaskRecoveryCompleted(int missedBeats)
    {
        rhythmQTE.OnSequenceCompleted -= HandleMaskRecoveryCompleted;

        oxygenSystem.RemoveZoneEffect(maskOffOxygenDrainMultiplier, "MaskOff");
        if (missedBeats > 0)
            oxygenSystem.RefillFromSpareTank(-missedBeats * missedBeatOxygenCost);

        if (activeMaskPickup != null)
            Destroy(activeMaskPickup.gameObject);
        activeMaskPickup = null;

        maskOff = false;
        OnMaskRecovered?.Invoke();
        Debug.Log($"[PlayerEquipmentState] Maske geri takıldı — {missedBeats} vuruş kaçırıldı ({missedBeats * missedBeatOxygenCost:F0} sn ek O2 kaybı).");
    }

    public void PullRegulator()
    {
        oxygenSystem.RefillFromSpareTank(-regulatorPullOxygenLoss);
        OnRegulatorPulled?.Invoke();
        Debug.Log($"[PlayerEquipmentState] Regülatör çekildi — {regulatorPullOxygenLoss} sn O2 kaybedildi.");
    }
}
