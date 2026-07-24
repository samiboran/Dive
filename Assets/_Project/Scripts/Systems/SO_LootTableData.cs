using UnityEngine;

/// <summary>
/// Bir loot havuzu: item listesi + yoğunluk parametresi.
/// Ağırlıklar her item'ın kendi SO_ItemData.RarityWeight alanından okunur —
/// çift veri girişi (burada ayrı weight dizisi) tutulmaz, tek doğruluk kaynağı SO'dur.
///
/// Yoğunluk, CONTEXT.md'deki Floor 1/2/3 loot density farkına karşılık gelir:
/// Floor 1 (düşük) → DensityMultiplier ~0.5
/// Floor 2 (orta)  → DensityMultiplier ~1.0
/// Floor 3 (yüksek)→ DensityMultiplier ~1.5
/// </summary>
[CreateAssetMenu(fileName = "SO_LootTable_", menuName = "UnderwaterExtraction/LootTableData")]
public class SO_LootTableData : ScriptableObject
{
    [field: SerializeField] public string TableName { get; private set; } = "Generic Loot";
    [field: SerializeField] public SO_ItemData[] Items { get; private set; }

    [Header("Density")]
    [Tooltip("Spawn noktası başına item çıkma olasılığını çarpar. 0.5 düşük, 1.0 orta, 1.5 yüksek.")]
    [field: SerializeField] public float DensityMultiplier { get; private set; } = 1f;

    /// <summary>Havuzdaki tüm item'ların toplam RarityWeight'i (0 korumalı).</summary>
    public int GetTotalWeight()
    {
        int total = 0;
        if (Items == null) return 0;
        foreach (var item in Items)
        {
            if (item != null)
                total += Mathf.Max(0, item.RarityWeight);
        }
        return total;
    }
}
