using UnityEngine;

public enum ConsumableType
{
    Bandage,
    AdrenalineShot,
    SpareTank
}

/// <summary>
/// Kullanılabilir item'lar (bandaj, adrenalin, yedek tüp).
/// InventorySystem.UseSelectedConsumable() → ilgili sisteme delege eder.
/// </summary>
[CreateAssetMenu(fileName = "SO_Consumable_", menuName = "UnderwaterExtraction/ConsumableItemData")]
public class SO_ConsumableItemData : SO_ItemData
{
    [field: SerializeField] public ConsumableType Type { get; private set; }

    // Consumable'a göre anlamı değişir: SpareTank için dolum saniyesi,
    // AdrenalineShot için PanicState.ApplyAdrenaline() bağışıklık süresi (sn).
    [field: SerializeField] public float EffectAmount { get; private set; } = 0f;
}
