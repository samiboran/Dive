using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zon/oda bazlı loot spawn'ı. Weighted random seçim yapar,
/// item'ları mevcut WorldItemPickup prefab sistemiyle sahneye koyar.
/// Yeni bir pickup mekanizması YOK — InventorySystem ile bire bir uyumlu.
///
/// Kurulum:
/// 1) Sahneye boş GameObject'ler koy, üzerine LootSpawnPoint ekle (aşağıda)
/// 2) Her zon/oda için bir SO_LootTableData asset'i oluştur
/// 3) Bu manager'a zon başına (table + spawnPoint listesi) tanımla
/// </summary>
public class LootManager : MonoBehaviour
{
    [System.Serializable]
    public class LootZone
    {
        public string ZoneName = "Zone";
        public SO_LootTableData LootTable;
        public List<Transform> SpawnPoints = new List<Transform>();

        [Tooltip("Bir spawn noktasında item çıkma temel olasılığı (0-1). DensityMultiplier ile çarpılır.")]
        [Range(0f, 1f)] public float BaseSpawnChance = 0.6f;
    }

    [SerializeField] private List<LootZone> zones = new List<LootZone>();
    [SerializeField] private bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
            SpawnAllZones();
    }

    public void SpawnAllZones()
    {
        foreach (var zone in zones)
            SpawnZone(zone);
    }

    public void SpawnZone(LootZone zone)
    {
        if (zone.LootTable == null || zone.LootTable.Items == null || zone.LootTable.Items.Length == 0)
        {
            Debug.LogWarning($"[LootManager] '{zone.ZoneName}' için loot table boş.");
            return;
        }

        float spawnChance = Mathf.Clamp01(zone.BaseSpawnChance * zone.LootTable.DensityMultiplier);

        foreach (var point in zone.SpawnPoints)
        {
            if (point == null) continue;

            // Yoğunluk kontrolü — bu noktada item çıkacak mı?
            if (Random.value > spawnChance) continue;

            SO_ItemData item = RollWeightedItem(zone.LootTable);
            if (item == null) continue;

            SpawnItem(item, point.position, point.rotation);
        }
    }

    /// <summary>RarityWeight'e göre ağırlıklı rastgele seçim.</summary>
    private SO_ItemData RollWeightedItem(SO_LootTableData table)
    {
        int totalWeight = table.GetTotalWeight();
        if (totalWeight <= 0)
        {
            Debug.LogWarning($"[LootManager] '{table.TableName}' toplam ağırlığı 0.");
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        foreach (var item in table.Items)
        {
            if (item == null) continue;
            roll -= Mathf.Max(0, item.RarityWeight);
            if (roll < 0)
                return item;
        }

        return null; // float yuvarlama güvenliği
    }

    private void SpawnItem(SO_ItemData item, Vector3 position, Quaternion rotation)
    {
        if (item.WorldPrefab == null)
        {
            Debug.LogWarning($"[LootManager] '{item.ItemName}' WorldPrefab'ı tanımlı değil — spawn atlandı.");
            return;
        }

        GameObject obj = Instantiate(item.WorldPrefab, position, rotation);

        // WorldPrefab'ın WorldItemPickup'ı doğru SO'ya işaret etsin diye garanti altına al
        var pickup = obj.GetComponent<WorldItemPickup>();
        if (pickup == null)
        {
            Debug.LogWarning($"[LootManager] '{item.ItemName}' prefab'ında WorldItemPickup yok — InventorySystem bunu alamaz!");
        }
    }
}
