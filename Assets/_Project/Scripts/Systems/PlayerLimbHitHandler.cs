using UnityEngine;

/// <summary>
/// Oyuncunun "Limb" tag'li collider'ına HarpoonProjectile isabet ettiğinde
/// tetiklenir (özellikle BotDiverAI Spear tier). Üç etki:
///   1) Yavaşlama — InjurySystem.ApplyInjury(KnifeWound) ile MovementMultiplier
///   2) Ekstra O2 kaybı — anlık
///   3) SharkBehavior kan lure — KnifeWound zaten SharkBehavior.PlayerIsBleeding'de okunuyor
///
/// Kurulum: oyuncunun "Limb" tag'li child collider'ına eklenir; sistemler
/// parent'ta (player root) aranır.
/// </summary>
public class PlayerLimbHitHandler : MonoBehaviour
{
    [SerializeField] private float oxygenLoss = 30f; // saniye cinsinden anlık kayıp

    private OxygenSystem oxygenSystem;
    private InjurySystem injurySystem;

    private void Awake()
    {
        oxygenSystem = GetComponentInParent<OxygenSystem>();
        injurySystem = GetComponentInParent<InjurySystem>();
    }

    public void HandleLimbHit()
    {
        injurySystem?.ApplyInjury(InjuryType.KnifeWound); // yavaşlama + kan lure
        oxygenSystem?.RefillFromSpareTank(-oxygenLoss);   // ekstra O2 kaybı
        Debug.Log("[PlayerLimbHitHandler] Uzuv vuruşu — yavaşlama + O2 kaybı + kan lure.");
    }
}
