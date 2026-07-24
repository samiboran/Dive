using UnityEngine;

/// <summary>
/// Oksijen tüpü verisi. SO_ItemData'dan türer (envanter + loot uyumluluğu).
/// ItemName/Weight/Icon/IsStackable/WorldPrefab/RarityWeight base'ten miras.
/// Tank stack'lenmez → IsStackable = false (inspector'da default bırak).
/// </summary>
[CreateAssetMenu(fileName = "SO_TankData_", menuName = "UnderwaterExtraction/TankData")]
public class SO_TankData : SO_ItemData
{
    [field: SerializeField] public string TierName { get; private set; } = "Standard";
    [field: SerializeField] public float CapacitySeconds { get; private set; } = 1800f; // 30 dk
    [field: SerializeField] public int TierLevel { get; private set; } = 1; // 1, 2, 3
}
