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
/// Kalıcılık: şu an bellek-içi; save/load (JSON) ayrı görev — bu sınıf
/// ToSaveData()/FromSaveData() hook'larına hazır yapıda.
/// </summary>
public class StashSystem : MonoBehaviour
{
    public static StashSystem Instance { get; private set; }

    [Header("Storage Tiers")]
    [SerializeField] private int tier1Slots = 20;
    [SerializeField] private int tier2Slots = 40;

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

    // ── Save/Load hook'ları (JSON görevi bağlanınca doldurulacak) ──
    // public StashSaveData ToSaveData() { ... }
    // public void FromSaveData(StashSaveData data) { ... }
}
