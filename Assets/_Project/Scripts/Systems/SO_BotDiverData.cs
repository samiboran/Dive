using UnityEngine;

public enum BotDiverTier
{
    Yellow,
    Red,
    Spear
}

/// <summary>
/// BotDiverAI'nin tüm ayarlanabilir davranış verisi. Tier'a göre kullanılan
/// alanlar değişir (ör. Spear tier CatchWindow/LurkRadius kullanmaz).
/// </summary>
[CreateAssetMenu(fileName = "SO_BotDiverData_", menuName = "UnderwaterExtraction/BotDiverData")]
public class SO_BotDiverData : ScriptableObject
{
    [field: SerializeField] public string BotName { get; private set; } = "Bot Diver";
    [field: SerializeField] public BotDiverTier Tier { get; private set; } = BotDiverTier.Yellow;
    [field: SerializeField] public float MaxHealth { get; private set; } = 60f;

    [Header("Detection")]
    [field: SerializeField] public float DetectionRange { get; private set; } = 20f;
    [field: SerializeField] public LayerMask VisionBlockLayers { get; private set; }
    [Tooltip("Oyuncunun görüş konisi TAM açısı (derece). Yarısı, botun 'yüzyüze mi' hesabında kullanılır.")]
    [field: SerializeField] public float PlayerViewAngleThreshold { get; private set; } = 100f;

    [Header("Movement")]
    [field: SerializeField] public float PatrolSpeed { get; private set; } = 2.5f;
    [field: SerializeField] public float ApproachSpeed { get; private set; } = 4f;
    [field: SerializeField] public float FleeSpeed { get; private set; } = 6f;

    [Header("Tier: Yellow — Catch Window")]
    [field: SerializeField] public float CatchWindowDuration { get; private set; } = 6f;
    [field: SerializeField] public float CatchRange { get; private set; } = 3f;

    [Header("Tier: Red — Lurking")]
    [field: SerializeField] public float LurkRadius { get; private set; } = 12f;

    [Header("Tier: Spear")]
    [field: SerializeField] public float SpearRange { get; private set; } = 15f;
    [field: SerializeField] public float SpearFireCooldown { get; private set; } = 2f;
    [field: SerializeField] public float SpearDamage { get; private set; } = 20f;
    [field: SerializeField] public float SpearProjectileSpeed { get; private set; } = 25f;
    [field: SerializeField] public GameObject SpearProjectilePrefab { get; private set; }

    [Header("Death")]
    [field: SerializeField] public SO_LootTableData DeathLootTable { get; private set; }
}
