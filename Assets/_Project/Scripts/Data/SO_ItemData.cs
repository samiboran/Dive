using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare
}

/// <summary>
/// Tüm envanter item'larının base ScriptableObject'i.
/// LootManager ile uyumluluk için rarity ve world prefab alanları içerir.
/// GÖREV 4: grid envanter için GridWidth/GridHeight eklendi (item kapladığı alan).
/// GÖREV 5: rarity tier eklendi (common/uncommon/rare dağılımı için).
/// </summary>
[CreateAssetMenu(fileName = "SO_ItemData_", menuName = "UnderwaterExtraction/ItemData")]
public class SO_ItemData : ScriptableObject
{
    [field: SerializeField] public string ItemName { get; private set; } = "Item";
    [field: SerializeField] public float Weight { get; private set; } = 1f;
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public bool IsStackable { get; private set; } = false;
    [field: SerializeField] public int MaxStack { get; private set; } = 1;

    [Header("Grid Inventory")]
    [Tooltip("Grid'de kapladığı genişlik (hücre). 1x1 = küçük item.")]
    [field: SerializeField] public int GridWidth { get; private set; } = 1;
    [field: SerializeField] public int GridHeight { get; private set; } = 1;

    [Header("Loot / World")]
    [field: SerializeField] public ItemRarity Rarity { get; private set; } = ItemRarity.Common;
    [field: SerializeField] public GameObject WorldPrefab { get; private set; } // Drop'ta sahneye spawn edilen
    [field: SerializeField] public int RarityWeight { get; private set; } = 100; // LootManager ağırlığı
}
