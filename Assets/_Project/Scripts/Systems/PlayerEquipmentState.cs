using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Oyuncunun ekipmanına yönelik "gear fear" durumları — BotDiverAI'nin
/// Yellow/Red tier melee saldırılarının hedefi (maske çıkarma / regülatör çekme).
///
/// Maske çıkması: geçici, OxygenSystem'in zone effect stack'i üzerinden
/// O2 tüketimini artırır (ApplyZoneEffect/RemoveZoneEffect zaten bu tarz
/// stack'lenebilir etkiler için mevcuttu).
/// Regülatör çekilmesi: anlık O2 kaybı (RefillFromSpareTank negatif amount).
/// </summary>
[RequireComponent(typeof(OxygenSystem))]
public class PlayerEquipmentState : MonoBehaviour
{
    [Header("Mask Knocked Off")]
    [SerializeField] private float maskOffDuration = 6f;
    [SerializeField] private float maskOffOxygenDrainMultiplier = 1.5f;

    [Header("Regulator Pulled")]
    [SerializeField] private float regulatorPullOxygenLoss = 60f; // saniye cinsinden ani kayıp

    private OxygenSystem oxygenSystem;
    private bool maskOff = false;

    public bool IsMaskOff => maskOff;

    public event Action OnMaskKnockedOff;
    public event Action OnMaskRecovered;
    public event Action OnRegulatorPulled;

    private void Awake()
    {
        oxygenSystem = GetComponent<OxygenSystem>();
    }

    public void KnockOffMask()
    {
        if (maskOff) return;
        StartCoroutine(MaskOffCoroutine());
    }

    private IEnumerator MaskOffCoroutine()
    {
        maskOff = true;
        oxygenSystem.ApplyZoneEffect(maskOffOxygenDrainMultiplier, "MaskOff");
        OnMaskKnockedOff?.Invoke();
        Debug.Log("[PlayerEquipmentState] Maske çıkarıldı — O2 tüketimi arttı.");

        yield return new WaitForSeconds(maskOffDuration);

        oxygenSystem.RemoveZoneEffect(maskOffOxygenDrainMultiplier, "MaskOff");
        maskOff = false;
        OnMaskRecovered?.Invoke();
        Debug.Log("[PlayerEquipmentState] Maske geri takıldı.");
    }

    public void PullRegulator()
    {
        oxygenSystem.RefillFromSpareTank(-regulatorPullOxygenLoss);
        OnRegulatorPulled?.Invoke();
        Debug.Log($"[PlayerEquipmentState] Regülatör çekildi — {regulatorPullOxygenLoss} sn O2 kaybedildi.");
    }
}
