using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// v2 — LootSpawnPoint marker tabanlı spawn.
/// Her nokta kendi table'ını ve density'sini getirir (Floor 1/2/3: %30/%60/%90).
/// Weighted random: SO_ItemData.RarityWeight üzerinden (common/uncommon/rare).
/// Spawn edilen item mevcut WorldItemPickup prefab'ı olarak konur.
/// </summary>
public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }

    private readonly List<LootSpawnPoint> spawnPoints = new List<LootSpawnPoint>();

    public event Action<int> OnLootSpawned; // toplam spawn sayısı (debug/UI)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // SpawnPoint'ler aynı frame'de kendini kaydeder; spawn bir frame sonra
        Invoke(nameof(SpawnAll), 0.05f);
    }

    public void RegisterSpawnPoint(LootSpawnPoint point)
    {
        if (!spawnPoints.Contains(point))
            spawnPoints.Add(point);
    }

    public void SpawnAll()
    {
        int spawned = 0;

        foreach (var point in spawnPoints)
        {
            if (point == null || point.LootTable == null) continue;

            // Density: noktanın base chance'i × table'ın multiplier'ı
            float chance = Mathf.Clamp01(point.BaseSpawnChance * point.LootTable.DensityMultiplier);
            if (UnityEngine.Random.value > chance) continue;

            SO_ItemData item = RollWeightedItem(point.LootTable);
            if (item == null) continue;

            if (SpawnItem(item, point.transform.position, point.transform.rotation))
                spawned++;
        }

        OnLootSpawned?.Invoke(spawned);
        Debug.Log($"[LootManager] {spawned}/{spawnPoints.Count} noktada loot spawn edildi.");
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

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (var item in table.Items)
        {
            if (item == null) continue;
            roll -= Mathf.Max(0, item.RarityWeight);
            if (roll < 0)
                return item;
        }

        return null;
    }

    private bool SpawnItem(SO_ItemData item, Vector3 position, Quaternion rotation)
    {
        if (item.WorldPrefab == null)
        {
            Debug.LogWarning($"[LootManager] '{item.ItemName}' WorldPrefab'ı tanımlı değil — spawn atlandı.");
            return false;
        }

        GameObject obj = Instantiate(item.WorldPrefab, position, rotation);

        if (obj.GetComponent<WorldItemPickup>() == null)
        {
            Debug.LogWarning($"[LootManager] '{item.ItemName}' prefab'ında WorldItemPickup yok — InventorySystem bunu alamaz!");
            return false;
        }

        return true;
    }
}
