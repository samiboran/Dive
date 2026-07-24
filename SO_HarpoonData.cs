using UnityEngine;

/// <summary>
/// REFACTOR: Artık SO_ItemData'dan türüyor (envanter + loot uyumluluğu).
/// ItemName/Weight/Icon/IsStackable/WorldPrefab/RarityWeight base'ten miras.
/// Zıpkın stack'lenmez → IsStackable = false (inspector'da default bırak).
/// </summary>
[CreateAssetMenu(fileName = "SO_HarpoonData_", menuName = "UnderwaterExtraction/HarpoonData")]
public class SO_HarpoonData : SO_ItemData
{
    [field: SerializeField] public string HarpoonName { get; private set; } = "Basic Harpoon";
    [field: SerializeField] public float Range { get; private set; } = 25f;
    [field: SerializeField] public float Damage { get; private set; } = 35f;
    [field: SerializeField] public float FireRate { get; private set; } = 1.2f; // shots per second
    [field: SerializeField] public float ReloadDuration { get; private set; } = 2.5f;
    [field: SerializeField] public float ProjectileSpeed { get; private set; } = 30f;
    [field: SerializeField] public GameObject ProjectilePrefab { get; private set; }
}
