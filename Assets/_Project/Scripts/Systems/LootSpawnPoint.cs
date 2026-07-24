using UnityEngine;

/// <summary>
/// Sahnede elle yerleştirilen loot spawn marker'ı.
/// Her nokta hangi loot table'ı kullanacağını (hangi kat/bölge) referans alır.
/// Start'ta kendini LootManager'a kaydeder — manager sahneyi taramak zorunda kalmaz.
/// </summary>
public class LootSpawnPoint : MonoBehaviour
{
    [field: SerializeField] public SO_LootTableData LootTable { get; private set; }

    [Tooltip("Bu noktada item çıkma temel olasılığı (0-1). Table'ın DensityMultiplier'ı ile çarpılır. CONTEXT.md: Floor1 ~%30, Floor2 ~%60, Floor3 ~%90.")]
    [field: SerializeField, Range(0f, 1f)] public float BaseSpawnChance { get; private set; } = 0.6f;

    private void Start()
    {
        if (LootManager.Instance != null)
            LootManager.Instance.RegisterSpawnPoint(this);
        else
            Debug.LogWarning($"[LootSpawnPoint] '{name}' — sahnede LootManager yok, spawn edilmeyecek.");
    }

    // Editörde görünürlük için
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
