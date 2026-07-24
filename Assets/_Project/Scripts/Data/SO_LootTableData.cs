using UnityEngine;

/// <summary>
/// Bir zon/oda için ağırlıklı loot havuzu. LootManager, RarityWeight'e
/// göre bu listeden rastgele item seçer.
/// </summary>
[CreateAssetMenu(fileName = "SO_LootTable_", menuName = "UnderwaterExtraction/LootTableData")]
public class SO_LootTableData : ScriptableObject
{
    [field: SerializeField] public string TableName { get; private set; } = "Loot Table";
    [field: SerializeField] public SO_ItemData[] Items { get; private set; }

    [Tooltip("Zon geneli yoğunluk çarpanı — LootManager.BaseSpawnChance ile çarpılır.")]
    [field: SerializeField] public float DensityMultiplier { get; private set; } = 1f;

    public int GetTotalWeight()
    {
        if (Items == null) return 0;

        int total = 0;
        foreach (var item in Items)
        {
            if (item != null)
                total += Mathf.Max(0, item.RarityWeight);
        }
        return total;
    }
}
