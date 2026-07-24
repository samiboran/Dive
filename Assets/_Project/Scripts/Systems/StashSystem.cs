using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ana depo (Stash) — dalıştan bağımsız, kalıcı saklama alanı.
/// InventorySystem'den ayrı ama aynı SO_ItemData tipini kullanır.
///
/// UI hook: Slots (IReadOnlyList) + OnStashChanged event'i — görsel
/// tasarım ayrı iş, bu sistem sadece veri expose eder.
///
/// Upgrade: UpgradeStorageTier() slot sayısını artırır (Phase 3 economy
/// bağlanana kadar sabit tier'lar).
///
/// Kalıcılık: JSON (persistentDataPath), her değişimde otomatik kayıt.
/// </summary>
public class StashSystem : MonoBehaviour
{
    public static StashSystem Instance { get; private set; }

    [Header("Storage Tiers")]
    [SerializeField] private int tier1Slots = 20;
    [SerializeField] private int tier2Slots = 40;

    [Header("Persistence")]
    [SerializeField] private ItemDatabase itemDatabase; // save/load çözümlemesi için
    [SerializeField] private string saveFileName = "stash_save.json";
    [SerializeField] private bool autoSaveOnChange = true;
    [SerializeField] private bool loadOnStart = true;

    public enum StorageTier { Tier1, Tier2 }

    [Serializable]
    public class StashSlot
    {
        public SO_ItemData Item;
        public int Count;

        public bool IsEmpty => Item == null;
    }

    private readonly List<StashSlot> slots = new List<StashSlot>();
    private StorageTier currentTier = StorageTier.Tier1;
    private int capacity;

    public IReadOnlyList<StashSlot> Slots => slots;   // ← UI hook noktası
    public StorageTier CurrentTier => currentTier;
    public int Capacity => capacity;
    public int UsedSlots => slots.Count;

    public event Action OnStashChanged;
    public event Action<StorageTier> OnStorageUpgraded;
    public event Action OnStashFull;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Her değişimde otomatik kaydet
        OnStashChanged += PersistIfEnabled;
        OnStorageUpgraded += _ => PersistIfEnabled();

        capacity = tier1Slots;
    }

    public bool TryStore(SO_ItemData item, int count)
    {
        if (item == null || count <= 0) return false;

        // Stack merge
        if (item.IsStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.Item == item && slot.Count < item.MaxStack)
                {
                    int add = Mathf.Min(count, item.MaxStack - slot.Count);
                    slot.Count += add;
                    count -= add;
                    if (count <= 0)
                    {
                        OnStashChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        // Kapasite kontrolü
        if (slots.Count >= capacity)
        {
            OnStashFull?.Invoke();
            Debug.Log("[StashSystem] Depo dolu — upgrade gerekli.");
            return false;
        }

        slots.Add(new StashSlot { Item = item, Count = count });
        OnStashChanged?.Invoke();
        return true;
    }

    /// <summary>Stash'ten oyuncu envanterine çekme (dalış öncesi hazırlık).</summary>
    public bool WithdrawTo(InventorySystem inventory, int stashSlotIndex)
    {
        if (stashSlotIndex >= slots.Count) return false;

        var slot = slots[stashSlotIndex];
        if (slot.IsEmpty) return false;

        if (!inventory.TryAdd(slot.Item, slot.Count)) return false;

        slots.RemoveAt(stashSlotIndex);
        OnStashChanged?.Invoke();
        return true;
    }

    /// <summary>Depo büyütme — Phase 3 economy'ye hazır hook.</summary>
    public bool UpgradeStorageTier()
    {
        if (currentTier == StorageTier.Tier2) return false; // şimdilik max tier

        currentTier = StorageTier.Tier2;
        capacity = tier2Slots;
        OnStorageUpgraded?.Invoke(currentTier);
        Debug.Log($"[StashSystem] Depo büyütüldü → {capacity} slot.");
        return true;
    }

    // ── Save / Load ──────────────────────────────────────────────

    [Serializable]
    private class StashEntry { public string itemName; public int count; }

    [Serializable]
    private class StashSaveData
    {
        public int tier; // 0 = Tier1, 1 = Tier2
        public List<StashEntry> entries = new List<StashEntry>();
    }

    private void Start()
    {
        if (loadOnStart)
            LoadStash();
    }

    /// <summary>Stash değişimlerinde otomatik çağrılır (autoSaveOnChange açıksa).</summary>
    private void PersistIfEnabled()
    {
        if (autoSaveOnChange)
            SaveStash();
    }

    public void SaveStash()
    {
        var data = new StashSaveData { tier = (int)currentTier };
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            data.entries.Add(new StashEntry { itemName = slot.Item.ItemName, count = slot.Count });
        }
        SaveSystem.Save(saveFileName, data);
    }

    public void LoadStash()
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("[StashSystem] ItemDatabase atanmadı — save yüklenemiyor.");
            return;
        }

        var data = SaveSystem.Load<StashSaveData>(saveFileName);

        // Tier'ı geri yükle
        currentTier = (StorageTier)data.tier;
        capacity = currentTier == StorageTier.Tier2 ? tier2Slots : tier1Slots;

        slots.Clear();
        foreach (var entry in data.entries)
        {
            var item = itemDatabase.FindByName(entry.itemName);
            if (item != null)
                slots.Add(new StashSlot { Item = item, Count = entry.count });
        }

        OnStashChanged?.Invoke();
        Debug.Log($"[StashSystem] Save yüklendi: {slots.Count} kalem, tier {currentTier}.");
    }
}
