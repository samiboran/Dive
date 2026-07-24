using UnityEngine;

/// <summary>
/// Tüm envanter item'larının base ScriptableObject'i.
/// LootManager ile uyumluluk için rarity ve world prefab alanları içerir.
/// </summary>
[CreateAssetMenu(fileName = "SO_ItemData_", menuName = "UnderwaterExtraction/ItemData")]
public class SO_ItemData : ScriptableObject
{
    [field: SerializeField] public string ItemName { get; private set; } = "Item";
    [field: SerializeField] public float Weight { get; private set; } = 1f;
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public bool IsStackable { get; private set; } = false;
    [field: SerializeField] public int MaxStack { get; private set; } = 1;

    [Header("Loot / World")]
    [field: SerializeField] public GameObject WorldPrefab { get; private set; } // Drop'ta sahneye spawn edilen
    [field: SerializeField] public int RarityWeight { get; private set; } = 100; // LootManager ağırlığı
}
